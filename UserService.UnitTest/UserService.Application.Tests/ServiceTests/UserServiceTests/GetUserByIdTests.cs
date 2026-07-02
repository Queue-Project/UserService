using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetUserByIdTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetUserByIdTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetUserById_Should_Return_User_When_User_Exists()
    {
        // Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new UserByIdRequest { UserId = user.Id };

        // Act
        var result = await _userService.GetUserById(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Id.ShouldBe(user.Id);
        result.EmailAddress.ShouldBe(user.EmailAddress);
        result.Roles.ShouldBe(user.Roles.ToString());
        result.EmployeeId.ShouldBe(user.EmployeeId);
        result.CustomerId.ShouldBe(user.CustomerId);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task GetUserById_Should_Return_Error_When_User_Not_Found()
    {
        // Arrange
        var request = new UserByIdRequest { UserId = 999 };

        // Act
        var result = await _userService.GetUserById(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Id.ShouldBe(request.UserId);
        result.EmailAddress.ShouldBe("Unknown");
        result.Roles.ShouldBe("Unknown");
        result.ErrorMessage.ShouldBe("User not found");
    }
    
}