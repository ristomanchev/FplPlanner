using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Domain.Exceptions;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Repository.Interface;
using ProektIntegrirani.Service.Interface;

namespace ProektIntegrirani.Service.Implementation;

// Turns Pending inbound entries into saved squads (Completed) or records why they could not be saved (Failed).
public class InboundSquadEntryProcessor
{
    private const int BatchSize = 10;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IRepository<InboundSquadEntry> _repository;
    private readonly IRepository<Manager> _managerRepository;
    private readonly IPlayerService _playerService;
    private readonly ISquadService _squadService;
    private readonly ILogger<InboundSquadEntryProcessor> _logger;

    public InboundSquadEntryProcessor(IRepository<InboundSquadEntry> repository,
        IRepository<Manager> managerRepository,
        IPlayerService playerService,
        ISquadService squadService,
        ILogger<InboundSquadEntryProcessor> logger)
    {
        _repository = repository;
        _managerRepository = managerRepository;
        _playerService = playerService;
        _squadService = squadService;
        _logger = logger;
    }

    public async Task<int> ProcessPendingEntriesAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _repository.GetAllAsync(
            selector: x => x,
            predicate: e => e.Status == InboundSquadStatus.Pending,
            orderBy: q => q.OrderBy(e => e.ReceivedAt),
            take: BatchSize);

        var processed = 0;
        foreach (var entry in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                entry.Status = InboundSquadStatus.Processing;
                await _repository.UpdateAsync(entry);

                entry.ManagerId = await ProcessEntryAsync(entry);
                entry.Status = InboundSquadStatus.Completed;
                entry.ProcessedAt = DateTime.UtcNow;

                _logger.LogInformation("Processed inbound squad {Id} for manager {ManagerId}", entry.Id,
                    entry.ManagerId);
            }
            catch (Exception ex)
            {
                entry.Status = InboundSquadStatus.Failed;
                entry.ErrorMessage = Describe(ex);
                entry.ProcessedAt = DateTime.UtcNow;

                _logger.LogWarning("Failed to process inbound squad {Id}: {Error}", entry.Id, entry.ErrorMessage);
            }

            await _repository.UpdateAsync(entry);
            processed++;
        }

        return processed;
    }

    private async Task<Guid> ProcessEntryAsync(InboundSquadEntry entry)
    {
        var request = JsonSerializer.Deserialize<InboundSquadRequest>(entry.RawPayload, JsonOptions)
                      ?? throw new BusinessRuleException("The payload is empty.");

        var manager = await _managerRepository.GetAsync(
                          selector: m => m,
                          predicate: m => m.FplEntryId == request.FplEntryId)
                      ?? throw new BusinessRuleException(
                          $"Manager with FPL entry id {request.FplEntryId} does not exist.");

        var fplIds = request.Picks.Select(p => p.FplId).Distinct().ToList();
        var playerIdsByFplId = (await _playerService.GetAllByFplIdsInAsync(fplIds))
            .ToDictionary(p => p.FplId, p => p.Id);

        var unknown = fplIds.Where(id => !playerIdsByFplId.ContainsKey(id)).ToList();
        if (unknown.Count > 0)
        {
            throw new BusinessRuleException($"Unknown player FPL ids: {string.Join(", ", unknown)}.");
        }

        var picks = request.Picks.Select(p => new SquadPickInputDto
        {
            PlayerId = playerIdsByFplId[p.FplId],
            SquadPosition = p.SquadPosition,
            IsCaptain = p.IsCaptain,
            IsViceCaptain = p.IsViceCaptain
        }).ToList();

        // Same FPL rules as every other way of saving a squad.
        await _squadService.SaveSquadAsync(manager.Id, request.GameweekNumber, picks);
        return manager.Id;
    }

    // Squad rule violations are listed individually so the external system can fix all of them at once.
    private static string Describe(Exception ex)
    {
        return ex is BusinessRuleException { Errors.Count: > 0 } rule
            ? $"{rule.Message} {string.Join(" ", rule.Errors)}"
            : ex.Message;
    }
}
