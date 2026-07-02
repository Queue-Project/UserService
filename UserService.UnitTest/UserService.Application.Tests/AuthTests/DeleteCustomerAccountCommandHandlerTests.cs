using System.Net;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.Auth.Commands.DeleteCustomerAccount;
using QUserService.Application.UseCases.Customers.Commands.DeleteCustomer;
using QUserService.Contracts.Events.CustomerEvent;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AuthTests;

public class DeleteCustomerAccountCommandHandlerTests
{
    private readonly Mock<ILogger<DeleteCustomerAccountCommandHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly UserServiceDbContext _dbContext;
    private readonly DeleteCustomerAccountCommandHandler _handler;
    
    public DeleteCustomerAccountCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<DeleteCustomerAccountCommandHandler>>();
        _mockCurrentUser = new Mock<ICurrentUserService>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new DeleteCustomerAccountCommandHandler(_mockLogger.Object, _dbContext, _mockCurrentUser.Object,
            _mockPublishEndpoint.Object);
    }
    
    [Fact]
    public async Task Handler_Should_Delete_Customer()
    {
        //Arrange
        var customer = TestDataSeeder.CreateCustomer();
        var userCustomer = TestDataSeeder.CreateUserCustomer();

        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Users.AddAsync(userCustomer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        _mockCurrentUser.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockCurrentUser.Setup(s => s.GetCurrentUserAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userCustomer);

        var command = new DeleteCustomerAccountCommand();


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert
        result.ShouldBe(true);
    }


    [Fact]
    public async Task Handler_Should_Throw_When_User_NotFound()
    {
        //Arrange
        
        var customer = TestDataSeeder.CreateCustomer();
        var userCustomer = TestDataSeeder.CreateUserCustomer();

        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Users.AddAsync(userCustomer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "User not found");
        
        _mockCurrentUser.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockCurrentUser.Setup(s => s.GetCurrentUserAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);

        var command = new DeleteCustomerAccountCommand();

        //Act
        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = result.ShouldThrow<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Customer_NotFound()
    {
        //Arrange
        
        var userCustomer = TestDataSeeder.CreateUserCustomer();

        await _dbContext.Users.AddAsync(userCustomer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Customer not found");
        
        _mockCurrentUser.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);

        _mockCurrentUser.Setup(s => s.GetCurrentUserAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userCustomer);

        var command = new DeleteCustomerAccountCommand();

        //Act
        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = result.ShouldThrow<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
        //Arrange
        var customer = TestDataSeeder.CreateCustomer();
        var userCustomer = TestDataSeeder.CreateUserCustomer();

        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Users.AddAsync(userCustomer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        _mockCurrentUser.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockCurrentUser.Setup(s => s.GetCurrentUserAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userCustomer);

        var command = new DeleteCustomerAccountCommand();


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        //Assert

        _mockPublishEndpoint.Verify(s => s.Publish(It.IsAny<CustomerDeletedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}