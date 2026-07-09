using System.Net;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.BlockedCustomers.Commands.DeleteBlockedCustomer;
using QUserService.Contracts.Events.BlockedCustomerEvent;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.BlockedCustomerTests;

public class DeleteBlockedCustomerCommandHandlerTests
{
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<DeleteBlockedCustomerCommandHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly DeleteBlockedCustomerCommandHandler _handler;

    public DeleteBlockedCustomerCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<DeleteBlockedCustomerCommandHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new DeleteBlockedCustomerCommandHandler(_mockLogger.Object, _dbContext, _mockCurrentUserService.Object, _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Handler_Should_Delete_BlockedCustomer()
    {
        //Arrange
        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteBlockedCustomerCommand(1);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        

        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert
        result.ShouldBe(true);
    }


    [Fact]
    public async Task Handler_Should_Return_NotFound()
    {
        //Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        var command = new DeleteBlockedCustomerCommand(1);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = result.ShouldThrow<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_CurrentEmployeeNotFound()
    {
        //Arrange

        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer);
        
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new DeleteBlockedCustomerCommand( 1);

        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found");
        
        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);
        
        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"Employee not found");
    }
    
    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
        //Arrange
        var blockedCustomer = TestDataSeeder.CreateBlockedCustomer();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.BlockedCustomers.AddAsync(blockedCustomer, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteBlockedCustomerCommand(1);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        

        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert

        _mockPublishEndpoint.Verify(s => s.Publish(It.IsAny<BlockedCustomerDeletedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}