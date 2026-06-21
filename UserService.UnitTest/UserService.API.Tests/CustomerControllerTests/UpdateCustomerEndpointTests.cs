using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.Requests.CustomerRequest;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.Customers.Commands.UpdateCustomer;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.CustomerControllerTests;

public class UpdateCustomerEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<CustomerController>> _mockLogger;
    private readonly CustomerController _customerController;

    public UpdateCustomerEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CustomerController>>();
        _customerController = new CustomerController(_mockLogger.Object, _mockMediator.Object);
    }
    
     [Fact]
    public async Task Should_Return_UpdatedCustomer_WithOkStatusCode()
    {
        //Arrange
        int id = 1;
        var updateRequest = new UpdateCustomerRequest
        {
            FirstName = "Update Firstname",
            LastName = "Update Lastname",
            PhoneNumber = "+992923224252"
        };


        var expectedResponse = new CustomerResponseModel
        {
            Id = 1,
            FirstName = "Update Firstname",
            LastName = "Update Lastname",
            PhoneNumber = "992923324252"
        };


        var updateCommand = new UpdateCustomerCommand(id,
            updateRequest.FirstName,
            updateRequest.LastName,
            updateRequest.PhoneNumber);

        _mockMediator.Setup(s => s.Send(updateCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        //Act
        var result = await _customerController.PutAsync(id, updateRequest);


        //Assert
        result.ShouldNotBeNull();
        var statusCode = result.ShouldBeOfType<OkObjectResult>();
        var returnValue = statusCode.Value.ShouldBeOfType<CustomerResponseModel>();

        returnValue.Id.ShouldBe(id);
        returnValue.FirstName.ShouldBe(expectedResponse.FirstName);
        returnValue.LastName.ShouldBe(expectedResponse.LastName);
        returnValue.PhoneNumber.ShouldBe(expectedResponse.PhoneNumber);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_InvalidCommand()
    {
        // Arrange

        int id = 1;
        var updateRequest = new UpdateCustomerRequest
        {
            FirstName = "",
            LastName = "Update Lastname",
            PhoneNumber = "+992923224252"
        };

        var updateCommand = new UpdateCustomerCommand(id,
            updateRequest.FirstName,
            updateRequest.LastName,
            updateRequest.PhoneNumber);


        _mockMediator
            .Setup(m => m.Send(updateCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed"));


        //Act
        var result = _customerController.PutAsync(id, updateRequest);

        //Assert
        var exception = result.ShouldThrow<FluentValidation.ValidationException>();
        exception.Message.ShouldBe("Validation failed");
    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_CustomerDoesNotExist()
    {
        int id = 999;
        var updateRequest = new UpdateCustomerRequest
        {
            FirstName = "Update Firstname",
            LastName = "Update Lastname",
            PhoneNumber = "+992923224252"
        };

        var updateCommand = new UpdateCustomerCommand(id,
            updateRequest.FirstName,
            updateRequest.LastName,
            updateRequest.PhoneNumber);
        
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Customer not found");
    
        _mockMediator
            .Setup(s => s.Send(updateCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _customerController.PutAsync(id, updateRequest);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Customer not found");

    }
}