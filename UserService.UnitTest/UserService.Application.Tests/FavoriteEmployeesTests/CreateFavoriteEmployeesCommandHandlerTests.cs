using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.FavoriteEmployees.Commands.CreateFavoriteEmployees;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.FavoriteEmployeesTests;

public class CreateFavoriteEmployeesCommandHandlerTests
{
    private readonly Mock<ILogger<CreateFavoriteEmployeesCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly CreateFavoriteEmployeesCommandHandler _handler;

    public CreateFavoriteEmployeesCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CreateFavoriteEmployeesCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new CreateFavoriteEmployeesCommandHandler(_mockLogger.Object, _dbContext,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handler_Should_Add_Employee_To_Favorite_List_Successfully()
    {
        // Arrange
        var customer = TestDataSeeder.CreateCustomer();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);


        var command = new CreateFavoriteEmployeesCommand(
            1
        );
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(true);
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Customer_Not_Found()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Customer not found");
        _mockCurrentUserService.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);


        var command = new CreateFavoriteEmployeesCommand(
            1
        );
        
        // Act
        var result =  _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe(expectedResponse.Message);
    }
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Employee_Not_Found()
    {
        // Arrange
        var customer = TestDataSeeder.CreateCustomer();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);


        var command = new CreateFavoriteEmployeesCommand(
            1
        );
        
        // Act
        var result =  _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe($"Employee with Id {command.EmployeeId} not found");
    }
}