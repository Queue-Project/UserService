using System.Net;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.Customers.Commands.UpdateCustomerProfile;
using QUserService.Contracts.Events.CustomerEvent;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.CustomerTests;

public class UpdateCustomerProfileCommandTests
{
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<UpdateCustomerProfileCommandHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly UpdateCustomerProfileCommandHandler _handler;

    public UpdateCustomerProfileCommandTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockLogger = new Mock<ILogger<UpdateCustomerProfileCommandHandler>>();
        _handler = new UpdateCustomerProfileCommandHandler(_mockLogger.Object, _dbContext,
            _mockCurrentUserService.Object, _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Handler_Should_Update_CustomerProfile()
    {
        //Arrange

        var customer = TestDataSeeder.CreateCustomer();
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new UpdateCustomerProfileCommand(
            "Update Firstname",
            "Update Lastname",
            "+992986654535",
            new DateTime(2006, 06, 06),
            "Male",
            "Test Country",
            "Test City",
            "Test Address",
            "123456");


        _mockCurrentUserService.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);


        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.FirstName.ShouldBe(command.FirstName);
        result.LastName.ShouldBe(command.LastName);
        result.EmailAddress.ShouldBe(user.EmailAddress);
        result.PhoneNumber.ShouldBe(command.PhoneNumber);
        result.DateOfBirth.ShouldBe(command.DateOfBirth);
        result.Gender.ShouldBe(command.Gender);
        result.Country.ShouldBe(command.Country);
        result.City.ShouldBe(command.City);
        result.Address.ShouldBe(command.Address);
        result.PostalCode.ShouldBe(command.PostalCode);
    }

    [Fact]
    public async Task Handler_Should_Return_CurrentCustomerNotFound()
    {
        //Arrange

        var command = new UpdateCustomerProfileCommand(
            "Update Firstname",
            "Update Lastname",
            "+992986654535",
            new DateTime(2006, 06, 06),
            "Male",
            "Test Country",
            "Test City",
            "Test Address",
            "123456");

        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Customer not found");

        _mockCurrentUserService.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);


        //Act
        var result = _handler.Handle(command, CancellationToken.None);


        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Customer not found");
    }

    [Fact]
    public async Task Handler_Should_Publish_CustomerUpdatedEvent()
    {
        var customer = TestDataSeeder.CreateCustomer();
        var user = TestDataSeeder.CreateUserCustomer();
        await _dbContext.Customer.AddAsync(customer, CancellationToken.None);
        await _dbContext.Users.AddAsync(user, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var command = new UpdateCustomerProfileCommand(
            "Update Firstname",
            "Update Lastname",
            "+992986654535",
            new DateTime(2006, 06, 06),
            "Male",
            "Test Country",
            "Test City",
            "Test Address",
            "123456");


        _mockCurrentUserService.Setup(s => s.GetCurrentCustomerAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);


        //Assert
        result.ShouldNotBeNull();

        _mockPublishEndpoint.Verify(s => s.Publish(
                It.IsAny<CustomerUpdatedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}