using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Employees.Queries.GetEmployeeById;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.EmployeeTests;

public class GetEmployeeByIdQueryHandlerTests
{
    private readonly Mock<ILogger<GetEmployeeByIdQueryHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly GetEmployeeByIdQueryHandler _handler;

    public GetEmployeeByIdQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetEmployeeByIdQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new GetEmployeeByIdQueryHandler(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task Handler_Should_Return_Employee_Successfully()
    {

        //Arrange
        var customer = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(customer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var query = new GetEmployeeByIdQuery(1);
        
        
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
    public async Task Handler_Should_Return_EmployeeNotFound()
    {
        //Arrange
        
        var query = new GetEmployeeByIdQuery(1);
        
        
        //Act
        var result =  _handler.Handle(query, CancellationToken.None);

        
        //Assert

        var exception = result.ShouldThrow<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("EmployeeEntity");
    }
}