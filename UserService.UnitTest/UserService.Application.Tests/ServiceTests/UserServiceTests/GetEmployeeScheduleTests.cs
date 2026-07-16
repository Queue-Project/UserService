using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Contracts.Requests.EmployeeRequests;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.ServiceTests.UserServiceTests;

public class GetEmployeeScheduleTests
{
    private readonly Mock<ILogger<QUserService.Application.Services.UserService>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly QUserService.Application.Services.UserService _userService;

    public GetEmployeeScheduleTests()
    {
        _mockLogger = new Mock<ILogger<QUserService.Application.Services.UserService>>();
        _dbContext = TestDbContextFactory.Create();
        _userService = new QUserService.Application.Services.UserService(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task GetEmployeeSchedule_Should_Return_Schedule_When_Exists()
    {
        // Arrange
        var employeeId = 1;
        var date = DateTimeOffset.UtcNow.Date;
        var fromTime = date.AddHours(9);
        var toTime = date.AddHours(17);

        var schedule = new AvailabilityScheduleEntity
        {
            EmployeeId = employeeId,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(fromTime, toTime)
            }
        };
        await _dbContext.AvailabilitySchedules.AddAsync(schedule);
        await _dbContext.SaveChangesAsync();

      
        
        var request = new EmployeeScheduleRequest
        {
            EmployeeId = employeeId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Act
        var result = await _userService.GetEmployeeSchedule(request);

        // Assert
        result.ShouldNotBeNull();
        result.EmployeeId.ShouldBe(employeeId);
        result.Date.ShouldBe(request.Date);
        result.Schedules.ShouldNotBeNull();
        result.Schedules.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetEmployeeSchedule_Should_Return_Empty_When_No_Schedule_For_Date()
    {
        // Arrange
        var employeeId = 1;
        var date = DateTimeOffset.UtcNow.Date;

        var schedule = new AvailabilityScheduleEntity
        {
            EmployeeId = employeeId,
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(
                    date.AddDays(1).AddHours(9), 
                    date.AddDays(1).AddHours(17))
            }
        };
        await _dbContext.AvailabilitySchedules.AddAsync(schedule);
        await _dbContext.SaveChangesAsync();

        var request = new EmployeeScheduleRequest
        {
            EmployeeId = employeeId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Act
        var result = await _userService.GetEmployeeSchedule(request);

        // Assert
        result.ShouldNotBeNull();
        result.EmployeeId.ShouldBe(employeeId);
        result.Date.ShouldBe(request.Date);
        result.Schedules.ShouldNotBeNull();
        result.Schedules.ShouldBeEmpty();
    }
}