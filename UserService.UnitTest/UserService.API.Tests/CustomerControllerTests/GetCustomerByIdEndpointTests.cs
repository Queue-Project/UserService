using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.Customers.Queries.GetCustomerById;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.CustomerControllerTests;

public class GetCustomerByIdEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<CustomerController>> _mockLogger;
    private readonly CustomerController _customerController;

    public GetCustomerByIdEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CustomerController>>();
        _customerController = new CustomerController(_mockLogger.Object, _mockMediator.Object);
    }
    
     [Fact]
    public async Task Should_Return_CustomerById_WithOkStatusCode()
    {
        var cusotmerId = 1;
        var query = new GetCustomerByIdQuery(cusotmerId);

        var expectedResponse = new CustomerResponseModel
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "992923324252"
        };

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _customerController.GetByIdAsync(cusotmerId);

        // Assert
        result.ShouldNotBeNull();
        
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
        
        var returnedValue = okResult.Value.ShouldBeOfType<CustomerResponseModel>();
        returnedValue.Id.ShouldBe(expectedResponse.Id);
        returnedValue.FirstName.ShouldBe(expectedResponse.FirstName);
        returnedValue.LastName.ShouldBe(expectedResponse.LastName);
        returnedValue.PhoneNumber.ShouldBe(expectedResponse.PhoneNumber);
        

    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_CustomerDoesNotExist()
    {
        var customerId = 999;
        var query = new GetCustomerByIdQuery(customerId);
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Customer not found");
    
        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _customerController.GetByIdAsync(customerId);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Customer not found");

    }
}