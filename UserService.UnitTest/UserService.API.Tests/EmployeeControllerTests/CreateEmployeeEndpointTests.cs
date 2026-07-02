using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.Employees.Commands.CreateEmployee;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.EmployeeControllerTests;

public class CreateEmployeeEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<EmployeeController>> _mockLogger;
    private readonly EmployeeController _employeeController;

    public CreateEmployeeEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<EmployeeController>>();
        _employeeController = new EmployeeController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Should_Return_CreatedEmployee_WithOkStatusCode()
    {
        // Arrange
        var createEmployeeCommand = new CreateEmployeeCommand
        (
            1,
            1,
            "Test Firstname",
            "Test Lastname",
            "Developer",
            "+992923324252"
        );

        var expectedResponse = new EmployeeResponseModel
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Developer",
            PhoneNumber = "992923324252"
        };

        _mockMediator
            .Setup(m => m.Send(createEmployeeCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _employeeController.PostAsync(createEmployeeCommand);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<EmployeeResponseModel>();

        returnValue.Id.ShouldBe(expectedResponse.Id);
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
        var createEmployeeCommand = new CreateEmployeeCommand
        (
            1,
            1,
            "",
            "Test Lastname",
            "Developer",
            "+992923324252"
        );

        _mockMediator
            .Setup(m => m.Send(createEmployeeCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed"));


        //Act
        var result = _employeeController.PostAsync(createEmployeeCommand);

        //Assert
        var exception = result.ShouldThrow<FluentValidation.ValidationException>();
        exception.Message.ShouldBe("Validation failed");
    }
}