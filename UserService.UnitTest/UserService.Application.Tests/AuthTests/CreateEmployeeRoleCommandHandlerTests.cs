using System.Net;
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
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.Auth.Commands.CreateEmployee;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class CreateEmployeeRoleCommandHandlerTests
{
    private readonly Mock<ILogger<CreateEmployeeRoleCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPasswordHasher<UserEntity>> _mockPasswordHasher;
    private readonly Mock<IBranchService> _mockBranchService;
    private readonly Mock<ICurrentUserService> _mockCurrentService;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly CreateEmployeeRoleCommandHandler _handler;

    public CreateEmployeeRoleCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CreateEmployeeRoleCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockPasswordHasher = new Mock<IPasswordHasher<UserEntity>>();
        _mockBranchService = new Mock<IBranchService>();
        _mockCurrentService = new Mock<ICurrentUserService>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _handler = new CreateEmployeeRoleCommandHandler(_mockLogger.Object, _dbContext, _mockPasswordHasher.Object,
            _mockCurrentService.Object, _mockBranchService.Object, _mockPublishEndpoint.Object);
    }


    [Fact]
    public async Task Handler_Should_Create_Employee_Successfully()
    {
        var userCompanyAdmin = TestDataSeeder.CreateUserCompanyAdmin();
        var employeeCompanyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        await _dbContext.Employees.AddAsync(employeeCompanyAdmin, CancellationToken.None);
        await _dbContext.Users.AddAsync(userCompanyAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new CreateEmployeeRoleCommand(
            1,
            1,
            "employee@test.com",
            "EmployeeTest.1234",
            "Test Firstname",
            "Test Lastname",
            "Barber",
            "+992923324252",
            userCompanyAdmin.Id);

        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };

        var branchExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            BranchName = "Test Branch",
            IsValid = true,
            ErrorMessage = null
        };

        var serviceExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            CompanyServiceName = "Test Service",
            IsValid = true,
            ErrorMessage = null
        };

        _mockCurrentService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeCompanyAdmin);

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));

        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchExpectedResponse));

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(serviceExpectedResponse));

        var hashedPassword = "HashedPassword123";
        _mockPasswordHasher
            .Setup(s => s.HashPassword(It.IsAny<UserEntity>(), command.Password))
            .Returns(hashedPassword);


        //Act

        var result = await _handler.Handle(command, CancellationToken.None);


        //Assert

        result.ShouldNotBeNull();
        result.Roles.ShouldBe(UserRoles.Employee);
        result.EmailAddress.ShouldBe(command.EmailAddress);
        result.PasswordHash.ShouldBe(hashedPassword);
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Creator_Not_Found()
    {
        //Arrange
        var command = new CreateEmployeeRoleCommand(
            1,
            1,
            "employee@test.com",
            "EmployeeTest.1234",
            "Test Firstname",
            "Test Lastname",
            "Barber",
            "+992923324252",
            222);

        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("Creator not found");
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Creator_Is_Not_SystemAdmin_Or_CompanyAdmin()
    {
        var employeeUser = TestDataSeeder.CreateUserEmployeeRole();
        await _dbContext.Users.AddAsync(employeeUser, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateEmployeeRoleCommand(
            1,
            1,
            "employee@test.com",
            "EmployeeTest.1234",
            "Test Firstname",
            "Test Lastname",
            "Barber",
            "+992923324252",
            employeeUser.Id);

        // Act 

        var result = _handler.Handle(command, CancellationToken.None);


        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Not allowed to create employee");
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Email_Already_Exists()
    {
        var companyAdmin = TestDataSeeder.CreateUserCompanyAdmin();

        await _dbContext.Users.AddAsync(companyAdmin, CancellationToken.None);

        var existingUser = TestDataSeeder.CreateUserEmployeeRole();
        existingUser.EmailAddress = "employee@test.com";
        await _dbContext.Users.AddAsync(existingUser, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateEmployeeRoleCommand(
            1,
            1,
            "employee@test.com",
            "EmployeeTest.1234",
            "Test Firstname",
            "Test Lastname",
            "Barber",
            "+992923324252",
            companyAdmin.Id);

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
        var companyAdmin = TestDataSeeder.CreateUserCompanyAdmin();

        await _dbContext.Users.AddAsync(companyAdmin, CancellationToken.None);

        var existingUser = TestDataSeeder.CreateEmployee();
        existingUser.PhoneNumber = "+992923324252";
        await _dbContext.Employees.AddAsync(existingUser, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateEmployeeRoleCommand(
            1,
            1,
            "employee@test.com",
            "EmployeeTest.1234",
            "Test Firstname",
            "Test Lastname",
            "Barber",
            "+992923324252",
            companyAdmin.Id);

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
        var userCompanyAdmin = TestDataSeeder.CreateUserCompanyAdmin();
        var employeeCompanyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        await _dbContext.Employees.AddAsync(employeeCompanyAdmin, CancellationToken.None);
        await _dbContext.Users.AddAsync(userCompanyAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new CreateEmployeeRoleCommand(
            1,
            1,
            "employee@test.com",
            "EmployeeTest.1234",
            "Test Firstname",
            "Test Lastname",
            "Barber",
            "+992923324252",
            userCompanyAdmin.Id);

        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 999,
            CompanyName = "Test Name",
            IsValid = false,
            ErrorMessage = "Company not found"
        };


        _mockCurrentService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeCompanyAdmin);

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));


        //Act

        var result = _handler.Handle(command, CancellationToken.None);


        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("Company not found");
    }

    [Fact]
    public async Task Handler_Should_Trow_Exception_When_Branch_Not_Found()
    {
        var userCompanyAdmin = TestDataSeeder.CreateUserCompanyAdmin();
        var employeeCompanyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        await _dbContext.Employees.AddAsync(employeeCompanyAdmin, CancellationToken.None);
        await _dbContext.Users.AddAsync(userCompanyAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new CreateEmployeeRoleCommand(
            1,
            1,
            "employee@test.com",
            "EmployeeTest.1234",
            "Test Firstname",
            "Test Lastname",
            "Barber",
            "+992923324252",
            userCompanyAdmin.Id);

        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 999,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };
        
        var branchExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            BranchName = "Test Branch",
            IsValid = false,
            ErrorMessage = "Branch not found"
        };


        _mockCurrentService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeCompanyAdmin);

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));
        
        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchExpectedResponse));
        


        //Act

        var result = _handler.Handle(command, CancellationToken.None);


        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("Branch not found");
    }


    [Fact]
    public async Task Handler_Should_Throw_Exception_When_CompanyService_Not_Found()
    {
        var userCompanyAdmin = TestDataSeeder.CreateUserCompanyAdmin();
        var employeeCompanyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        await _dbContext.Employees.AddAsync(employeeCompanyAdmin, CancellationToken.None);
        await _dbContext.Users.AddAsync(userCompanyAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new CreateEmployeeRoleCommand(
            1,
            1,
            "employee@test.com",
            "EmployeeTest.1234",
            "Test Firstname",
            "Test Lastname",
            "Barber",
            "+992923324252",
            userCompanyAdmin.Id);

        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 999,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };
        
        var branchExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            BranchName = "Test Branch",
            IsValid = true,
            ErrorMessage = null
        };
        
        var serviceExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            CompanyServiceName = "Test Service",
            IsValid = false,
            ErrorMessage = "CompanyService not found"
        };


        _mockCurrentService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeCompanyAdmin);

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));
        
        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchExpectedResponse));
        
        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(serviceExpectedResponse));
        


        //Act

        var result = _handler.Handle(command, CancellationToken.None);


        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("CompanyService not found");
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_ServiceId_Is_Null()
    {
        var userCompanyAdmin = TestDataSeeder.CreateUserCompanyAdmin();
        var employeeCompanyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        await _dbContext.Employees.AddAsync(employeeCompanyAdmin, CancellationToken.None);
        await _dbContext.Users.AddAsync(userCompanyAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new CreateEmployeeRoleCommand(
            1,
            null,
            "employee@test.com",
            "EmployeeTest.1234",
            "Test Firstname",
            "Test Lastname",
            "Barber",
            "+992923324252",
            userCompanyAdmin.Id);
        
        _mockCurrentService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeCompanyAdmin);
        
        
        //Act

        var result = _handler.Handle(command, CancellationToken.None);


        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("ServiceId is required for creating an employee");

    }
    
    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
        
        var userCompanyAdmin = TestDataSeeder.CreateUserCompanyAdmin();
        var employeeCompanyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        await _dbContext.Employees.AddAsync(employeeCompanyAdmin, CancellationToken.None);
        await _dbContext.Users.AddAsync(userCompanyAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new CreateEmployeeRoleCommand(
            1,
            1,
            "employee@test.com",
            "EmployeeTest.1234",
            "Test Firstname",
            "Test Lastname",
            "Barber",
            "+992923324252",
            userCompanyAdmin.Id);

        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };

        var branchExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            BranchName = "Test Branch",
            IsValid = true,
            ErrorMessage = null
        };

        var serviceExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            CompanyServiceName = "Test Service",
            IsValid = true,
            ErrorMessage = null
        };

        _mockCurrentService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeCompanyAdmin);

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));

        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchExpectedResponse));

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(serviceExpectedResponse));

        var hashedPassword = "HashedPassword123";
        _mockPasswordHasher
            .Setup(s => s.HashPassword(It.IsAny<UserEntity>(), command.Password))
            .Returns(hashedPassword);


        //Act

        var result = await _handler.Handle(command, CancellationToken.None);


        // Assert
    

        result.ShouldNotBeNull();
        _mockPublishEndpoint.Verify(x=>x.Publish(It.IsAny<SendNotificationEvent>(), It.IsAny<CancellationToken>()));
        
    }
}