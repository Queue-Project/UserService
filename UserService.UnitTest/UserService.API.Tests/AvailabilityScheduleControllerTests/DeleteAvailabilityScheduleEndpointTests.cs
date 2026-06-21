using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.AvailabilitySchedule.Commands.DeleteAvailabilitySchedule;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.AvailabilityScheduleControllerTests;

public class DeleteAvailabilityScheduleEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<AvailabilityScheduleController>> _mockLogger;
    private readonly AvailabilityScheduleController _scheduleController;

    public DeleteAvailabilityScheduleEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<AvailabilityScheduleController>>();
        _scheduleController = new AvailabilityScheduleController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_DeletedSchedule_WithNoContentStatus()
    {
        //Arrange
        var deleteCommand = new DeleteAvailabilityScheduleCommand(1, false);

        _mockMediator.Setup(s => s.Send(deleteCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
       
       
        //Act
        var result = await _scheduleController.DeleteAsync(deleteCommand.Id, deleteCommand.DeleteAllSlots);
       
        //Assert
        result.ShouldBeOfType<NoContentResult>();

    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_ScheduleDoesNotExist()
    {
        var deleteCommand = new DeleteAvailabilityScheduleCommand(1, false);

        
        
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Schedule not found");
    
        _mockMediator
            .Setup(s => s.Send(deleteCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _scheduleController.DeleteAsync(deleteCommand.Id, deleteCommand.DeleteAllSlots);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Schedule not found");

    }

}