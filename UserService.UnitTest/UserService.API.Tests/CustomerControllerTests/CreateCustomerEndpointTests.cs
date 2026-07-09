using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.Customers.Commands.CreateCustomer;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.CustomerControllerTests;

public class CreateCustomerEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<CustomerController>> _mockLogger;
    private readonly CustomerController _customerController;

    public CreateCustomerEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CustomerController>>();
        _customerController = new CustomerController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Should_Return_CreatedCustomer_WithOkStatusCode()
    {
        // Arrange
        var createCustomerCommand = new CreateCustomerCommand
        (
            "Test Firstname",
            "Test Lastname",
            "+992923324252"
        );

        var expectedResponse = new CustomerResponseModel
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "992923324252"
        };

        _mockMediator
            .Setup(m => m.Send(createCustomerCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _customerController.PostAsync(createCustomerCommand);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<CustomerResponseModel>();

        returnValue.Id.ShouldBe(expectedResponse.Id);
        returnValue.FirstName.ShouldBe(expectedResponse.FirstName);
        returnValue.LastName.ShouldBe(expectedResponse.LastName);
        returnValue.PhoneNumber.ShouldBe(expectedResponse.PhoneNumber);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_InvalidCommand()
    {
        // Arrange
        var createCustomerCommand = new CreateCustomerCommand
        (
            "",
            "Test Lastname",
            "+992923324252"
        );

        _mockMediator
            .Setup(m => m.Send(createCustomerCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed"));


        //Act
        var result = _customerController.PostAsync(createCustomerCommand);

        //Assert
        var exception = result.ShouldThrow<FluentValidation.ValidationException>();
        exception.Message.ShouldBe("Validation failed");
    }
}