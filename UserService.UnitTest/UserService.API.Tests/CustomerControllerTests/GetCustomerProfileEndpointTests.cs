using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.Customers.Queries.GetCustomerById;
using QUserService.Application.UseCases.Customers.Queries.GetCustomerProfile;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.CustomerControllerTests;

public class GetCustomerProfileEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<CustomerController>> _mockLogger;
    private readonly CustomerController _customerController;

    public GetCustomerProfileEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CustomerController>>();
        _customerController = new CustomerController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_CustomerProfile_WithOkStatusCode()
    {
        var query = new GetCustomerProfileQuery();

        var expectedResponse = new CustomerProfileResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "992923324252",
            EmailAddress = "test@gmail.com",
            Country = "Test Country",
            City = "Test City",
            Address = "Test Address",
            DateOfBirth = new DateTime(2006,06,06),
            Gender = "Male",
            PostalCode = "666666",
            CreatedAt = DateTime.UtcNow.Date.AddHours(5),
            UpdatedAt = DateTime.UtcNow.Date.AddHours(6)
        };

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _customerController.GetCustomerProfile();

        // Assert
        result.ShouldNotBeNull();
        
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
        
        var returnedValue = okResult.Value.ShouldBeOfType<CustomerProfileResponse>();
        returnedValue.Id.ShouldBe(expectedResponse.Id);
        returnedValue.FirstName.ShouldBe(expectedResponse.FirstName);
        returnedValue.LastName.ShouldBe(expectedResponse.LastName);
        returnedValue.PhoneNumber.ShouldBe(expectedResponse.PhoneNumber);
        returnedValue.Country.ShouldBe(expectedResponse.Country);
        returnedValue.City.ShouldBe(expectedResponse.City);
        returnedValue.Address.ShouldBe(expectedResponse.Address);
        returnedValue.EmailAddress.ShouldBe(expectedResponse.EmailAddress);
        
    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_CompanyDoesNotExist()
    {
        var query = new GetCustomerProfileQuery();
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Customer not found");
    
        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _customerController.GetCustomerProfile();

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Customer not found");

    }
}