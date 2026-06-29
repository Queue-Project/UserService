using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.Employees.Queries.GetEmployeeById;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.EmployeeControllerTests;

public class GetEmployeeByIdEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<EmployeeController>> _mockLogger;
    private readonly EmployeeController _employeeController;

    public GetEmployeeByIdEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<EmployeeController>>();
        _employeeController = new EmployeeController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Should_Return_EmployeeById_WithOkStatusCode()
    {
        var employeeId = 1;
        var query = new GetEmployeeByIdQuery(employeeId);

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
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _employeeController.GetByIdAsync(employeeId);

        // Assert
        result.ShouldNotBeNull();

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);

        var returnedValue = okResult.Value.ShouldBeOfType<EmployeeResponseModel>();
        returnedValue.Id.ShouldBe(expectedResponse.Id);
        returnedValue.FirstName.ShouldBe(expectedResponse.FirstName);
        returnedValue.LastName.ShouldBe(expectedResponse.LastName);
        returnedValue.PhoneNumber.ShouldBe(expectedResponse.PhoneNumber);
    }

    [Fact]
    public async Task Should_Return_NotFound_When_EmployeeDoesNotExist()
    {
        var employeeId = 999;
        var query = new GetEmployeeByIdQuery(employeeId);
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found");

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = _employeeController.GetByIdAsync(employeeId);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Employee not found");
    }
}