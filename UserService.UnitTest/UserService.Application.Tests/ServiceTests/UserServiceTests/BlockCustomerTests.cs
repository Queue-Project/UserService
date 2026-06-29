using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.BlockedCustomersRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class BlockCustomerTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public BlockCustomerTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task BlockCustomer_Should_Block_Customer_Successfully()
    {
        // Arrange
        var customer = TestDataSeeder.CreateCustomer();
        await _dbContext.Customer.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var request = new BlockCustomerRequest
        {
            CustomerId = customer.Id,
            CompanyId = 1,
            Reason = "Did not come 3  times",
            BannedUntil = DateTime.UtcNow.AddDays(30),
            DoesBanForever = false
        };

        // Act
        var result = await _userService.BlockCustomer(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeTrue();
        result.BlockedCustomerId.ShouldBeGreaterThan(0);
        result.ErrorMessage.ShouldBeNull();
        
    }

    [Fact]
    public async Task BlockCustomer_Should_Return_Error_When_Customer_Already_Blocked()
    {
        // Arrange
        var customer = TestDataSeeder.CreateCustomer();
        await _dbContext.Customer.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var existingBlock = TestDataSeeder.CreateBlockedCustomer();
        existingBlock.CustomerId = customer.Id;
        existingBlock.CompanyId = 1;
        await _dbContext.BlockedCustomers.AddAsync(existingBlock);
        await _dbContext.SaveChangesAsync();

        var request = new BlockCustomerRequest
        {
            CustomerId = customer.Id,
            CompanyId = 1,
            Reason = "Did not come 3 time",
            BannedUntil = DateTime.UtcNow.AddDays(15),
            DoesBanForever = false
        };

        // Act
        var result = await _userService.BlockCustomer(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Customer is already blocked for this company");
    }
    
}