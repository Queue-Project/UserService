using System.Net;
using BranchService.Contracts.Interfaces;
using BranchService.Contracts.Requests;
using BranchService.Contracts.Responses;
using MagicOnion;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.Employees.Commands.CreateEmployee;
using QUserService.Contracts.Events.EmployeeEvent;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.EmployeeTests;

public class CreateEmployeeCommandHandlerTests
{
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<CreateEmployeeCommandHandler>> _mockLogger;
    private readonly Mock<IBranchService> _mockBranchService;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly CreateEmployeeCommandHandler _handler;

    public CreateEmployeeCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<CreateEmployeeCommandHandler>>();
        _mockBranchService = new Mock<IBranchService>();
        _mockCurrentUser = new Mock<ICurrentUserService>();
        _handler = new CreateEmployeeCommandHandler(
            _mockLogger.Object, 
            _dbContext, 
            _mockPublishEndpoint.Object, 
            _mockBranchService.Object, 
            _mockCurrentUser.Object);
    }
    
    [Fact]
    public async Task Handler_Should_Create_Employee_Successfully()
    {
        // Arrange
        var companyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        await _dbContext.Employees.AddAsync(companyAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateEmployeeCommand(
            1,
            1,
            "Test Firstname",
            "Test Lastname",
            "Test Position",
            "+992923324252"
        );

        var serviceExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            CompanyServiceName = "Test Service",
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
        
        _mockCurrentUser
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(companyAdmin);

        
        _mockBranchService
            .Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(serviceExpectedResponse));

        _mockBranchService
            .Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchExpectedResponse));
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeGreaterThan(0);
        result.FirstName.ShouldBe(command.Firstname);
        result.LastName.ShouldBe(command.Lastname);
        result.PhoneNumber.ShouldBe(command.PhoneNumber);
        result.Position.ShouldBe(command.Position);
        result.CompanyId.ShouldBe(companyAdmin.CompanyId);
        result.BranchId.ShouldBe(command.BranchId);
        result.ServiceId.ShouldBe(command.ServiceId);
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_CompanyService_Not_Found()
    {
        // Arrange
        var companyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        await _dbContext.Employees.AddAsync(companyAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateEmployeeCommand(
            99, 
            1,
            "Test Firstname",
            "Test Lastname",
            "Test Position",
            "+992923324252"
        );

        var serviceExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 99,
            IsValid = false,
            ErrorMessage = "CompanyService not found",
            CompanyServiceName = null
        };
        
        _mockCurrentUser
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(companyAdmin);

        _mockBranchService
            .Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(serviceExpectedResponse));

        // Act 
        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("CompanyService not found");
        
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Branch_Not_Found()
    {
        // Arrange
        var companyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        await _dbContext.Employees.AddAsync(companyAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateEmployeeCommand(
            99,
            1,
            "Test Firstname",
            "Test Lastname",
            "Test Position",
            "+992923324252"
        );

        var serviceExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            CompanyServiceName = "Test Service",
            IsValid = true,
            ErrorMessage = null
        };

        var branchExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 99,
            IsValid = false,
            ErrorMessage = "Branch not found",
            BranchName = null
        };
        
        _mockCurrentUser
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(companyAdmin);

        _mockBranchService
            .Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(serviceExpectedResponse));

        _mockBranchService
            .Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchExpectedResponse));

        // Act 
        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("Branch not found");

        
    }
    

    [Fact]
    public async Task Handler_Should_Throw_When_CurrentUser_Not_Found()
    {
        // Arrange
        var command = new CreateEmployeeCommand(
            1,
            1,
            "Test Firstname",
            "Test Lastname",
            "Test Position",
            "+992923324252"
        );
        
        _mockCurrentUser
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Current user not found"));

        
        // Act 
        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
         await result.ShouldThrowAsync<UnauthorizedAccessException>();
        
    }
    
    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
         var companyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        await _dbContext.Employees.AddAsync(companyAdmin, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateEmployeeCommand(
            1,
            1,
            "Test Firstname",
            "Test Lastname",
            "Test Position",
            "+992923324252"
        );

        var serviceExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            CompanyServiceName = "Test Service",
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
        
        _mockCurrentUser
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(companyAdmin);

        
        _mockBranchService
            .Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(serviceExpectedResponse));

        _mockBranchService
            .Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchExpectedResponse));
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
    

        result.ShouldNotBeNull();
        _mockPublishEndpoint.Verify(x=>x.Publish(It.IsAny<EmployeeCreatedEvent>(), It.IsAny<CancellationToken>()));
        
    }
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Phone_Number_Already_Exists()
    {
        // Arrange

        var existingUser = TestDataSeeder.CreateEmployee();
        existingUser.PhoneNumber = "+992986654535";
        await _dbContext.Employees.AddAsync(existingUser, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateEmployeeCommand(
            1,
            1,
            "Test Firstname",
            "Test Lastname",
            "Test Position",
            "+992986654535"
        );

        // Act 
        
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Phone number already exists");

      
    }
    
    
}