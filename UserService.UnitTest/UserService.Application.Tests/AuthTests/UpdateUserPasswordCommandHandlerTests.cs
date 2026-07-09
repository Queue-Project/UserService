using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.Auth.Commands.UpdateUserPassword;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class UpdateUserPasswordCommandHandlerTests
{
    private readonly Mock<ILogger<UpdateUserPasswordCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IPasswordHasher<UserEntity>> _mockPasswordHasher;
    private readonly UpdateUserPasswordCommandHandler _handler;

    public UpdateUserPasswordCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<UpdateUserPasswordCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockPasswordHasher = new Mock<IPasswordHasher<UserEntity>>();
        _handler = new UpdateUserPasswordCommandHandler(_mockLogger.Object, _dbContext, _mockCurrentUserService.Object,
            _mockPasswordHasher.Object);
    }

    [Fact]
    public async Task Handler_Should_Update_Password_Successfully()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new UpdateUserPasswordCommand(
            "newTestPassword.1234",
            "Password-hash-1234");

        _mockCurrentUserService.Setup(s => s.GetCurrentUserAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(s =>
                s.VerifyHashedPassword(It.IsAny<UserEntity>(), user.PasswordHash, command.OldPassword))
            .Returns(PasswordVerificationResult.Success);

        var hashedPassword = "hashedPassword";

        _mockPasswordHasher.Setup(s => s.HashPassword(It.IsAny<UserEntity>(), command.NewPassword))
            .Returns(hashedPassword);
        
        
        //Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        //Assert
        
        result.ShouldBe(true);
        
    }

    [Fact]
    public async Task Handler_Should_Throw_When_User_Not_Found()
    {
        //Arrange
        var command = new UpdateUserPasswordCommand(
            "newTestPassword.1234",
            "Password-hash-1234");

        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "User not found");
        _mockCurrentUserService.Setup(s => s.GetCurrentUserAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);
        
        //Act
        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("User not found");
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Password_Verification_Result_Failed()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new UpdateUserPasswordCommand(
            "newTestPassword.1234",
            "Password-hash-4321");

        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.BadRequest, "Current Password is incorrect");
        
        
        _mockCurrentUserService.Setup(s => s.GetCurrentUserAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(s =>
                s.VerifyHashedPassword(It.IsAny<UserEntity>(), user.PasswordHash, command.OldPassword))
            .Throws(expectedResponse);

        
        //Act
        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Current Password is incorrect");
        
    }
}