using Microsoft.Extensions.Options;
using ProektIntegrirani.Domain.Configuration;
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
}
