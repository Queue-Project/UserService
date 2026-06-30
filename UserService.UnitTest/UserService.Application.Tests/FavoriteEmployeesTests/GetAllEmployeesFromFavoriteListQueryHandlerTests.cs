using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.UseCases.FavoriteEmployees.Queries.GetAllEmployeesFromFavoriteList;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.FavoriteEmployeesTests;

public class GetAllEmployeesFromFavoriteListQueryHandlerTests
{
    private readonly Mock<ILogger<GetAllEmployeesFromFavoriteListQueryHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly GetAllEmployeesFromFavoriteListQueryHandler _handler;

    public GetAllEmployeesFromFavoriteListQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetAllEmployeesFromFavoriteListQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new GetAllEmployeesFromFavoriteListQueryHandler(_mockLogger.Object, _dbContext);
    }
    
    [Fact]
    public async Task Should_Return_Favorite_Employees_Successfully()
    {
        
        //Arrange
        var employees = TestDataSeeder.CreateFavoriteEmployees();
        await _dbContext.FavoriteEmployeeEntities.AddRangeAsync(employees, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var query = new GetAllEmployeesFromFavoriteListQuery(1);
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        result.ShouldNotBeNull();
        result.HasNextPage.ShouldBe(false);
        result.TotalPages.ShouldBe(1);
        result.TotalCount.ShouldBe(3);

        var employee = result.Items.FirstOrDefault();
        employee!.Id.ShouldBe(1);
    }

    [Fact]
    public async Task Should_Return_Favorite_Employees_EmptyList()
    {
        //Arrange
        var query = new GetAllEmployeesFromFavoriteListQuery(1);
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        
        result.TotalCount.ShouldBe(0);
        result.HasNextPage.ShouldBe(false);
        var companyList = result.Items;
        companyList.ShouldBeEmpty();
        
    }
}