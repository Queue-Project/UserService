using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.Customers.Queries.GetAllCustomers;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.CustomerControllerTests;

public class GetAllCustomersEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<CustomerController>> _mockLogger;
    private readonly CustomerController _customerController;

    public GetAllCustomersEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CustomerController>>();
        _customerController = new CustomerController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_AllCustomers_WithOkStatusCode()
    {
        var pageNumber = 1;
        var query = new GetAllCustomersQuery(pageNumber);

        var expectedResponse = new PagedResponse<CustomerResponseModel>
        {
            Items = [
                new CustomerResponseModel
                {
                    Id = 1,
                    FirstName = "Test Firstname",
                    LastName = "Test Lastname",
                    PhoneNumber = "992923324252"
                },
                new CustomerResponseModel
                {
                    Id = 2,
                    FirstName = "Test Firstname2",
                    LastName = "Test Lastname2",
                    PhoneNumber = "992923324251"
                }
            ],
            PageNumber = 1,
            PageSize = 15
        };

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _customerController.GetAllAsync(pageNumber);

        // Assert
        result.ShouldNotBeNull();
        
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
        
        var returnedValue = okResult.Value.ShouldBeOfType<PagedResponse<CustomerResponseModel>>();
        returnedValue.Items.Count.ShouldBe(2);
        returnedValue.HasNextPage.ShouldBe(false);
        

    }
}