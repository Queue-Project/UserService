
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.Auth.Queries.Login;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class LoginQueryHandlerTests
{
    private readonly Mock<ILogger<LoginQueryHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPasswordHasher<UserEntity>> _mockPasswordHasher;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly LoginQueryHandler _handler;
    
    
    public LoginQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<LoginQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockPasswordHasher = new Mock<IPasswordHasher<UserEntity>>();
        _mockTokenService = new Mock<ITokenService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _handler = new LoginQueryHandler(_mockLogger.Object, _dbContext, _mockPasswordHasher.Object,
            _mockConfiguration.Object, _mockTokenService.Object);
    }

    [Fact]
    public async Task Handler_Should_Login_Successfully()
    {
        //Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new LoginQuery("test@gmail.com", "Password-hash-1234");
        
        
        _mockPasswordHasher.Setup(s =>
                s.VerifyHashedPassword(It.IsAny<UserEntity>(), user.PasswordHash, query.Password))
            .Returns(PasswordVerificationResult.Success);

        var accessTokenMinutes = "60";
        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.Value).Returns(accessTokenMinutes);
        _mockConfiguration.Setup(s => s["Jwt:AccessTokenMinutes"]).Returns(accessTokenMinutes);
        
        
        var expectedAccessToken = "Test.Access.Token.123";
        var accessExpires = DateTime.UtcNow.AddMinutes(int.Parse(accessTokenMinutes));
        
        _mockTokenService.Setup(s =>
            s.GenerateAccessToken(user.Id, user.EmailAddress, user.Roles.ToString(), It.IsAny<DateTime>()))
            .Returns(expectedAccessToken);

        var expectedToken = "token.1234";
        var expectedExpireTime = DateTime.UtcNow.AddMinutes(60);

        _mockTokenService.Setup(s => s.GenerateRefreshToken())
            .Returns((expectedToken, expectedExpireTime));
        
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        
        result.EmailAddress.ShouldBe(query.EmailAddress);
        result.Role.ShouldBe(user.Roles.ToString());
        result.UserId.ShouldBe(user.Id);
        result.AccessToken.ShouldBe(expectedAccessToken);
        result.RefreshToken.ShouldBe(expectedToken);
    }
    
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_User_Not_Found()
    {
        // Arrange
        var query = new LoginQuery("nonexistent@test.com", "Password-hash-1234");

        // Act 
        var result = _handler.Handle(query, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Message.ShouldContain("UserEntity not found");
        
    }
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Password_Is_Invalid()
    {
        // Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new LoginQuery(user.EmailAddress, "WrongPassword123");

        _mockPasswordHasher
            .Setup(s => s.VerifyHashedPassword(user, user.PasswordHash, query.Password))
            .Returns(PasswordVerificationResult.Failed);

        // Act
        var result = _handler.Handle(query, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Message.ShouldContain("Invalid password");
        
    }
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Email_Not_Verified()
    {
        // Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        user.IsEmailVerified = false;
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new LoginQuery(user.EmailAddress, "Password-hash-1234");

        _mockPasswordHasher
            .Setup(s => s.VerifyHashedPassword(user, user.PasswordHash, query.Password))
            .Returns(PasswordVerificationResult.Success);

        // Act
        var result = _handler.Handle(query, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Message.ShouldContain("Email not verified");
        
    }

}