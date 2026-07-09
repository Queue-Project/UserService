using System.Net;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Auth.Commands.VerifyAccount;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class VerifyEmailCommandHandlerTests
{
    private readonly Mock<ILogger<VerifyEmailCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly VerifyEmailCommandHandler _handler;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    

    public VerifyEmailCommandHandlerTests()
    {
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<VerifyEmailCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new VerifyEmailCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Handler_Should_Verify_Successfully()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomerWithoutEmailVerification();
        user.EmailVerificationCode = "111111";
        user.EmailVerificationCodeExpires = DateTime.UtcNow.AddHours(1);

        await _dbContext.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new VerifyEmailCommand("test@gmail.com", "111111");
        
        
        //Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        
        //Assert
        
        result.ShouldBe(true);
        
    }


    [Fact]
    public async Task Handler_Should_Throw_When_User_Not_Found()
    {
        //Arrange
        var command = new VerifyEmailCommand("test@gmail.com", "111111");
        
        
        //Act
        var result =  _handler.Handle(command, CancellationToken.None);
        
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"User with this email {command.EmailAddress} not found");
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Verification_Code_Not_Exist()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomerWithoutEmailVerification();
        user.EmailVerificationCode = "111111";
        user.EmailVerificationCodeExpires = DateTime.UtcNow.AddHours(1);

        await _dbContext.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new VerifyEmailCommand("test@gmail.com", "111122");
        
        
        //Act
        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain($"Invalid Code");

    }


    [Fact]
    public async Task Handler_Should_Throw_When_Verification_Code_Expired()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomerWithoutEmailVerification();
        user.EmailVerificationCode = "111111";
        user.EmailVerificationCodeExpires = DateTime.MinValue;

        await _dbContext.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new VerifyEmailCommand("test@gmail.com", "111111");
        
        
        //Act
        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain($"Code expired");
    }
    
}