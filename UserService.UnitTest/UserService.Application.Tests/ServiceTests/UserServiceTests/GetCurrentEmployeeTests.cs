using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetCurrentEmployeeTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetCurrentEmployeeTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetCurrentEmployee_Should_Return_Employee_When_User_Is_Employee()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        employee.FirstName = "John";
        employee.LastName = "Doe";
        employee.Position = "Manager";
        employee.PhoneNumber = "+1234567890";
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync();

        var user = TestDataSeeder.CreateUserEmployeeRole();
        user.EmployeeId = employee.Id;
        user.Employee = employee;
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new CurrentUserRequest
        {
            UserId = user.Id
        };

        // Act
        var result = await _userService.GetCurrentEmployee(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.EmployeeId.ShouldBe(employee.Id);
        result.CompanyId.ShouldBe(employee.CompanyId);
        result.BranchId.ShouldBe(employee.BranchId);
        result.FirstName.ShouldBe(employee.FirstName);
        result.LastName.ShouldBe(employee.LastName);
        result.Position.ShouldBe(employee.Position);
        result.PhoneNumber.ShouldBe(employee.PhoneNumber);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task GetCurrentEmployee_Should_Return_Error_When_User_Not_Found()
    {
        // Arrange
        var request = new CurrentUserRequest
        {
            UserId = 999
        };

        // Act
        var result = await _userService.GetCurrentEmployee(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.EmployeeId.ShouldBe(0);
        result.ErrorMessage.ShouldBe("User not found");
    }

    [Fact]
    public async Task GetCurrentEmployee_Should_Return_Error_When_User_Is_Not_Employee()
    {
        // Arrange
        var user = TestDataSeeder.CreateUserCustomer();
        user.EmployeeId = null;
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new CurrentUserRequest
        {
            UserId = user.Id
        };

        // Act
        var result = await _userService.GetCurrentEmployee(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.EmployeeId.ShouldBe(0);
        result.ErrorMessage.ShouldBe("User is not an employee");
    }
}