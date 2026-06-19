using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.BlockedCustomers.Queries.GetBlockedCustomerById;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.BlockedCustomerTests;

public class GetBlockedCustomerByIdQueryHandlerTests
{
    private readonly Mock<ILogger<GetBlockedCustomerByIdQueryHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly GetBlockedCustomerByIdQueryHandler _handler;

    public GetBlockedCustomerByIdQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetBlockedCustomerByIdQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new GetBlockedCustomerByIdQueryHandler(_mockLogger.Object, _dbContext, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_Customer_Successfully()
    {

        //Arrange
        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var query = new GetBlockedCustomerByIdQuery(1);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        
        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(blockedCustomer.Id);
        result.CompanyId.ShouldBe(blockedCustomer.CompanyId);
        result.CustomerId.ShouldBe(blockedCustomer.CustomerId);
        result.BannedUntil.ShouldBe(blockedCustomer.BannedUntil);
        result.DoesBanForever.ShouldBe(blockedCustomer.DoesBanForever);
        result.Reason.ShouldBe(blockedCustomer.Reason);
        
    }

    [Fact]
    public async Task Handler_Should_Return_CustomerNotFound()
    {
        //Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        
        var query = new GetBlockedCustomerByIdQuery(1);
        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        
        //Act
        var result =  _handler.Handle(query, CancellationToken.None);

        
        //Assert

        var exception = result.ShouldThrow<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("BlockedCustomerEntity");
    }


    [Fact]
    public async Task Handler_Should_Throw_Exception_When_CurrentEmployeeNotFound()
    {
        //Arrange

        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer);
        
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var query = new GetBlockedCustomerByIdQuery(1);

        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found");
        
        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);
        
        //Act
        var result = _handler.Handle(query, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"Employee not found");
    }
}