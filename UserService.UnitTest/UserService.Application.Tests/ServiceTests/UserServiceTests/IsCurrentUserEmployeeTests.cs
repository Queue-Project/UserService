using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class IsCurrentUserEmployeeTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public IsCurrentUserEmployeeTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task IsCurrentUserEmployee_Should_Return_True_When_User_Has_EmployeeId()
    {
        // Arrange
        var user = TestDataSeeder.CreateUserEmployeeRole();
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new CurrentUserRequest
        {
            UserId = user.Id
        };

        // Act
        var result = await _userService.IsCurrentUserEmployee(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsEmployee.ShouldBeTrue();
    }

    [Fact]
    public async Task IsCurrentUserEmployee_Should_Return_False_When_User_Has_No_EmployeeId()
    {
        // Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new CurrentUserRequest
        {
            UserId = user.Id
        };

        // Act
        var result = await _userService.IsCurrentUserEmployee(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsEmployee.ShouldBeFalse();
    }
    
}