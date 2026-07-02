using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.BlockedCustomersRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetBlockedCustomerByIdTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetBlockedCustomerByIdTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetBlockedCustomerById_Should_Return_BlockedCustomer_When_Exists()
    {
        // Arrange
        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        blockedCustomer.Reason = "Violated company policies";
        blockedCustomer.BannedUntil = DateTime.UtcNow.AddDays(30);
        blockedCustomer.DoesBanForever = false;
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer);
        await _dbContext.SaveChangesAsync();

        var request = new BlockedCustomerByIdRequest
        {
            BlockedCustomerId = blockedCustomer.Id
        };

        // Act
        var result = await _userService.GetBlockedCustomerById(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Id.ShouldBe(blockedCustomer.Id);
        result.CustomerId.ShouldBe(blockedCustomer.CustomerId);
        result.CompanyId.ShouldBe(blockedCustomer.CompanyId);
        result.Reason.ShouldBe(blockedCustomer.Reason);
        result.BannedUntil.ShouldBe(blockedCustomer.BannedUntil);
        result.DoesBanForever.ShouldBe(blockedCustomer.DoesBanForever);
        result.CreatedAt.ShouldBe(blockedCustomer.CreatedAt);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task GetBlockedCustomerById_Should_Return_Error_When_BlockedCustomer_Not_Found()
    {
        // Arrange
        var request = new BlockedCustomerByIdRequest
        {
            BlockedCustomerId = 999
        };

        // Act
        var result = await _userService.GetBlockedCustomerById(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Id.ShouldBe(request.BlockedCustomerId);
        result.ErrorMessage.ShouldBe("Blocked customer not found");
    }
    
}