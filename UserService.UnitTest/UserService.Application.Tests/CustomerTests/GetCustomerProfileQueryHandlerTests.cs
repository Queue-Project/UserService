using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.Customers.Queries.GetCustomerProfile;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.CustomerTests;

public class GetCustomerProfileQueryHandlerTests
{
    private readonly Mock<ILogger<GetCustomerProfileQueryHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly GetCustomerProfileQueryHandler _handler;

    public GetCustomerProfileQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetCustomerProfileQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new GetCustomerProfileQueryHandler(_mockLogger.Object, _dbContext, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_CustomerProfile()
    {
        //Arrange
        var customer = TestDataSeeder.CreateCustomer();
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetCustomerProfileQuery();
        
        _mockCurrentUserService.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.FirstName.ShouldBe(customer.FirstName);
        result.LastName.ShouldBe(customer.LastName);
        result.EmailAddress.ShouldBe(user.EmailAddress);
        result.PhoneNumber.ShouldBe(customer.PhoneNumber);
        result.DateOfBirth.ShouldBe(customer.DateOfBirth);
        result.Gender.ShouldBe(customer.Gender);
        result.Country.ShouldBe(customer.Country);
        result.City.ShouldBe(customer.City);
        result.Address.ShouldBe(customer.Address);
        result.PostalCode.ShouldBe(customer.PostalCode);
    }

    [Fact]
    public async Task Handler_Should_Return_CurrentCustomerNotFound()
    {
        //Arrange
        
        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Customer not found");

        _mockCurrentUserService.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);

        var query = new GetCustomerProfileQuery();
        

        //Act
        var result = _handler.Handle(query, CancellationToken.None);


        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Customer not found");
    }
}