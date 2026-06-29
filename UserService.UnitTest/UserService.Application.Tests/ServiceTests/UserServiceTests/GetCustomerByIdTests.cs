using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetCustomerByIdTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetCustomerByIdTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetCustomerById_Should_Return_Customer_When_Customer_Exists()
    {
        // Arrange
        var customer = TestDataSeeder.CreateCustomer();
        await _dbContext.Customer.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var request = new CustomerByIdRequest { CustomerId = customer.Id };

        // Act
        var result = await _userService.GetCustomerById(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Id.ShouldBe(customer.Id);
        result.FirstName.ShouldBe(customer.FirstName);
        result.LastName.ShouldBe(customer.LastName);
        result.PhoneNumber.ShouldBe(customer.PhoneNumber);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task GetCustomerById_Should_Return_Error_When_Customer_Not_Found()
    {
        // Arrange
        var request = new CustomerByIdRequest { CustomerId = 999 };

        // Act
        var result = await _userService.GetCustomerById(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Id.ShouldBe(request.CustomerId);
        result.FirstName.ShouldBe("Unknown");
        result.LastName.ShouldBe("Unknown");
        result.PhoneNumber.ShouldBe("Unknown");
        result.ErrorMessage.ShouldBe("Customer not found");
    }
    
}