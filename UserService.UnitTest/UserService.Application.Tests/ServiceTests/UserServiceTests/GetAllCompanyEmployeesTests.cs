using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetAllCompanyEmployeesTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetAllCompanyEmployeesTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetAllCompanyEmployees_Should_Return_Employees_When_Company_Has_Employees()
    {
        // Arrange
        var companyId = 1;
        var employees = TestDataSeeder.CreateEmployees();
        foreach (var emp in employees)
        {
            emp.CompanyId = companyId;
        }
        await _dbContext.Employees.AddRangeAsync(employees);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userService.GetAllCompanyEmployees(companyId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(employees.Count);
        
        var firstEmployee = result.First();
        firstEmployee.EmployeeId.ShouldBe(employees.First().Id);
        firstEmployee.CompanyId.ShouldBe(companyId);
        firstEmployee.FirstName.ShouldBe(employees.First().FirstName);
        firstEmployee.LastName.ShouldBe(employees.First().LastName);
        firstEmployee.Position.ShouldBe(employees.First().Position);
        firstEmployee.PhoneNumber.ShouldBe(employees.First().PhoneNumber);
        
    }

    [Fact]
    public async Task GetAllCompanyEmployees_Should_Return_Empty_List_When_Company_Has_No_Employees()
    {
        // Arrange
        var companyId = 999;

        // Act
        var result = await _userService.GetAllCompanyEmployees(companyId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

      ;
    }
    
}