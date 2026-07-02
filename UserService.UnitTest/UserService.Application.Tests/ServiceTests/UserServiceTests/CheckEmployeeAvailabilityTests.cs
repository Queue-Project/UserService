using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.EmployeeRequests;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class CheckEmployeeAvailabilityTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public CheckEmployeeAvailabilityTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task CheckEmployeeAvailability_Should_Return_Available_When_Slot_Is_Within_Working_Hours()
    {
        // Arrange
        var employeeId = 1;
        var fromTime = DateTimeOffset.UtcNow.Date.AddHours(9);
        var toTime = DateTimeOffset.UtcNow.Date.AddHours(10);

        var schedule = new AvailabilityScheduleEntity
        {
            EmployeeId = employeeId,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(fromTime, toTime.AddHours(7))
            }
        };
        await _dbContext.AvailabilitySchedules.AddAsync(schedule);
        await _dbContext.SaveChangesAsync();

        var request = new EmployeeAvailabilityRequest
        {
            EmployeeId = employeeId,
            StartTime = fromTime,
            EndTime = toTime
        };

        // Act
        var result = await _userService.CheckEmployeeAvailability(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsAvailable.ShouldBeTrue();
        result.ErrorMessage.ShouldBeNull();
        result.AvailableSlots.ShouldNotBeNull();
        result.AvailableSlots.Count.ShouldBe(1);
    }

    [Fact]
    public async Task CheckEmployeeAvailability_Should_Return_NotAvailable_When_Outside_Working_Hours()
    {
        // Arrange
        var employeeId = 1;
        var fromTime = DateTimeOffset.UtcNow.Date.AddHours(6);
        var toTime = DateTimeOffset.UtcNow.Date.AddHours(7);

        var schedule = new AvailabilityScheduleEntity
        {
            EmployeeId = employeeId,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(9), DateTimeOffset.UtcNow.Date.AddHours(17))
            }
        };
        await _dbContext.AvailabilitySchedules.AddAsync(schedule);
        await _dbContext.SaveChangesAsync();

        var request = new EmployeeAvailabilityRequest
        {
            EmployeeId = employeeId,
            StartTime = fromTime,
            EndTime = toTime
        };

        // Act
        var result = await _userService.CheckEmployeeAvailability(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsAvailable.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("outside working hours");
        result.AvailableSlots.ShouldNotBeNull();
        result.AvailableSlots.Count.ShouldBe(1);
    }
    

  

    [Fact]
    public async Task CheckEmployeeAvailability_Should_Return_Error_When_No_Schedule_Found()
    {
        // Arrange
        var request = new EmployeeAvailabilityRequest
        {
            EmployeeId = 999,
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Act
        var result = await _userService.CheckEmployeeAvailability(request);

        // Assert
        result.ShouldNotBeNull();
        result.IsAvailable.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("No availability schedule found for this employee");
    }
}