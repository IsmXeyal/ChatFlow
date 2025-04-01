using ChatFlow.Application.Repositories.Reads;
using ChatFlow.Application.Repositories.Writes;
using ChatFlow.Persistence.DbContexts;
using ChatFlow.Persistence.Repositories.AppUser;
using ChatFlow.Persistence.Repositories.AppUserRepo;
using ChatFlow.Persistence.Repositories.GroupRepo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatFlow.Persistence;

public static class ServiceRegistrations
{
    public static IServiceCollection AddPersistenceRegister(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            options.UseNpgsql(configuration.GetConnectionString("Default"));
        });

        // Reads
        services.AddScoped<IAppUserReadRepository, AppUserReadRepository>();
        services.AddScoped<IGroupReadRepository, GroupReadRepository>();

        // Writes
        services.AddScoped<IAppUserWriteRepository, AppUserWriteRepository>();
        services.AddScoped<IGroupWriteRepository, GroupWriteRepository>();

        return services;
    }
}
