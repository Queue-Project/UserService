using System.Net;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Employees.Commands.UpdateEmployee;
using QUserService.Contracts.Events.EmployeeEvent;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.EmployeeTests;

public class UpdateEmployeeCommandHandlerTests
{
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<UpdateEmployeeCommandHandler>> _mockLogger;
    private readonly UpdateEmployeeCommandHandler _handler;

    public UpdateEmployeeCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<UpdateEmployeeCommandHandler>>();
        _handler = new UpdateEmployeeCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object);
    }
    
    [Fact]
    public async Task Handler_Should_Update_Employee()
    {
        //Arrange

        var customer = TestDataSeeder.CreateEmployee();

        await _dbContext.Employees.AddAsync(customer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        
        
        var command = new UpdateEmployeeCommand(
            1,
            "Update Firstname",
            "Update Lastname",
            "Barber",
            "+992923324252");
        
        
        //Act
        var result =await _handler.Handle(command, CancellationToken.None);
        
        
        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);


        var employeeResult= await _dbContext.Employees
            .FirstOrDefaultAsync();

        employeeResult.ShouldNotBeNull();

        employeeResult.Id.ShouldBe(1);
        employeeResult!.FirstName.ShouldBe(command.Firstname);
        employeeResult.LastName.ShouldBe(command.Lastname);
        employeeResult.PhoneNumber.ShouldBe(command.PhoneNumber);
    }


    [Fact]
    public async Task Handler_Should_Return_NotFound()
    {
        var command = new UpdateEmployeeCommand(
            1,
            "Update Firstname",
            "Update Lastname",
            "Barber",
            "+992923324252");

        
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

        var customer = TestDataSeeder.CreateEmployee();

        await _dbContext.Employees.AddAsync(customer, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        
        
        var command = new UpdateEmployeeCommand(
            1,
            "Update Firstname",
            "Update Lastname",
            "Barber",
            "+992923324252");
        
        
        //Act
        var result =await _handler.Handle(command, CancellationToken.None);

        //Assert

        result.ShouldNotBeNull();
        
        _mockPublishEndpoint.Verify(x=>
            x.Publish(It.IsAny<EmployeeUpdatedEvent>(),
                It.IsAny<CancellationToken>()), Times.Once);
        
    }
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Phone_Number_Already_Exists()
    {
        // Arrange

        var existingUser = TestDataSeeder.CreateEmployee();
        existingUser.PhoneNumber = "+992923324252";
        await _dbContext.Employees.AddAsync(existingUser, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateEmployeeCommand(
            1,
            "Update Firstname",
            "Update Lastname",
            "Barber",
            "+992923324252");

        // Act 
        
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("Phone number already exists");

      
    }
}