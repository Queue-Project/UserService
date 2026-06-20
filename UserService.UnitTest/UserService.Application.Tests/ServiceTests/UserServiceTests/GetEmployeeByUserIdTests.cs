using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.EmployeeRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetEmployeeByUserIdTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetEmployeeByUserIdTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetEmployeeByUserId_Should_Return_Employee_When_User_Is_Employee()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync();

        var user = TestDataSeeder.CreateUserEmployeeRole();
        user.EmployeeId = employee.Id;
        user.Employee = employee;
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new GetEmployeeByUserIdRequest { UserId = user.Id };

        // Act
        var result = await _userService.GetEmployeeByUserId(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Id.ShouldBe(employee.Id);
        result.CompanyId.ShouldBe(employee.CompanyId);
        result.FirstName.ShouldBe(employee.FirstName);
        result.LastName.ShouldBe(employee.LastName);
        result.Position.ShouldBe(employee.Position);
        result.PhoneNumber.ShouldBe(employee.PhoneNumber);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task GetEmployeeByUserId_Should_Return_Error_When_User_Not_Found()
    {
        // Arrange
        var request = new GetEmployeeByUserIdRequest { UserId = 999 };

        // Act
        var result = await _userService.GetEmployeeByUserId(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.FirstName.ShouldBe("Unknown");
        result.LastName.ShouldBe("Unknown");
        result.Position.ShouldBe("Unknown");
        result.PhoneNumber.ShouldBe("Unknown");
        result.ErrorMessage.ShouldBe("User not found");
    }

    [Fact]
    public async Task GetEmployeeByUserId_Should_Return_Error_When_User_Is_Not_Employee()
    {
        // Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        user.EmployeeId = null;
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new GetEmployeeByUserIdRequest { UserId = user.Id };

        // Act
        var result = await _userService.GetEmployeeByUserId(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.FirstName.ShouldBe("Unknown");
        result.LastName.ShouldBe("Unknown");
        result.Position.ShouldBe("Unknown");
        result.PhoneNumber.ShouldBe("Unknown");
        result.ErrorMessage.ShouldBe("User is not an employee");
    }
}