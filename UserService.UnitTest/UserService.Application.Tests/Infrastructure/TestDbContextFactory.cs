using Microsoft.EntityFrameworkCore;
using QUserService.Infrastructure.Persistence.Database;

namespace UserService.UnitTest.UserService.Application.Tests.Infrastructure;

public static class TestDbContextFactory
{
    public static UserServiceDbContext Create()
    {
        var options = new DbContextOptionsBuilder<UserServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new UserServiceDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}