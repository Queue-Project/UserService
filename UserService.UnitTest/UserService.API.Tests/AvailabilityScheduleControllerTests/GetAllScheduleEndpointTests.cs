using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.AvailabilitySchedule.Queries.GetAllAvailabilitySchedules;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;
using Shouldly;
using DayOfWeek = System.DayOfWeek;

namespace UserService.UnitTest.UserService.API.Tests.AvailabilityScheduleControllerTests;

public class GetAllScheduleEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<AvailabilityScheduleController>> _mockLogger;
    private readonly AvailabilityScheduleController _scheduleController;

    public GetAllScheduleEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<AvailabilityScheduleController>>();
        _scheduleController = new AvailabilityScheduleController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_AllEmployees_WithOkStatusCode()
    {
        var pageNumber = 1;
        var query = new GetAllAvailabilitySchedulesQuery(pageNumber);

        var expectedResponse = new PagedResponse<AvailabilityScheduleResponseModel>
        {
            Items = [
                new AvailabilityScheduleResponseModel
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
                },
                new AvailabilityScheduleResponseModel
                {
                    Id = 2,
                    EmployeeId = 1,
                    GroupId = null,
                    Description = "Update working hours",
                    RepeatDuration = null,
                    RepeatSlot = RepeatSlot.None,
                    DayOfWeek = DayOfWeek.Monday,
                    AvailableSlots = new List<Interval<DateTimeOffset>>
                    {
                        new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(10),
                            DateTimeOffset.UtcNow.Date.AddHours(14))
                    }
                }
            ],
            PageNumber = 1,
            PageSize = 15
        };

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _scheduleController.GetAllAsync(pageNumber);

        // Assert
        result.ShouldNotBeNull();
        
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
        
        var returnedValue = okResult.Value.ShouldBeOfType<PagedResponse<AvailabilityScheduleResponseModel>>();
        returnedValue.Items.Count.ShouldBe(2);
        returnedValue.HasNextPage.ShouldBe(false);
        

    }
}