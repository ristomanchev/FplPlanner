using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Dto.Email;
using FplPlanner.Domain.Enums;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Domain.Models;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Interface;
using FplPlanner.Service.Logic;

namespace FplPlanner.Service.Implementation;

public class WeeklyReportService : IWeeklyReportService
{
    private const int TransferHorizon = 5;
    private const int MaxTransfers = 2;

    private readonly ISquadService _squadService;
    private readonly IGameweekService _gameweekService;
    private readonly IRepository<Manager> _managerRepository;
    private readonly IEmailQueue _emailQueue;
    private readonly IExcelExportService _excelExportService;
    private readonly ILogger<WeeklyReportService> _logger;

    public WeeklyReportService(ISquadService squadService,
        IGameweekService gameweekService,
        IRepository<Manager> managerRepository,
        IEmailQueue emailQueue,
        IExcelExportService excelExportService,
        ILogger<WeeklyReportService> logger)
    {
        _squadService = squadService;
        _gameweekService = gameweekService;
        _managerRepository = managerRepository;
        _emailQueue = emailQueue;
        _excelExportService = excelExportService;
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

        if (report.Deadline <= DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                $"The deadline for gameweek {report.GameweekNumber} has passed; a report would be of no use.");
        }

        // Queued, not sent here: EmailBackgroundService delivers it and only then marks the gameweek as reported,
        // so a report that could not be delivered is tried again by the next scheduled run.
        var gameweekNumber = report.GameweekNumber;
        await _emailQueue.EnqueueAsync(new EmailMessage
        {
            To = report.Email,
            ToName = report.ManagerName,
            Subject = $"FPL Planner — {report.TeamName}, gameweek {gameweekNumber}",
            HtmlBody = WeeklyReportHtmlBuilder.Build(report),
            Attachments =
            [
                new EmailAttachment
                {
                    FileName = $"fpl-predictions-gw{gameweekNumber}.xlsx",
                    Content = await _excelExportService.ExportPredictionsToExcel(TransferHorizon),
                    ContentType = EmailAttachment.ExcelContentType
                }
            ]
        }, onSent: (services, _) => MarkReportedAsync(services, managerId, gameweekNumber),
            cancellationToken: cancellationToken);

        return report;
    }

    private static async Task MarkReportedAsync(IServiceProvider services, Guid managerId, int gameweekNumber)
    {
        var managerRepository = services.GetRequiredService<IRepository<Manager>>();
        var manager = await managerRepository.GetAsync(selector: m => m, predicate: m => m.Id == managerId);
        if (manager == null || manager.LastReportedGameweek >= gameweekNumber)
        {
            return;
        }

        manager.LastReportedGameweek = gameweekNumber;
        await managerRepository.UpdateAsync(manager);
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
