using System.Net;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Auth.Commands.ResetPassword;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<ILogger<ResetPasswordCommandHandler>> _mockLogger;
    private readonly Mock<IPasswordHasher<UserEntity>> _mockPasswordHasher;
    private readonly UserServiceDbContext _dbContext;
    private readonly ResetPasswordCommandHandler _handler;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;

    public ResetPasswordCommandHandlerTests()
    {
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<ResetPasswordCommandHandler>>();
        _mockPasswordHasher = new Mock<IPasswordHasher<UserEntity>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new ResetPasswordCommandHandler(_mockLogger.Object, _dbContext, _mockPasswordHasher.Object, _mockPublishEndpoint.Object);
    }


    [Fact]
    public async Task Handler_Should_Return_ResetPassword_Successfully()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        user.PasswordResetCode = "666666";
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);

        await _dbContext.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var hashedPassword = "hashed1234";
        
        _mockPasswordHasher.Setup(s => s.HashPassword(It.IsAny<UserEntity>(), user.PasswordHash))
            .Returns(hashedPassword);


        var command = new ResetPasswordCommand(
            user.EmailAddress,
            "666666",
            "TestNewPassword.1234",
            "TestNewPassword.1234");

        //Act
        var result = await _handler.Handle(command, CancellationToken.None);


        //Assert
        result.ShouldBe(true);
    }


    [Fact]
    public async Task Handler_Should_Throw_When_User_Not_Found()
    {
        //Arrange
        
        var command = new ResetPasswordCommand(
            "test@gmail.com",
            "666666",
            "TestNewPassword.1234",
            "TestNewPassword.1234");

        //Act
        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"User with {command.EmailAddress} not found");
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Reset_Code_Not_Found()
    {
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        


        var command = new ResetPasswordCommand(
            user.EmailAddress,
            "666666",
            "TestNewPassword.1234",
            "TestNewPassword.1234");

        //Act
        var result =  _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Reset code not found");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_ResetCode_IsNot_Request_Code()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        user.PasswordResetCode = "666666";
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);

        await _dbContext.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        

        var command = new ResetPasswordCommand(
            user.EmailAddress,
            "666665",
            "TestNewPassword.1234",
            "TestNewPassword.1234");

        //Act
        var result =  _handler.Handle(command, CancellationToken.None);


        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Invalid reset code");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_ResetCode_Expired()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        user.PasswordResetCode = "666666";
        user.PasswordResetExpiry = DateTime.MinValue;

        await _dbContext.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        

        var command = new ResetPasswordCommand(
            user.EmailAddress,
            "666666",
            "TestNewPassword.1234",
            "TestNewPassword.1234");

        //Act
        var result =  _handler.Handle(command, CancellationToken.None);


        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Reset code expired");

    }

    [Fact]
    public async Task Handler_Should_Throw_When_NewCode_Not_Match_With_ConfirmCode()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        user.PasswordResetCode = "666666";
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);

        await _dbContext.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        

        var command = new ResetPasswordCommand(
            user.EmailAddress,
            "666666",
            "TestNewPassword.1234",
            "TestNewPassword.4321");

        //Act
        var result =  _handler.Handle(command, CancellationToken.None);


        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Passwords do not match");
    }
}