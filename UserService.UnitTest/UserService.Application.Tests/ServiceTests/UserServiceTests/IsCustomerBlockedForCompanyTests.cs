using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class IsCustomerBlockedForCompanyTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public IsCustomerBlockedForCompanyTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task IsCustomerBlockedForCompany_Should_Return_Blocked_When_Customer_Is_Blocked()
    {
        // Arrange
        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        blockedCustomer.BannedUntil = DateTime.UtcNow.AddDays(10);
        blockedCustomer.DoesBanForever = false;
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer);
        await _dbContext.SaveChangesAsync();

        var request = new IsCustomerBlockedRequest
        {
            CustomerId = blockedCustomer.CustomerId,
            CompanyId = blockedCustomer.CompanyId
        };

        // Act
        var result = await _userService.IsCustomerBlockedForCompany(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsBlocked.ShouldBeTrue();
        result.IsBlockedForever.ShouldBeFalse();
        result.BannedUntil.ShouldBe(blockedCustomer.BannedUntil);
        result.BlockReason.ShouldBe(blockedCustomer.Reason);
    }

    [Fact]
    public async Task IsCustomerBlockedForCompany_Should_Return_NotBlocked_When_Customer_Not_Blocked()
    {
        // Arrange
        var request = new IsCustomerBlockedRequest
        {
            CustomerId = 1,
            CompanyId = 1
        };

        // Act
        var result = await _userService.IsCustomerBlockedForCompany(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsBlocked.ShouldBeFalse();
    }
    
}