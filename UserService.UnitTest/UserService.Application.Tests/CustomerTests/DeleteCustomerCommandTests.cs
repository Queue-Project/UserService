using System.Net;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Customers.Commands.DeleteCustomer;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.CustomerTests;

public class DeleteCustomerCommandTests
{
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<DeleteCustomerCommandHandler>> _mockLogger;
    private readonly DeleteCustomerCommandHandler _handler;

    public DeleteCustomerCommandTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<DeleteCustomerCommandHandler>>();
        _handler = new DeleteCustomerCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Handler_Should_Delete_Company()
    {
        //Arrange
        var company = TestDataSeeder.CreateCustomer();

        await _dbContext.Customer.AddAsync(company, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteCustomerCommand(1);


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert
        result.ShouldBe(true);
    }


    [Fact]
    public async Task Handler_Should_Return_NotFound()
    {
        //Arrange
        var command = new DeleteCustomerCommand(1);


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = result.ShouldThrow<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}