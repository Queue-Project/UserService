using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetUserEmailByCustomerIdTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetUserEmailByCustomerIdTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetUserEmailByCustomerId_Should_Return_UserEmail_When_Customer_Has_User()
    {
        // Arrange
        var customer = TestDataSeeder.CreateCustomer();
        await _dbContext.Customer.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var user = TestDataSeeder.CreateUserCustomer();
        user.CustomerId = customer.Id;
        user.EmailAddress = "customer@example.com";
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new GetUserEmailByCustomerIdRequest
        {
            CustomerId = customer.Id
        };

        // Act
        var result = await _userService.GetUserEmailByCustomerId(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.UserId.ShouldBe(user.Id);
        result.EmailAddress.ShouldBe(user.EmailAddress);
        result.CustomerId.ShouldBe(customer.Id);
        result.EmployeeId.ShouldBeNull();
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task GetUserEmailByCustomerId_Should_Return_Error_When_User_Not_Found()
    {
        // Arrange
        var request = new GetUserEmailByCustomerIdRequest
        {
            CustomerId = 999
        };

        // Act
        var result = await _userService.GetUserEmailByCustomerId(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.UserId.ShouldBe(0);
        result.EmailAddress.ShouldBe("Unknown");
        result.ErrorMessage.ShouldBe("User not found for this customer");
    }
    
}