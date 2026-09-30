using System.Threading.Channels;
using Microsoft.Extensions.Options;
using FplPlanner.Domain.Configuration;
using FplPlanner.Repository.Implementation;
using FplPlanner.Repository.Interface;
using FplPlanner.Service.Implementation;
using FplPlanner.Service.Interface;
using FplPlanner.Service.Jobs;
using FplPlanner.Service.Logic;
using FplPlanner.Web.Mapper;
using Quartz;

namespace FplPlanner.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // One open-generic registration covers IRepository<Club>, IRepository<Player>, ...
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IFplDataRepository, FplDataRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IClubService, ClubService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IGameweekService, GameweekService>();
        services.AddScoped<IFixtureService, FixtureService>();
        services.AddScoped<IManagerService, ManagerService>();
        services.AddScoped<ISquadPickService, SquadPickService>();
        services.AddScoped<IPlayerPredictionService, PlayerPredictionService>();
        services.AddScoped<IFplEtlService, FplEtlService>();
        services.AddScoped<IFplManagerImportService, FplManagerImportService>();
        services.AddScoped<IPredictionService, PredictionService>();
        services.AddScoped<ISquadService, SquadService>();
        services.AddScoped<IWeeklyReportService, WeeklyReportService>();
        services.AddScoped<IExcelImportService, ExcelImportService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();
        services.AddScoped<IApiClientService, ApiClientService>();
        services.AddScoped<IInboundSquadEntryService, InboundSquadEntryService>();
        services.AddScoped<InboundSquadEntryProcessor>();
        return services;
    }

    public static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddScoped<ClubMapper>();
        services.AddScoped<PlayerMapper>();
        services.AddScoped<GameweekMapper>();
        services.AddScoped<FixtureMapper>();
        services.AddScoped<ManagerMapper>();
        services.AddScoped<SquadPickMapper>();
        services.AddScoped<PlayerPredictionMapper>();
        services.AddScoped<EtlMapper>();
        services.AddScoped<SquadMapper>();
        services.AddScoped<ReportMapper>();
        services.AddScoped<ApiClientMapper>();
        services.AddScoped<InboundSquadEntryMapper>();
        return services;
    }

    public static IServiceCollection AddFplIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FplApiSettings>(configuration.GetSection(FplApiSettings.SectionName));
        services.AddMemoryCache();

        services.AddHttpClient<IFplApiClient, FplApiClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<FplApiSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseAddress);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FplPlanner/1.0");
        });

        services.AddHostedService<FplEtlBackgroundService>();
        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
        services.AddHostedService<PredictionRecalculationConsumer>();
        return services;
    }

    public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        // Fail at startup, not on the first e-mail, when the SMTP settings are missing or still placeholders.
        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.SmtpHost), "EmailSettings:SmtpHost is required.")
            .Validate(s => s.SmtpPort is > 0 and <= 65535, "EmailSettings:SmtpPort must be a valid port.")
            .Validate(s => EmailAddressValidator.IsValid(s.FromAddress),
                "EmailSettings:FromAddress must be a valid e-mail address.")
            .ValidateOnStart();

        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddSingleton(Channel.CreateUnbounded<QueuedEmail>());
        services.AddSingleton<IEmailQueue, ChannelEmailQueue>();
        services.AddHostedService<EmailBackgroundService>();

        services.AddQuartz(options =>
        {
            var jobKey = new JobKey("weekly-report", "email");
            options.AddJob<QuartzWeeklyReportJob>(o => o.WithIdentity(jobKey));

            options.AddTrigger(o => o
                .ForJob(jobKey)
                .WithIdentity("weekly-report-trigger")
                .WithCronSchedule("0 0 * * * ?") // at the start of every hour
                .WithDescription("Queues weekly reports before the gameweek deadline"));
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        return services;
    }

    public static IServiceCollection AddInboundProcessing(this IServiceCollection services)
    {
        services.AddHostedService<InboundSquadProcessingBackgroundService>();
        return services;
    }
}
