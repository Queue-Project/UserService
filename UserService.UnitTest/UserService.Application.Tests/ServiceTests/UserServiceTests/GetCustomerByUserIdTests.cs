using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetCustomerByUserIdTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetCustomerByUserIdTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetCustomerByUserId_Should_Return_Customer_When_User_Is_Customer()
    {
        // Arrange
        var customer = TestDataSeeder.CreateCustomer();
        await _dbContext.Customer.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var user = TestDataSeeder.CreateUserCustomer();
        user.CustomerId = customer.Id;
        user.Customer = customer;
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new GetCustomerByUserIdRequest { UserId = user.Id };

        // Act
        var result = await _userService.GetCustomerByUserId(request);

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
    public async Task GetCustomerByUserId_Should_Return_Error_When_User_Not_Found()
    {
        // Arrange
        var request = new GetCustomerByUserIdRequest { UserId = 999 };

        // Act
        var result = await _userService.GetCustomerByUserId(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.FirstName.ShouldBe("Unknown");
        result.LastName.ShouldBe("Unknown");
        result.PhoneNumber.ShouldBe("Unknown");
        result.ErrorMessage.ShouldBe("User not found");
    }

    [Fact]
    public async Task GetCustomerByUserId_Should_Return_Error_When_User_Is_Not_Customer()
    {
        // Arrange
        var user = TestDataSeeder.CreateSystemAdmin();
        user.CustomerId = null;
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new GetCustomerByUserIdRequest { UserId = user.Id };

        // Act
        var result = await _userService.GetCustomerByUserId(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.FirstName.ShouldBe("Unknown");
        result.LastName.ShouldBe("Unknown");
        result.PhoneNumber.ShouldBe("Unknown");
        result.ErrorMessage.ShouldBe("User is not a customer");
    }
}