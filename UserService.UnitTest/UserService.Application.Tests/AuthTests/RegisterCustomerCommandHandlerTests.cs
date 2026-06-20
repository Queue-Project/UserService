using System.Net;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using QNotificationService.Contracts.NotificationEvents;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Auth.Commands.RegisterCustomer;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class RegisterCustomerCommandHandlerTests
{
    private readonly Mock<ILogger<RegisterCustomerCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPasswordHasher<UserEntity>> _mockPasswordHasher;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly RegisterCustomerCommandHandler _handler;


    public RegisterCustomerCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<RegisterCustomerCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockPasswordHasher = new Mock<IPasswordHasher<UserEntity>>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _handler = new RegisterCustomerCommandHandler(_mockLogger.Object, _dbContext, _mockPasswordHasher.Object,
            _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_Registered_Customer_Successfully()
    {
        //Arrange
        var customer = new RegisterCustomerCommand(
            "customer@test.com",
            "Test.1234",
            "Test Firstname",
            "Test Lastname",
            "+992923357686");

        var hashedPassword = "Hashed1234";
        _mockPasswordHasher.Setup(s => s.HashPassword(It.IsAny<UserEntity>(), customer.Password))
            .Returns(hashedPassword);
        
        
        //Act
        var result = await _handler.Handle(customer, CancellationToken.None);
        
        
        //Assert
        
        result.Id.ShouldBe(1);
        result.Roles.ShouldBe(UserRoles.Customer);
        result.EmailAddress.ShouldBe(customer.EmailAddress);
        result.PasswordHash.ShouldBe(hashedPassword);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Email_Exists()
    {
        
        //Arrange
        var customer = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Users.AddAsync(customer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        
        var command = new RegisterCustomerCommand(
            customer.EmailAddress,
            "Test.1234",
            "Test Firstname",
            "Test Lastname",
            "+992923357686");
        
        
        //Act
        var result =  _handler.Handle(command, CancellationToken.None);
        
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Email address already exists");
        
    }
    
    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
        //Arrange
        var customer = new RegisterCustomerCommand(
            "customer@test.com",
            "Test.1234",
            "Test Firstname",
            "Test Lastname",
            "+992923357686");

        var hashedPassword = "Hashed1234";
        _mockPasswordHasher.Setup(s => s.HashPassword(It.IsAny<UserEntity>(), customer.Password))
            .Returns(hashedPassword);
        
        
        //Act
        var result = await _handler.Handle(customer, CancellationToken.None);
        
        //Assert

        _mockPublishEndpoint.Verify(s => s.Publish(It.IsAny<SendNotificationEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}