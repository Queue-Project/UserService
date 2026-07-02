using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QUserService.Infrastructure.Persistence.Database;

public class UserServiceContextFactory: IDesignTimeDbContextFactory<UserServiceDbContext>
{
    public UserServiceDbContext CreateDbContext(string[] args)
    {
        var optionBuilder = new DbContextOptionsBuilder<UserServiceDbContext>();

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../QUserService.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        

        optionBuilder.UseNpgsql(connectionString);
        return new UserServiceDbContext(optionBuilder.Options);
    }
}