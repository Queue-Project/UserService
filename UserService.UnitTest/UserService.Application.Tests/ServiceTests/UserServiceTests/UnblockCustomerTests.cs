using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.BlockedCustomersRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class UnblockCustomerTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public UnblockCustomerTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task UnblockCustomer_Should_Unblock_Customer_Successfully()
    {
        // Arrange
        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer);
        await _dbContext.SaveChangesAsync();

        var request = new UnblockCustomerRequest
        {
            BlockedCustomerId = blockedCustomer.Id
        };

        // Act
        var result = await _userService.UnblockCustomer(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeTrue();
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task UnblockCustomer_Should_Return_Error_When_BlockedCustomer_Not_Found()
    {
        // Arrange
        var request = new UnblockCustomerRequest
        {
            BlockedCustomerId = 999
        };

        // Act
        var result = await _userService.UnblockCustomer(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Blocked customer not found");
    }
}