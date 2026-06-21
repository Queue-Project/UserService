using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.Requests.EmployeeRequest;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.Employees.Commands.UpdateEmployee;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.EmployeeControllerTests;

public class UpdateEmployeeEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<EmployeeController>> _mockLogger;
    private readonly EmployeeController _employeeController;

    public UpdateEmployeeEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<EmployeeController>>();
        _employeeController = new EmployeeController(_mockLogger.Object, _mockMediator.Object);
    }
    
    
       [Fact]
    public async Task Should_Return_UpdatedEmployee_WithOkStatusCode()
    {
        //Arrange
        int id = 1;
        var updateRequest = new UpdateEmployeeRequest
        {
            FirstName = "Update Firstname",
            LastName = "Update Lastname",
            Position = "Update Position",
            PhoneNumber = "+992923224252"
        };


        var expectedResponse = new EmployeeResponseModel
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Update Firstname",
            LastName = "Update Lastname",
            Position = "Update Position",
            PhoneNumber = "992923324252"
        };

        var updateCommand = new UpdateEmployeeCommand(id,
            updateRequest.FirstName,
            updateRequest.LastName,
            updateRequest.Position,
            updateRequest.PhoneNumber);

        _mockMediator.Setup(s => s.Send(updateCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        //Act
        var result = await _employeeController.PutAsync(id, updateRequest);


        //Assert
        result.ShouldNotBeNull();
        var statusCode = result.ShouldBeOfType<OkObjectResult>();
        var returnValue = statusCode.Value.ShouldBeOfType<EmployeeResponseModel>();

        returnValue.Id.ShouldBe(id);
        returnValue.FirstName.ShouldBe(expectedResponse.FirstName);
        returnValue.LastName.ShouldBe(expectedResponse.LastName);
        returnValue.PhoneNumber.ShouldBe(expectedResponse.PhoneNumber);
        returnValue.Position.ShouldBe(expectedResponse.Position);
        returnValue.CompanyId.ShouldBe(expectedResponse.CompanyId);
        returnValue.BranchId.ShouldBe(expectedResponse.BranchId);
        returnValue.ServiceId.ShouldBe(expectedResponse.ServiceId);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_InvalidCommand()
    {
        // Arrange

        int id = 1;
        var updateRequest = new UpdateEmployeeRequest
        {
            FirstName = "",
            LastName = "Update Lastname",
            Position = "Update Position",
            PhoneNumber = "+992923224252"
        };

        var updateCommand = new UpdateEmployeeCommand(id,
            updateRequest.FirstName,
            updateRequest.LastName,
            updateRequest.Position,
            updateRequest.PhoneNumber);


        _mockMediator
            .Setup(m => m.Send(updateCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed"));


        //Act
        var result = _employeeController.PutAsync(id, updateRequest);

        //Assert
        var exception = result.ShouldThrow<FluentValidation.ValidationException>();
        exception.Message.ShouldBe("Validation failed");
    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_EmployeeDoesNotExist()
    {
        int id = 999;
        var updateRequest = new UpdateEmployeeRequest
        {
            FirstName = "Update Firstname",
            LastName = "Update Lastname",
            Position = "Update Position",
            PhoneNumber = "+992923224252"
        };

        var updateCommand = new UpdateEmployeeCommand(id,
            updateRequest.FirstName,
            updateRequest.LastName,
            updateRequest.Position,
            updateRequest.PhoneNumber);
        
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found");
    
        _mockMediator
            .Setup(s => s.Send(updateCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _employeeController.PutAsync(id, updateRequest);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Employee not found");

    }
    
}