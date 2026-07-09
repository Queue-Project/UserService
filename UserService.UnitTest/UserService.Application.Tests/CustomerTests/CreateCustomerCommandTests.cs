using System.Net;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Customers.Commands.CreateCustomer;
using QUserService.Contracts.Events.CustomerEvent;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.CustomerTests;

public class CreateCustomerCommandTests
{
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<CreateCustomerCommandHandler>> _mockLogger;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandTests()
    {
        _mockLogger = new Mock<ILogger<CreateCustomerCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _handler = new CreateCustomerCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Handler_Should_Create_Customer()
    {
        //Arrange
        var command = new CreateCustomerCommand(
            "Test Firstname",
            "Test Lastname",
            "+992923324252"
        );
        
        //Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        //Assert

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.FirstName.ShouldBe(command.Firstname);
        result.LastName.ShouldBe(command.Lastname);
        result.PhoneNumber.ShouldBe(command.PhoneNumber);



    }
    
    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
        // Arrange

        var command = new CreateCustomerCommand(
            "Test Firstname",
            "Test Lastname",
            "+992923324252"
        );

        // Act

        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert

        result.ShouldNotBeNull();
        _mockPublishEndpoint.Verify(x=>x.Publish(It.IsAny<CustomerCreatedEvent>(), It.IsAny<CancellationToken>()));
        

    }
    
     
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Phone_Number_Already_Exists()
    {
        // Arrange

        var existingUser = TestDataSeeder.CreateCustomer();
        existingUser.PhoneNumber = "+992923324252";
        await _dbContext.Customer.AddAsync(existingUser, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateCustomerCommand(
            "Test Firstname",
            "Test Lastname",
            "+992923324252"
        );

        // Act 
        
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Phone number already exists");

      
    }
}