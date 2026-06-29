using System.Net;
using BranchService.Contracts.Interfaces;
using BranchService.Contracts.Requests;
using BranchService.Contracts.Responses;
using MagicOnion;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QAuthService.Contracts.Events.BlockedCustomerEvent;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.BlockedCustomers.Commands.CreateBlockedCustomer;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.BlockedCustomerTests;

public class CreateBlockedCustomerCommandHandlerTests
{
    private readonly Mock<ILogger<CreateBlockedCustomerCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly Mock<IBranchService> _mockBranchService;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly CreateBlockedCustomerCommandHandler _handler;

    public CreateBlockedCustomerCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CreateBlockedCustomerCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockCurrentUser = new Mock<ICurrentUserService>();
        _mockBranchService = new Mock<IBranchService>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _handler = new CreateBlockedCustomerCommandHandler(_mockLogger.Object, _dbContext, _mockCurrentUser.Object,
            _mockBranchService.Object, _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_BlockedCustomer()
    {
        //Arrange

        var customer = TestDataSeeder.CreateCustomer();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateBlockedCustomerCommand(
            1,
            "Did not come 3 times",
            DateTime.UtcNow.AddDays(20),
            false);
        
        
        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = employee.CompanyId,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };

        _mockCurrentUser.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));
        
        //Act

        var result = await _handler.Handle(command, CancellationToken.None);
        
        //Assert

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.CompanyId.ShouldBe(companyExpectedResponse.CompanyId);
        result.CustomerId.ShouldBe(command.CustomerId);
        result.BannedUntil.ShouldBe(command.BannedUntil);
        result.DoesBanForever.ShouldBe(command.DoesBanForever);
        result.Reason.ShouldBe(command.Reason);

    }


    [Fact]
    public async Task Handler_Should_Return_CustomerNotFound()
    {
        //Arrange
        
        var command = new CreateBlockedCustomerCommand(
            1,
            "Did not come 3 times",
            DateTime.UtcNow.AddDays(20),
            false);
        
        //Act
        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("CustomerEntity");
    }


    [Fact]
    public async Task Handler_Should_Throw_Exception_When_CompanyNotFound()
    {
        //Arrange

        var customer = TestDataSeeder.CreateCustomer();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateBlockedCustomerCommand(
            1,
            "Did not come 3 times",
            DateTime.UtcNow.AddDays(20),
            false);
        
        
        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 99,
            CompanyName = "Test Name",
            IsValid = false,
            ErrorMessage = "Company not found"
        };

        _mockCurrentUser.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));
        
        
        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain($"Company not found");

    }


    [Fact]
    public async Task Handler_Should_Throw_Exception_When_CurrentEmployeeNotFound()
    {
        //Arrange

        var customer = TestDataSeeder.CreateCustomer();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateBlockedCustomerCommand(
            1,
            "Did not come 3 times",
            DateTime.UtcNow.AddDays(20),
            false);

        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found");
        
        _mockCurrentUser.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);
        
        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"Employee not found");
    }


    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Customer_Already_Blocked()
    {
        var customer = TestDataSeeder.CreateCustomer();
        var employee = TestDataSeeder.CreateEmployee();
        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateBlockedCustomerCommand(
            1,
            "Did not come 3 times",
            DateTime.UtcNow.AddDays(20),
            false);
        
        
        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = employee.CompanyId,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };

        _mockCurrentUser.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));
        
        //Act

        var result =  _handler.Handle(command, CancellationToken.None);

        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        exception.Message.ShouldContain("Customer is already blocked for this company");
    }
    
    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
        //Arrange

        var customer = TestDataSeeder.CreateCustomer();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateBlockedCustomerCommand(
            1,
            "Did not come 3 times",
            DateTime.UtcNow.AddDays(20),
            false);
        
        
        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = employee.CompanyId,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };

        _mockCurrentUser.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));
        
        //Act

        var result = await _handler.Handle(command, CancellationToken.None);
        
        //Assert
    

        result.ShouldNotBeNull();
        _mockPublishEndpoint.Verify(x=>x.Publish(It.IsAny<BlockedCustomerCreatedEvent>(), It.IsAny<CancellationToken>()));
        
    }
    
    

}