using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Exceptions;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.AvailabilitySchedule.Queries.GetAvailabilityScheduleById;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;
using Shouldly;
using DayOfWeek = System.DayOfWeek;

namespace UserService.UnitTest.UserService.API.Tests.AvailabilityScheduleControllerTests;

public class GetScheduleByIdEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<AvailabilityScheduleController>> _mockLogger;
    private readonly AvailabilityScheduleController _scheduleController;

    public GetScheduleByIdEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<AvailabilityScheduleController>>();
        _scheduleController = new AvailabilityScheduleController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_ScheduleById_WithOkStatusCode()
    {
        var query = new GetAvailabilityScheduleByIdQuery(1);

       
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

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _scheduleController.GetByIdAsync(1);

        // Assert
        result.ShouldNotBeNull();

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);

        var returnedValue = okResult.Value.ShouldBeOfType<AvailabilityScheduleResponseModel>();
        returnedValue.Id.ShouldBe(expectedResponse.Id);
        returnedValue.Description.ShouldBe(expectedResponse.Description);
        returnedValue.EmployeeId.ShouldBe(expectedResponse.EmployeeId);
        returnedValue.RepeatDuration.ShouldBe(expectedResponse.RepeatDuration);
        returnedValue.RepeatSlot.ShouldBe(expectedResponse.RepeatSlot);
    }

    [Fact]
    public async Task Should_Return_NotFound_When_ScheduleDoesNotExist()
    {
        var query = new GetAvailabilityScheduleByIdQuery(1);
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Schedule not found");

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = _scheduleController.GetByIdAsync(1);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Schedule not found");
    }
}