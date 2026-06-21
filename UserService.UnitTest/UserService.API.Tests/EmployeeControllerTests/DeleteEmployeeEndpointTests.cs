using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Employees.Commands.DeleteEmployee;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.EmployeeControllerTests;

public class DeleteEmployeeEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<EmployeeController>> _mockLogger;
    private readonly EmployeeController _employeeController;

    public DeleteEmployeeEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<EmployeeController>>();
        _employeeController = new EmployeeController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_DeletedEmployee_WithNoContentStatus()
    {
        //Arrange
        var deleteCommand = new DeleteEmployeeCommand(1);

        _mockMediator.Setup(s => s.Send(deleteCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
       
       
        //Act
        var result = await _employeeController.Delete(deleteCommand.Id);
       
        //Assert
        result.ShouldBeOfType<NoContentResult>();

    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_EmployeeDoesNotExist()
    {
        int id = 999;
        var deleteCommand = new DeleteEmployeeCommand(id);
        
        
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found");
    
        _mockMediator
            .Setup(s => s.Send(deleteCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _employeeController.Delete(id);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Employee not found");

    }
}