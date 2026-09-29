using ProektIntegrirani.Repository.Implementation;
using ProektIntegrirani.Repository.Interface;
using ProektIntegrirani.Service.Implementation;
using ProektIntegrirani.Service.Interface;
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
        return services;
    }
}
