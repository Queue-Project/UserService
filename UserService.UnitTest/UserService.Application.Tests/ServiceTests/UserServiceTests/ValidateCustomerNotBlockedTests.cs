using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class ValidateCustomerNotBlockedTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public ValidateCustomerNotBlockedTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task ValidateCustomerNotBlocked_Should_Return_Valid_When_Customer_Not_Blocked()
    {
        // Arrange
        var request = new CustomerBlockValidationRequest
        {
            CustomerId = 1,
            CompanyId = 1
        };

        // Act
        var result = await _userService.ValidateCustomerNotBlocked(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.IsBlocked.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateCustomerNotBlocked_Should_Return_Invalid_When_Customer_Is_Blocked()
    {
        // Arrange
        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        blockedCustomer.BannedUntil = DateTime.UtcNow.AddDays(10);
        blockedCustomer.DoesBanForever = false;
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer);
        await _dbContext.SaveChangesAsync();

        var request = new CustomerBlockValidationRequest
        {
            CustomerId = blockedCustomer.CustomerId,
            CompanyId = blockedCustomer.CompanyId
        };

        // Act
        var result = await _userService.ValidateCustomerNotBlocked(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.IsBlocked.ShouldBeTrue();
        result.BlockReason.ShouldBe(blockedCustomer.Reason);
        result.BannedUntil.ShouldBe(blockedCustomer.BannedUntil);
    }
    
}