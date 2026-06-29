using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QNotificationService.Contracts.NotificationEvents;
using QUserService.Application.UseCases.Auth.Commands.ForgotPassword;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<ILogger<ForgotPasswordCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<ForgotPasswordCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _handler = new ForgotPasswordCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Handler_Should_Forgot_Password_Successfully()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new ForgotPasswordCommand("test@gmail.com");


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);


        //Assert
        result.ShouldBe(true);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_User_Not_Found()
    {
        var command = new ForgotPasswordCommand("test@gmail.com");


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);


        //Assert
        result.ShouldBe(false);
    }


    [Fact]
    public async Task Handler_Should_Publish_Notification_Event()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new ForgotPasswordCommand("test@gmail.com");


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);


        //Assert

        _mockPublishEndpoint.Verify(s => s.Publish(It.IsAny<SendNotificationEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}