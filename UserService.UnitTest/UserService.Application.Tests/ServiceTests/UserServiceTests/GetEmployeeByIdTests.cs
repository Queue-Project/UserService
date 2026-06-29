using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.EmployeeRequests;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetEmployeeByIdTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetEmployeeByIdTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetEmployeeById_Should_Return_Employee_When_Employee_Exists()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync();

        var request = new EmployeeByIdRequest { EmployeeId = employee.Id };

        // Act
        var result = await _userService.GetEmployeeById(request);

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
    public async Task GetEmployeeById_Should_Return_Error_When_Employee_Not_Found()
    {
        // Arrange
        var request = new EmployeeByIdRequest { EmployeeId = 999 };

        // Act
        var result = await _userService.GetEmployeeById(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Id.ShouldBe(request.EmployeeId);
        result.FirstName.ShouldBe("Unknown");
        result.LastName.ShouldBe("Unknown");
        result.Position.ShouldBe("Unknown");
        result.PhoneNumber.ShouldBe("Unknown");
        result.ErrorMessage.ShouldBe("Employee not found");
    }
    
}