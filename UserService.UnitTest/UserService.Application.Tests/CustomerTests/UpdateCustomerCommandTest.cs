using System.Net;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Customers.Commands.UpdateCustomer;
using QUserService.Contracts.Events.CustomerEvent;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.CustomerTests;

public class UpdateCustomerCommandTest
{
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<UpdateCustomerCommandHandler>> _mockLogger;
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandTest()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<UpdateCustomerCommandHandler>>();
        _handler = new UpdateCustomerCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object);
    }
    
    [Fact]
    public async Task Handler_Should_Update_Customer()
    {
        //Arrange

        var customer = TestDataSeeder.CreateCustomer();

        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        
        
        var command = new UpdateCustomerCommand(
            1,
            "Update Firstname",
            "Update Lastname",
            "+992986654535");
        
        
        //Act
        var result =await _handler.Handle(command, CancellationToken.None);
        
        
        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);


        var customerResult= await _dbContext.Customer
            .FirstOrDefaultAsync();

        customerResult.ShouldNotBeNull();

        customerResult.Id.ShouldBe(1);
        customerResult!.FirstName.ShouldBe(command.Firstname);
        customerResult.LastName.ShouldBe(command.Lastname);
        customerResult.PhoneNumber.ShouldBe(command.PhoneNumber);
    }


    [Fact]
    public  async Task Handler_Should_Return_NotFound()
    {
        var command = new UpdateCustomerCommand(
            1,
            "Update Firstname",
            "Update Lastname",
            "+992986654535");
        
        
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

        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        
        
        var command = new UpdateCustomerCommand(
            1,
            "Update Firstname",
            "Update Lastname",
            "+992986654535");

        
        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert

        result.ShouldNotBeNull();
        
        _mockPublishEndpoint.Verify(x=>
            x.Publish(It.IsAny<CustomerUpdatedEvent>(),
                It.IsAny<CancellationToken>()), Times.Once);
        
    }
}