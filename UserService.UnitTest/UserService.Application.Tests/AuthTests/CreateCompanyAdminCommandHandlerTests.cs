using System.Net;
using BranchService.Contracts.Events.Enums;
using BranchService.Contracts.Interfaces;
using BranchService.Contracts.Requests;
using BranchService.Contracts.Responses;
using MagicOnion;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using QNotificationService.Contracts.NotificationEvents;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Auth.Commands.CreateCompanyAdmin;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class CreateCompanyAdminCommandHandlerTests
{
    private readonly Mock<ILogger<CreateCompanyAdminCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPasswordHasher<UserEntity>> _mockPasswordHasher;
    private readonly Mock<IBranchService> _mockBranchService;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly CreateCompanyAdminCommandHandler _handler;

    public CreateCompanyAdminCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CreateCompanyAdminCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockPasswordHasher = new Mock<IPasswordHasher<UserEntity>>();
        _mockBranchService = new Mock<IBranchService>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _handler = new CreateCompanyAdminCommandHandler(
            _mockLogger.Object, 
            _dbContext, 
            _mockPasswordHasher.Object,
            _mockBranchService.Object, 
            _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Handler_Should_Create_CompanyAdmin_Successfully()
    {
        // Arrange
        var systemAdmin = TestDataSeeder.CreateSystemAdmin();
        await _dbContext.Users.AddAsync(systemAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateCompanyAdminCommand(
            1,
            "companyAdmin@test.com",
            "CompanyAdminTest.1234",
            "Test Firstname",
            "Test Lastname",
            "CompanyAdmin",
            "+992923324567",
            1);
        
        var companyExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyName = "Test Company",
            CompanyCategory = CompanyCategory.Beauty,
            IsValid = true,
            ErrorMessage = null
        };
        
        var hashedPassword = "HashedPassword123";
        _mockPasswordHasher
            .Setup(s => s.HashPassword(It.IsAny<UserEntity>(), command.Password))
            .Returns(hashedPassword);

        _mockBranchService
            .Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));

        _mockPublishEndpoint
            .Setup(s => s.Publish(It.IsAny<SendNotificationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeGreaterThan(0);
        result.EmailAddress.ShouldBe(command.EmailAddress);
        result.Roles.ShouldBe(UserRoles.CompanyAdmin);
        result.PasswordHash.ShouldBe(hashedPassword); 
        result.EmailVerificationCode.ShouldNotBeNull();
        
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Creator_Not_Found()
    {
        // Arrange
        var command = new CreateCompanyAdminCommand(
            1,
            "companyAdmin@test.com",
            "CompanyAdminTest.1234",
            "Test Firstname",
            "Test Lastname",
            "CompanyAdmin",
            "+992923324567",
            999); 

        // Act 
        
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("Creator not found");

       
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Creator_Is_Not_SystemAdmin()
    {
        // Arrange
        var employeeUser = TestDataSeeder.CreateUserEmployeeRole(); 
        await _dbContext.Users.AddAsync(employeeUser, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateCompanyAdminCommand(
            employeeUser.Id,
            "companyAdmin@test.com",
            "CompanyAdminTest.1234",
            "Test Firstname",
            "Test Lastname",
            "CompanyAdmin",
            "+992923324567",
            employeeUser.Id);

        // Act 

        var result = _handler.Handle(command, CancellationToken.None);

        
        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Not allowed to create barbershop admin");

       
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Email_Already_Exists()
    {
        // Arrange
        var systemAdmin = TestDataSeeder.CreateSystemAdmin();
        await _dbContext.Users.AddAsync(systemAdmin, CancellationToken.None);
        
        var existingUser = TestDataSeeder.CreateUserEmployeeRole();
        existingUser.EmailAddress = "companyAdmin@test.com";
        await _dbContext.Users.AddAsync(existingUser, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateCompanyAdminCommand(
            systemAdmin.Id,
            "companyAdmin@test.com", 
            "CompanyAdminTest.1234",
            "Test Firstname",
            "Test Lastname",
            "CompanyAdmin",
            "+992923324567",
            1);

        // Act 
        
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Email already exists");

      
    }
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Phone_Number_Already_Exists()
    {
        // Arrange
        var systemAdmin = TestDataSeeder.CreateSystemAdmin();
        await _dbContext.Users.AddAsync(systemAdmin, CancellationToken.None);

        var existingUser = TestDataSeeder.CreateEmployee();
        existingUser.PhoneNumber = "+992923324567";
        await _dbContext.Employees.AddAsync(existingUser, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateCompanyAdminCommand(
            systemAdmin.Id,
            "companyAdmin@test.com", 
            "CompanyAdminTest.1234",
            "Test Firstname",
            "Test Lastname",
            "CompanyAdmin",
            "+992923324567",
            1);

        // Act 
        
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Phone number already exists");

      
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Company_Not_Found()
    {
        // Arrange
        var systemAdmin = TestDataSeeder.CreateSystemAdmin();
        await _dbContext.Users.AddAsync(systemAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateCompanyAdminCommand(
            99,
            "companyAdmin@test.com",
            "CompanyAdminTest.1234",
            "Test Firstname",
            "Test Lastname",
            "CompanyAdmin",
            "+992923324567",
            systemAdmin.Id); 

        var companyExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 99,
            IsValid = false,
            ErrorMessage = "Company not found",
            CompanyName = null
        };

        _mockBranchService
            .Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));

        // Act 
        
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("Company not found");

       
    }
    
    
    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
        
        // Arrange
        var systemAdmin = TestDataSeeder.CreateSystemAdmin();
        await _dbContext.Users.AddAsync(systemAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateCompanyAdminCommand(
            1,
            "companyAdmin@test.com",
            "CompanyAdminTest.1234",
            "Test Firstname",
            "Test Lastname",
            "CompanyAdmin",
            "+992923324567",
            1);
        
        var companyExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyName = "Test Company",
            CompanyCategory = CompanyCategory.Beauty,
            IsValid = true,
            ErrorMessage = null
        };
        
        var hashedPassword = "HashedPassword123";
        _mockPasswordHasher
            .Setup(s => s.HashPassword(It.IsAny<UserEntity>(), command.Password))
            .Returns(hashedPassword);

        _mockBranchService
            .Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));

        _mockPublishEndpoint
            .Setup(s => s.Publish(It.IsAny<SendNotificationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
    

        result.ShouldNotBeNull();
        _mockPublishEndpoint.Verify(x=>x.Publish(It.IsAny<SendNotificationEvent>(), It.IsAny<CancellationToken>()));
        
    }
}