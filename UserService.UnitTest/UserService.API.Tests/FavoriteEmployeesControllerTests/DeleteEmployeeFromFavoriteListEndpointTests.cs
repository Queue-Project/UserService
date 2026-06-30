using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.FavoriteEmployees.Commands.DeleteEmployeeFromFavorite;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.FavoriteEmployeesControllerTests;

public class DeleteEmployeeFromFavoriteListEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<FavoriteEmployeesController>> _mockLogger;
    private readonly FavoriteEmployeesController _employeeController;

    public DeleteEmployeeFromFavoriteListEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<FavoriteEmployeesController>>();
        _employeeController = new FavoriteEmployeesController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_Deleted_Favorite_Employee_WithNoContentStatus()
    {
        //Arrange
        var deleteCommand = new DeleteEmployeeFromFavoriteCommand(1);

        _mockMediator.Setup(s => s.Send(deleteCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
       
       
        //Act
        var result = await _employeeController.DeleteEmployeeFromFavoriteList(deleteCommand.EmployeeId);
       
        //Assert
        result.ShouldBeOfType<NoContentResult>();

    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_EmployeeDoesNotExist()
    {
        int id = 999;
        var deleteCommand = new DeleteEmployeeFromFavoriteCommand(id);
        
        
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found");
    
        _mockMediator
            .Setup(s => s.Send(deleteCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _employeeController.DeleteEmployeeFromFavoriteList(id);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Employee not found");

    }
}