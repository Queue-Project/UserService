using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Domain.Enums;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetCurrentUserInfoTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetCurrentUserInfoTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetCurrentUserInfo_Should_Return_UserInfo_When_User_Exists()
    {
        // Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        user.EmailAddress = "current@example.com";
        user.Roles = UserRoles.Customer;
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new CurrentUserRequest
        {
            UserId = user.Id
        };

        // Act
        var result = await _userService.GetCurrentUserInfo(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.UserId.ShouldBe(user.Id);
        result.EmailAddress.ShouldBe(user.EmailAddress);
        result.Role.ShouldBe(user.Roles.ToString());
        result.EmployeeId.ShouldBe(user.EmployeeId);
        result.CustomerId.ShouldBe(user.CustomerId);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task GetCurrentUserInfo_Should_Return_Error_When_User_Not_Found()
    {
        // Arrange
        var request = new CurrentUserRequest
        {
            UserId = 999
        };

        // Act
        var result = await _userService.GetCurrentUserInfo(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.UserId.ShouldBe(request.UserId);
        result.EmailAddress.ShouldBe("Unknown");
        result.Role.ShouldBe("Unknown");
        result.ErrorMessage.ShouldBe("User not found");
    }
    
    
}