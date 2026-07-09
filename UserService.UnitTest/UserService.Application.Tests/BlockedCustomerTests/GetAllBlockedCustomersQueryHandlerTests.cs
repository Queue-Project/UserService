using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.BlockedCustomers.Queries.GetAllBlockedCustomers;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.BlockedCustomerTests;

public class GetAllBlockedCustomersQueryHandlerTests
{
    private readonly Mock<ILogger<GetAllBlockedCustomersQueryHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly GetAllBlockedCustomersQueryHandler _handler;

    public GetAllBlockedCustomersQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetAllBlockedCustomersQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new GetAllBlockedCustomersQueryHandler(_mockLogger.Object, _dbContext, _mockCurrentUserService.Object);
    }
    
    [Fact]
    public async Task Should_Return_BlockedCustomers_Successfully()
    {
        
        //Arrange
        var employee = TestDataSeeder.CreateEmployee();
        var blockedCustomers = TestDataSeeder.CreateBlockedCustomers();
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.BlockedCustomers.AddRangeAsync(blockedCustomers, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var query = new GetAllBlockedCustomersQuery(1);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        result.ShouldNotBeNull();
        result.HasNextPage.ShouldBe(false);
        result.TotalPages.ShouldBe(1);
        result.TotalCount.ShouldBe(3);

        var customer = result.Items.FirstOrDefault();
        customer!.Id.ShouldBe(1);
    }

    [Fact]
    public async Task Should_Return_BlockedCustomersEmptyList()
    {
        //Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var query = new GetAllBlockedCustomersQuery(1);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        
        result.TotalCount.ShouldBe(0);
        result.HasNextPage.ShouldBe(false);
        var blockedList = result.Items;
        blockedList.ShouldBeEmpty();
        
    }
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_CurrentEmployeeNotFound()
    {
        //Arrange

        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer);
        
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var query = new GetAllBlockedCustomersQuery(1);

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