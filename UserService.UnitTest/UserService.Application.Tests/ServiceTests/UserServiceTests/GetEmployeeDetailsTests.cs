using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetEmployeeDetailsTests
{
    
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetEmployeeDetailsTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetEmployeeDetails_Should_Return_Employees_When_Exist_Within_Ids()
    {
        // Arrange
        List<int> ids = [1,2,3];
        var users = TestDataSeeder.CreateUserEmployeesRole();
        
        await _dbContext.Users.AddRangeAsync(users);
        await _dbContext.SaveChangesAsync();
        

        // Act
        var result = await _userService.GetEmployeeDetails(ids);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(users.Count);
        
        var user = result.First();
        user.EmployeeId.ShouldBe(users.First().Id);
        user.CompanyId.ShouldBe(users.First().Employee.CompanyId);
     
        
    }

    [Fact]
    public async Task GetEmployeeDetails_Should_Return_Empty_List_When_Employees_Not_Exists()
    {
        // Arrange
        List<int> ids = [];


        // Act
        var result = await _userService.GetEmployeeDetails(ids);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
        
    }
}