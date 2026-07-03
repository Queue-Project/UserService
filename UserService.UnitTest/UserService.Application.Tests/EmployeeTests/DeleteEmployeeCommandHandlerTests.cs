using System.Net;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Employees.Commands.DeleteEmployee;
using QUserService.Contracts.Events.EmployeeEvent;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.EmployeeTests;

public class DeleteEmployeeCommandHandlerTests
{
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<DeleteEmployeeCommandHandler>> _mockLogger;
    private readonly DeleteEmployeeCommandHandler _handler;

    public DeleteEmployeeCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<DeleteEmployeeCommandHandler>>();
        _handler = new DeleteEmployeeCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Handler_Should_Delete_Employee()
    {
        //Arrange
        var userEmployee = TestDataSeeder.CreateUserEmployeeRole();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Users.AddAsync(userEmployee, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteEmployeeCommand(1);


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert
        result.ShouldBe(true);
    }


    [Fact]
    public async Task Handler_Should_Return_NotFound()
    {
        //Arrange
        var command = new DeleteEmployeeCommand(1);


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = result.ShouldThrow<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
        //Arrange
        var userEmployee = TestDataSeeder.CreateUserEmployeeRole();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Users.AddAsync(userEmployee, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteEmployeeCommand(1);


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert

        _mockPublishEndpoint.Verify(s => s.Publish(It.IsAny<EmployeeDeletedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}