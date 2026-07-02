using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.UseCases.Employees.Queries.GetAllEmployees;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.EmployeeTests;

public class GetAllEmployeesQueryHandlerTests
{
    private readonly Mock<ILogger<GetAllEmployeesQueryHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly GetAllEmployeesQueryHandler _handler;

    public GetAllEmployeesQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetAllEmployeesQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new GetAllEmployeesQueryHandler(_mockLogger.Object, _dbContext);
    }
    
    [Fact]
    public async Task Should_Return_Employees_Successfully()
    {
        
        //Arrange
        var employees = TestDataSeeder.CreateEmployees();
        await _dbContext.Employees.AddRangeAsync(employees, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var query = new GetAllEmployeesQuery(1);
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        result.ShouldNotBeNull();
        result.HasNextPage.ShouldBe(false);
        result.TotalPages.ShouldBe(1);
        result.TotalCount.ShouldBe(3);

        var customer = result.Items.FirstOrDefault();
        customer!.Id.ShouldBe(1);
    }

    [Fact]
    public async Task Should_Return_EmployeeEmptyList()
    {
        //Arrange
        var query = new GetAllEmployeesQuery(1);
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        
        result.TotalCount.ShouldBe(0);
        result.HasNextPage.ShouldBe(false);
        var companyList = result.Items;
        companyList.ShouldBeEmpty();
        
    }
}