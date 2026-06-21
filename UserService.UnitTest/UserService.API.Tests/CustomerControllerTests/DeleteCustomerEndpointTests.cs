using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Customers.Commands.DeleteCustomer;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.CustomerControllerTests;

public class DeleteCustomerEndpointTests
{
    
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<CustomerController>> _mockLogger;
    private readonly CustomerController _customerController;

    public DeleteCustomerEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CustomerController>>();
        _customerController = new CustomerController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_DeletedCustomer_WithNoContentStatus()
    {
        //Arrange
        var deleteCommand = new DeleteCustomerCommand(1);

        _mockMediator.Setup(s => s.Send(deleteCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
       
       
        //Act
        var result = await _customerController.DeleteAsync(deleteCommand.Id);
       
        //Assert
        result.ShouldBeOfType<NoContentResult>();

    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_CustomerDoesNotExist()
    {
        int id = 999;
        var deleteCommand = new DeleteCustomerCommand(id);
        
        
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Customer not found");
    
        _mockMediator
            .Setup(s => s.Send(deleteCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _customerController.DeleteAsync(id);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Customer not found");

    }
}