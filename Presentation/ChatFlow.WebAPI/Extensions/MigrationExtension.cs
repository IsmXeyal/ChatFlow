using ChatFlow.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ChatFlow.WebAPI.Extensions;

public static class MigrationExtension
{
    public static void ApplyMigration(this IApplicationBuilder app)
    {
        using IServiceScope serviceScope = app.ApplicationServices.CreateScope();
        using AppDbContext dbContext = serviceScope.ServiceProvider.GetService<AppDbContext>()!;

        dbContext.Database.Migrate();
    }
}
