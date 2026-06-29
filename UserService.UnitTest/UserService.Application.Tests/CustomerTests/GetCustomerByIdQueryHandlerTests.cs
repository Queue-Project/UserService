using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Customers.Queries.GetCustomerById;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.CustomerTests;

public class GetCustomerByIdQueryHandlerTests
{
    private readonly Mock<ILogger<GetCustomerByIdQueryHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly GetCustomerByIdQueryHandler _handler;

    public GetCustomerByIdQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetCustomerByIdQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new GetCustomerByIdQueryHandler(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task Handler_Should_Return_Customer_Successfully()
    {

        //Arrange
        var customer = TestDataSeeder.CreateCustomer();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var query = new GetCustomerByIdQuery(1);
        
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        
        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(result.Id);
        result.FirstName.ShouldBe(customer.FirstName);
        result.LastName.ShouldBe(customer.LastName);
        result.PhoneNumber.ShouldBe(customer.PhoneNumber);
        
    }

    [Fact]
    public async Task Handler_Should_Return_CustomerNotFound()
    {
        //Arrange
        
        var query = new GetCustomerByIdQuery(1);
        
        
        //Act
        var result =  _handler.Handle(query, CancellationToken.None);

        
        //Assert

        var exception = result.ShouldThrow<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("CustomerEntity");
    }
}