using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.BlockedCustomers.Commands.CreateBlockedCustomer;
using QUserService.Application.UseCases.Customers.Commands.CreateCustomer;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.BlockedCustomerControllerTests;

public class BlockCustomerEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<BlockedCustomerController>> _mockLogger;
    private readonly BlockedCustomerController _customerController;

    public BlockCustomerEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<BlockedCustomerController>>();
        _customerController = new BlockedCustomerController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_BlockedCustomer_WithCreatedStatusCode()
    {
        // Arrange
        var createBlockedCustomerCommand = new CreateBlockedCustomerCommand
        (
            1,
            "Test Reason",
            DateTime.UtcNow.Date.AddMonths(1),
            false
        );

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
            .Setup(m => m.Send(createBlockedCustomerCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _customerController.Block(createBlockedCustomerCommand);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        
        createdResult.ActionName.ShouldBe("GetById");
        
        var returnValue = createdResult.Value.ShouldBeOfType<BlockedCustomerResponseModel>();
        returnValue.CompanyId.ShouldBe(expectedResponse.CompanyId);
        returnValue.CustomerId.ShouldBe(expectedResponse.CompanyId);
    }
    
}