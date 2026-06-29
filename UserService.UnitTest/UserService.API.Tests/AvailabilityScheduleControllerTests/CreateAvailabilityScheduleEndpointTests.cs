using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.AvailabilitySchedule.Commands.CreateAvailabilitySchedule;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;
using Shouldly;
using DayOfWeek = System.DayOfWeek;

namespace UserService.UnitTest.UserService.API.Tests.AvailabilityScheduleControllerTests;

public class CreateAvailabilityScheduleEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<AvailabilityScheduleController>> _mockLogger;
    private readonly AvailabilityScheduleController _scheduleController;

    public CreateAvailabilityScheduleEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<AvailabilityScheduleController>>();
        _scheduleController = new AvailabilityScheduleController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Should_Return_CreatedSchedule_WithOkStatusCode()
    {
        // Arrange
        var createScheduleCommand = new CreateAvailabilityScheduleCommand("Before lunch time working hours",
            RepeatSlot.None,
            null)
        {
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(8),
                    DateTimeOffset.UtcNow.Date.AddHours(12))
            }
        };


        var expectedResponse = new List<AvailabilityScheduleResponseModel>
        {
            new AvailabilityScheduleResponseModel
            {
                Id = 1,
                EmployeeId = 1,
                GroupId = null,
                Description = "Before lunch time working hours",
                RepeatDuration = null,
                RepeatSlot = RepeatSlot.None,
                DayOfWeek = DayOfWeek.Saturday,
                AvailableSlots = new List<Interval<DateTimeOffset>>
                {
                    new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(8),
                        DateTimeOffset.UtcNow.Date.AddHours(12))
                }
            }
        };

        _mockMediator
            .Setup(m => m.Send(createScheduleCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _scheduleController.PostAsync(createScheduleCommand);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<List<AvailabilityScheduleResponseModel>>();
        var schedule = returnValue.FirstOrDefault();
        var responseSchedule = expectedResponse.FirstOrDefault();

        schedule!.Id.ShouldBe(responseSchedule!.Id);
        schedule.EmployeeId.ShouldBe(responseSchedule.EmployeeId);
        schedule.Description.ShouldBe(responseSchedule.Description);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_InvalidCommand()
    {
        // Arrange
        var createScheduleCommand = new CreateAvailabilityScheduleCommand("Before lunch time working hours",
            RepeatSlot.None,
            null)
        {
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(8),
                    DateTimeOffset.UtcNow.Date.AddHours(12))
            }
        };

        _mockMediator
            .Setup(m => m.Send(createScheduleCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed"));


        //Act
        var result = _scheduleController.PostAsync(createScheduleCommand);

        //Assert
        var exception = result.ShouldThrow<FluentValidation.ValidationException>();
        exception.Message.ShouldBe("Validation failed");
    }
}