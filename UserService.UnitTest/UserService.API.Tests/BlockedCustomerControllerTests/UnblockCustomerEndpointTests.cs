using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.BlockedCustomers.Commands.DeleteBlockedCustomer;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.BlockedCustomerControllerTests;

public class UnblockCustomerEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<BlockedCustomerController>> _mockLogger;
    private readonly BlockedCustomerController _blockedCustomerController;

    public UnblockCustomerEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<BlockedCustomerController>>();
        _blockedCustomerController = new BlockedCustomerController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_UnblockedCustomer_WithNoContentStatus()
    {
        //Arrange
        var deleteCommand = new DeleteBlockedCustomerCommand(1);

        _mockMediator.Setup(s => s.Send(deleteCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
       
       
        //Act
        var result = await _blockedCustomerController.Unblock(deleteCommand.Id);
       
        //Assert
        result.ShouldBeOfType<NoContentResult>();

    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_BlockedCustomerDoesNotExist()
    {
        int id = 999;
        var deleteCommand = new DeleteBlockedCustomerCommand(id);
        
        
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Blocked Customer not found");
    
        _mockMediator
            .Setup(s => s.Send(deleteCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _blockedCustomerController.Unblock(id);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Blocked Customer not found");

    }
}