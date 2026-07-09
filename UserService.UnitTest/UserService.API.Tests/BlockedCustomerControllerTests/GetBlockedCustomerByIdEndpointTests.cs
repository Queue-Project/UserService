using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.BlockedCustomers.Queries.GetBlockedCustomerById;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.BlockedCustomerControllerTests;

public class GetBlockedCustomerByIdEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<BlockedCustomerController>> _mockLogger;
    private readonly BlockedCustomerController _blockedCustomerController;

    public GetBlockedCustomerByIdEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<BlockedCustomerController>>();
        _blockedCustomerController = new BlockedCustomerController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_BlockedCustomerById_WithOkStatusCode()
    {
        var query = new GetBlockedCustomerByIdQuery(1);

        var expectedResponse = new BlockedCustomerResponseModel
        {
            Id = 1,
            CustomerId = 1,
            CompanyId = 1,
            Reason = "Test Reason",
            BannedUntil = DateTime.UtcNow.Date.AddMonths(1),
            DoesBanForever = false
            
        };

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _blockedCustomerController.GetById(1);

        // Assert
        result.ShouldNotBeNull();
        
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
        
        var returnedValue = okResult.Value.ShouldBeOfType<BlockedCustomerResponseModel>();
        returnedValue.Id.ShouldBe(expectedResponse.Id);
        returnedValue.CustomerId.ShouldBe(expectedResponse.CustomerId);
        returnedValue.CompanyId.ShouldBe(expectedResponse.CompanyId);
        returnedValue.BannedUntil.ShouldBe(expectedResponse.BannedUntil);
        returnedValue.DoesBanForever.ShouldBe(expectedResponse.DoesBanForever);
        returnedValue.Reason.ShouldBe(expectedResponse.Reason);

    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_BlockedCustomerDoesNotExist()
    {
        var customerId = 999;
        var query = new GetBlockedCustomerByIdQuery(customerId);
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Blocked Customer not found");
    
        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _blockedCustomerController.GetById(customerId);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Blocked Customer not found");

    }
}