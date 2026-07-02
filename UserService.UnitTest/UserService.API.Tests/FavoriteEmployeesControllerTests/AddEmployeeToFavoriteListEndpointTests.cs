using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.UseCases.FavoriteEmployees.Commands.CreateFavoriteEmployees;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.FavoriteEmployeesControllerTests;

public class AddEmployeeToFavoriteListEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<FavoriteEmployeesController>> _mockLogger;
    private readonly FavoriteEmployeesController _favoriteEmployeesController;

    public AddEmployeeToFavoriteListEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<FavoriteEmployeesController>>();
        _favoriteEmployeesController = new FavoriteEmployeesController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Should_Return_Favorite_Employees_WithOkStatusCode()
    {
        // Arrange
        var createEmployeeCommand = new CreateFavoriteEmployeesCommand
        (
            1
        );

        var expectedResponse = true;
        _mockMediator
            .Setup(m => m.Send(createEmployeeCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _favoriteEmployeesController.AddEmployeeToFavoriteListAsync(createEmployeeCommand.EmployeeId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe("Successfully added employee into favorite list");
        
    }
    
}