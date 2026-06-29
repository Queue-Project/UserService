using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.UseCases.Auth.Commands.Logout;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class LogoutCommandHandlerTests
{
    private readonly Mock<ILogger<LogoutCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<LogoutCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new LogoutCommandHandler(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task Handler_Should_Logout_Successfully()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        var logout = TestDataSeeder.CreateToken();

        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.RefreshTokens.AddAsync(logout, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new LogoutCommand("generatedToken");

        //Act

        var result = await _handler.Handle(command, CancellationToken.None);
        //Assert
        
        result.ShouldBe(true);
    }

    [Fact]
    public async Task Handler_Should_Logout_Return_False()
    {
        //Arrange
        var command = new LogoutCommand("generatedToken");

        //Act

        var result = await _handler.Handle(command, CancellationToken.None);
        //Assert
        
        result.ShouldBe(false);
    }
    
}