using System.Net;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QNotificationService.Contracts.NotificationEvents;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Auth.Commands.ResendCode;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class ResendVerificationCommandHandlerTests
{
    private readonly Mock<ILogger<ResendVerificationCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly ResendVerificationCommandHandler _handler;


    public ResendVerificationCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<ResendVerificationCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _handler = new ResendVerificationCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object);
    }


    [Fact]
    public async Task Handler_Should_Return_Resend_Code_Successfully()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomerWithoutEmailVerification();
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new ResendVerificationCodeCommand("test@gmail.com");


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);


        //Assert

        result.ShouldBe(true);
    }

    [Fact]
    public async Task Handler_Should_Throw_User_Not_Found()
    {
        var command = new ResendVerificationCodeCommand("test@gmail.com");


        //Act
        var result = _handler.Handle(command, CancellationToken.None);


        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"User with Email {command.EmailAddress} not found");
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Email_Already_Verified()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new ResendVerificationCodeCommand("test@gmail.com");


        //Act
        var result = _handler.Handle(command, CancellationToken.None);


        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Email already verified");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Last_Code_Sent_HasValue()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomerWithLastCodeSent();
        var lastCodeSent = user.LastCodeSentAt.HasValue ? user.LastCodeSentAt.Value : DateTime.UtcNow;
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new ResendVerificationCodeCommand("test@gmail.com");


        //Act
        var result = _handler.Handle(command, CancellationToken.None);


        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Please request after 1 minute");
    }

    

    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomerWithoutEmailVerification();
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new ResendVerificationCodeCommand("test@gmail.com");


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);


        //Assert

        _mockPublishEndpoint.Verify(s => s.Publish(It.IsAny<SendNotificationEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}