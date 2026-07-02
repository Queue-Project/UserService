using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetAllCompanyBlockedCustomersTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetAllCompanyBlockedCustomersTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetAllCompanyBlockedCustomers_Should_Return_BlockedCustomers_When_Company_Has_Blocked_Customers()
    {
        // Arrange
        var companyId = 1;
        var customer = TestDataSeeder.CreateCustomer();
        await _dbContext.Customer.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var blockedCustomers = TestDataSeeder.CreateBlockedCustomers();
        foreach (var blockedCustomer in blockedCustomers)
        {
            blockedCustomer.CompanyId = companyId;
            blockedCustomer.CustomerId = customer.Id;
        }
        await _dbContext.BlockedCustomers.AddRangeAsync(blockedCustomers);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userService.GetAllCompanyBlockedCustomers(companyId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(blockedCustomers.Count);
        
        var blocked = result.First();
        blocked.BlockedId.ShouldBe(blockedCustomers.First().Id);
        blocked.CompanyId.ShouldBe(companyId);
        blocked.CustomerId.ShouldBe(customer.Id);
        blocked.Reason.ShouldBe(blockedCustomers.First().Reason);
        
    }

    [Fact]
    public async Task GetAllCompanyBlockedCustomers_Should_Return_Empty_List_When_Company_Has_No_Blocked_Customers()
    {
        // Arrange
        var companyId = 999;

        // Act
        var result = await _userService.GetAllCompanyBlockedCustomers(companyId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
        
    }
    
}