using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.UseCases.Customers.Queries.GetAllCustomers;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.CustomerTests;

public class GetAllCustomerQueryHandlerTests
{
    private readonly Mock<ILogger<GetAllCustomersQueryHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly GetAllCustomersQueryHandler _handler;

    public GetAllCustomerQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetAllCustomersQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new GetAllCustomersQueryHandler(_mockLogger.Object, _dbContext);
    }
    
    [Fact]
    public async Task Should_Return_Customers_Successfully()
    {
        
        //Arrange
        var customers = TestDataSeeder.CreateCustomers();
        await _dbContext.Customer.AddRangeAsync(customers, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var query = new GetAllCustomersQuery(1);
        
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
    public async Task Should_Return_CustomerEmptyList()
    {
        //Arrange
        var query = new GetAllCustomersQuery(1);
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        
        result.TotalCount.ShouldBe(0);
        result.HasNextPage.ShouldBe(false);
        var companyList = result.Items;
        companyList.ShouldBeEmpty();
        
    }
}