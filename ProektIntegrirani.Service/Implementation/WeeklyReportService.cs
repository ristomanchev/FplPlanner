using Microsoft.Extensions.Logging;
using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Dto.Email;
using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Domain.Exceptions;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Repository.Interface;
using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Service.Logic;

namespace ProektIntegrirani.Service.Implementation;

public class WeeklyReportService : IWeeklyReportService
{
    private const int TransferHorizon = 5;
    private const int MaxTransfers = 2;

    private readonly ISquadService _squadService;
    private readonly IGameweekService _gameweekService;
    private readonly IRepository<Manager> _managerRepository;
    private readonly IEmailQueue _emailQueue;
    private readonly ILogger<WeeklyReportService> _logger;

    public WeeklyReportService(ISquadService squadService,
        IGameweekService gameweekService,
        IRepository<Manager> managerRepository,
        IEmailQueue emailQueue,
        ILogger<WeeklyReportService> logger)
    {
        _squadService = squadService;
        _gameweekService = gameweekService;
        _managerRepository = managerRepository;
        _emailQueue = emailQueue;
        _logger = logger;
    }

    public async Task<WeeklyReportDto> BuildAsync(Guid managerId)
    {
        var manager = await _managerRepository.GetAsync(selector: m => m, predicate: m => m.Id == managerId)
                      ?? throw new NotFoundException(nameof(Manager), managerId);
        var nextGameweek = await _gameweekService.GetNextAsync();

        var squad = await _squadService.GetSquadAsync(managerId, gameweekNumber: null);
        var lineup = await _squadService.GetBestLineupAsync(managerId);
        var transfers = await _squadService.GetTransferAdviceAsync(managerId, TransferHorizon, MaxTransfers);

        return new WeeklyReportDto
        {
            ManagerId = manager.Id,
            TeamName = manager.TeamName,
            ManagerName = manager.ManagerName,
            Email = manager.Email,
            GameweekNumber = nextGameweek.Number,
            Deadline = nextGameweek.Deadline,
            Lineup = lineup,
            TransferAdvice = transfers,
            FlaggedPlayers = squad.Members.Where(m => m.Status != PlayerStatus.Available).ToList()
        };
    }

    public async Task<string> RenderHtmlAsync(Guid managerId)
    {
        return WeeklyReportHtmlBuilder.Build(await BuildAsync(managerId));
    }

    public async Task<WeeklyReportDto> SendAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        var report = await BuildAsync(managerId);
        if (string.IsNullOrWhiteSpace(report.Email))
        {
            throw new BusinessRuleException($"{report.TeamName} has no e-mail address.");
        }

        // Queued, not sent here: EmailBackgroundService delivers it.
        await _emailQueue.EnqueueAsync(new EmailMessage
        {
            To = report.Email,
            ToName = report.ManagerName,
            Subject = $"FPL Planner — {report.TeamName}, gameweek {report.GameweekNumber}",
            HtmlBody = WeeklyReportHtmlBuilder.Build(report)
        }, cancellationToken);

        var manager = await _managerRepository.GetAsync(selector: m => m, predicate: m => m.Id == managerId);
        manager!.LastReportedGameweek = report.GameweekNumber;
        await _managerRepository.UpdateAsync(manager);

        return report;
    }

    public async Task<int> SendDueReportsAsync(CancellationToken cancellationToken = default)
    {
        var nextGameweek = await _gameweekService.GetNextAsync();
        var managerIds = await _managerRepository.GetAllAsync(
            selector: m => m.Id,
            predicate: m => m.Email != null
                            && (m.LastReportedGameweek == null || m.LastReportedGameweek < nextGameweek.Number));

        var sent = 0;
        foreach (var managerId in managerIds)
        {
            try
            {
                await SendAsync(managerId, cancellationToken);
                sent++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // One manager without a squad must not stop the reports for everyone else.
                _logger.LogWarning(ex, "Weekly report for manager {ManagerId} was not sent.", managerId);
            }
        }

        return sent;
    }
}
