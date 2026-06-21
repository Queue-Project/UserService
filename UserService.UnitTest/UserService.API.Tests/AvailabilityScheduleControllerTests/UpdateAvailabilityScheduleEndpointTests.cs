using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.Requests.AvailabilityScheduleRequest;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.AvailabilitySchedule.Commands.UpdateAvailabilitySchedule;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;
using Shouldly;
using DayOfWeek = System.DayOfWeek;

namespace UserService.UnitTest.UserService.API.Tests.AvailabilityScheduleControllerTests;

public class UpdateAvailabilityScheduleEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<AvailabilityScheduleController>> _mockLogger;
    private readonly AvailabilityScheduleController _scheduleController;

    public UpdateAvailabilityScheduleEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<AvailabilityScheduleController>>();
        _scheduleController = new AvailabilityScheduleController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Should_Return_UpdatedSchedule_WithOkStatusCode()
    {
        //Arrange
        int id = 1;
        var updateRequest = new UpdateAvailabilityScheduleRequest
        {
            Description = "Update working hours",
            RepeatSlot = RepeatSlot.None,
            RepeatDuration = null,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(10),
                    DateTimeOffset.UtcNow.Date.AddHours(14))
            }
        };


        var expectedResponse = new AvailabilityScheduleResponseModel
        {
            Id = 1,
            EmployeeId = 1,
            GroupId = null,
            Description = "Update working hours",
            RepeatDuration = null,
            RepeatSlot = RepeatSlot.None,
            DayOfWeek = DayOfWeek.Saturday,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(10),
                    DateTimeOffset.UtcNow.Date.AddHours(14))
            }
        };


        var updateCommand = new UpdateAvailabilityScheduleCommand(id,
            updateRequest.Description,
            updateRequest.RepeatSlot,
            updateRequest.RepeatDuration,
            updateRequest.AvailableSlots,
            false);

        // _mockMediator.Setup(s => s.Send(updateCommand, It.IsAny<CancellationToken>()))
        //     .ReturnsAsync(expectedResponse);

        _mockMediator
            .Setup(m => m.Send(It.Is<UpdateAvailabilityScheduleCommand>(cmd =>
                    cmd.Id == id &&
                    cmd.Description == updateRequest.Description &&
                    cmd.RepeatSlot == updateRequest.RepeatSlot &&
                    cmd.RepeatDuration == updateRequest.RepeatDuration &&
                    cmd.UpdateAllSlots == false &&
                    cmd.AvailableSlots == updateRequest.AvailableSlots),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        //Act
        var result = await _scheduleController.UpdateAsync(id, updateRequest, false);


        //Assert
        result.ShouldNotBeNull();
        var statusCode = result.ShouldBeOfType<OkObjectResult>();
        var returnValue = statusCode.Value.ShouldBeOfType<AvailabilityScheduleResponseModel>();

        returnValue.Id.ShouldBe(id);
        returnValue.Description.ShouldBe(expectedResponse.Description);
        returnValue.DayOfWeek.ShouldBe(expectedResponse.DayOfWeek);
        returnValue.RepeatDuration.ShouldBe(expectedResponse.RepeatDuration);
        returnValue.RepeatSlot.ShouldBe(expectedResponse.RepeatSlot);
        returnValue.EmployeeId.ShouldBe(expectedResponse.EmployeeId);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_InvalidCommand()
    {
        // Arrange

        int id = 1;
        var updateRequest = new UpdateAvailabilityScheduleRequest
        {
            Description = "Update working hours",
            RepeatSlot = RepeatSlot.None,
            RepeatDuration = 1,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(10),
                    DateTimeOffset.UtcNow.Date.AddHours(14))
            }
        };
        
        _mockMediator
            .Setup(m => m.Send(It.Is<UpdateAvailabilityScheduleCommand>(cmd =>
                    cmd.Id == id &&
                    cmd.Description == updateRequest.Description &&
                    cmd.RepeatSlot == updateRequest.RepeatSlot &&
                    cmd.RepeatDuration == updateRequest.RepeatDuration &&
                    cmd.UpdateAllSlots == false &&
                    cmd.AvailableSlots == updateRequest.AvailableSlots),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed"));


        //Act
        var result = _scheduleController.UpdateAsync(id, updateRequest, false);

        //Assert
        var exception = result.ShouldThrow<FluentValidation.ValidationException>();
        exception.Message.ShouldBe("Validation failed");
    }

    [Fact]
    public async Task Should_Return_NotFound_When_ScheduleDoesNotExist()
    {
        int id = 999;
        var updateRequest = new UpdateAvailabilityScheduleRequest
        {
            Description = "Update working hours",
            RepeatSlot = RepeatSlot.None,
            RepeatDuration = 1,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(10),
                    DateTimeOffset.UtcNow.Date.AddHours(14))
            }
        };
        

        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Schedule not found");

        _mockMediator
            .Setup(m => m.Send(It.Is<UpdateAvailabilityScheduleCommand>(cmd =>
                    cmd.Id == id &&
                    cmd.Description == updateRequest.Description &&
                    cmd.RepeatSlot == updateRequest.RepeatSlot &&
                    cmd.RepeatDuration == updateRequest.RepeatDuration &&
                    cmd.UpdateAllSlots == false &&
                    cmd.AvailableSlots == updateRequest.AvailableSlots),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = _scheduleController.UpdateAsync(id, updateRequest, false);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Schedule not found");
    }
}