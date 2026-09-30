using System.Threading.Channels;
using Microsoft.Extensions.Options;
using ProektIntegrirani.Domain.Configuration;
using ProektIntegrirani.Domain.Dto.Email;
using ProektIntegrirani.Repository.Implementation;
using ProektIntegrirani.Repository.Interface;
using ProektIntegrirani.Service.Implementation;
using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Service.Jobs;
using ProektIntegrirani.Web.Mapper;

namespace ProektIntegrirani.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // One open-generic registration covers IRepository<Club>, IRepository<Player>, ...
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
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

        services.AddHttpClient<IFplApiClient, FplApiClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<FplApiSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseAddress);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FplPlanner/1.0");
        });

        services.AddHostedService<FplSyncBackgroundService>();
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
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddSingleton(Channel.CreateUnbounded<EmailMessage>());
        services.AddSingleton<IEmailQueue, ChannelEmailQueue>();
        services.AddHostedService<EmailBackgroundService>();
        services.AddHostedService<WeeklyReportBackgroundService>();
        return services;
    }

    public static IServiceCollection AddInboundProcessing(this IServiceCollection services)
    {
        services.AddHostedService<InboundSquadProcessingBackgroundService>();
        return services;
    }
}
