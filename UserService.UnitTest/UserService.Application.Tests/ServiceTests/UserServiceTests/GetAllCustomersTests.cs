using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetAllCustomersTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetAllCustomersTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetAllCustomers_Should_Return_All_Customers_When_Customers_Exist()
    {
        // Arrange
        var customers = TestDataSeeder.CreateCustomers();
        await _dbContext.Customer.AddRangeAsync(customers);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userService.GetAllCustomers();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(customers.Count);
        
        var customer = result.First();
        customer.CustomerId.ShouldBe(customers.First().Id);
        customer.FirstName.ShouldBe(customers.First().FirstName);
        customer.LastName.ShouldBe(customers.First().LastName);
        
    }

    [Fact]
    public async Task GetAllCustomers_Should_Return_Empty_List_When_No_Customers_Exist()
    {
        // Act
        var result = await _userService.GetAllCustomers();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

    }
    
}