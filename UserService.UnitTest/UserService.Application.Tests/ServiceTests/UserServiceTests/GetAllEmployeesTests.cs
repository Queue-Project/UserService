using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetAllEmployeesTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetAllEmployeesTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetAllEmployees_Should_Return_All_Employees_When_Employees_Exist()
    {
        // Arrange
        var employees = TestDataSeeder.CreateEmployees();
        await _dbContext.Employees.AddRangeAsync(employees);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userService.GetAllEmployees();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(employees.Count);
        
        var employee = result.First();
        employee.EmployeeId.ShouldBe(employees.First().Id);
        employee.FirstName.ShouldBe(employees.First().FirstName);
        employee.LastName.ShouldBe(employees.First().LastName);
        
    }

    [Fact]
    public async Task GetAllEmployees_Should_Return_Empty_List_When_No_Employees_Exist()
    {
        // Act
        var result = await _userService.GetAllEmployees();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
        
    }
    
}