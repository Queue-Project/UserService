using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.BlockedCustomers.Queries.GetAllBlockedCustomers;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.BlockedCustomerControllerTests;

public class GetAllBlockedCustomersEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<BlockedCustomerController>> _mockLogger;
    private readonly BlockedCustomerController _blockedCustomerController;

    public GetAllBlockedCustomersEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<BlockedCustomerController>>();
        _blockedCustomerController = new BlockedCustomerController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_AllBlockedCustomers_WithOkStatusCode()
    {
        var pageNumber = 1;
        var query = new GetAllBlockedCustomersQuery(pageNumber);

        var expectedResponse = new PagedResponse<BlockedCustomerResponseModel>
        {
            Items = [
                new BlockedCustomerResponseModel
                {
                    Id = 1,
                    CustomerId = 1,
                    CompanyId = 1,
                    Reason = "Test Reason",
                    BannedUntil = DateTime.UtcNow.Date.AddMonths(1),
                    DoesBanForever = false
            
                },
                new BlockedCustomerResponseModel
                {
                    Id = 2,
                    CustomerId =2,
                    CompanyId = 1,
                    Reason = "Test Reason2",
                    BannedUntil = DateTime.UtcNow.Date.AddMonths(1),
                    DoesBanForever = false
            
                }
            ],
            PageNumber = 1,
            PageSize = 15
        };

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _blockedCustomerController.GetAllAsync(pageNumber);

        // Assert
        result.ShouldNotBeNull();
        
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
        
        var returnedValue = okResult.Value.ShouldBeOfType<PagedResponse<BlockedCustomerResponseModel>>();
        returnedValue.Items.Count.ShouldBe(2);
        returnedValue.HasNextPage.ShouldBe(false);
        

    }
}