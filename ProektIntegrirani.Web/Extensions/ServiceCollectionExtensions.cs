using ProektIntegrirani.Repository.Implementation;
using ProektIntegrirani.Repository.Interface;

namespace ProektIntegrirani.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // One open-generic registration covers IRepository<Club>, IRepository<Player>, ...
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        return services;
    }
}
