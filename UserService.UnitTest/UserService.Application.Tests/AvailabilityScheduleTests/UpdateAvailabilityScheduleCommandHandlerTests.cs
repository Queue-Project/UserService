using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.AvailabilitySchedule.Commands.UpdateAvailabilitySchedule;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AvailabilityScheduleTests;

public class UpdateAvailabilityScheduleCommandHandlerTests
{
    private readonly Mock<ILogger<UpdateAvailabilityScheduleCommandHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly UserServiceDbContext _dbContext;
    private readonly UpdateAvailabilityScheduleCommandHandler _handler;

    public UpdateAvailabilityScheduleCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<UpdateAvailabilityScheduleCommandHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new UpdateAvailabilityScheduleCommandHandler
        (_mockLogger.Object, _dbContext,
            _mockCurrentUserService.Object);
    }
    
    [Fact]
    public async Task Handler_Should_Update_Schedule_Successfully()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var existingSchedule = TestDataSeeder.CreateSchedule();
        existingSchedule.EmployeeId = employee.Id;
        await _dbContext.AvailabilitySchedules.AddAsync(existingSchedule);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var newFrom = DateTimeOffset.UtcNow.Date.AddHours(10);
        var newTo = DateTimeOffset.UtcNow.Date.AddHours(18);

        var command = new UpdateAvailabilityScheduleCommand(
            existingSchedule.Id,
            "Updated description",
            RepeatSlot.None,
            0,
            new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(newFrom, newTo)
            },
            false
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingSchedule.Id);
        result.Description.ShouldBe(command.Description);
        result.RepeatSlot.ShouldBe(RepeatSlot.None);
        result.RepeatDuration.ShouldBe(0);
        result.AvailableSlots.ShouldNotBeNull();
        result.AvailableSlots.Count.ShouldBe(1);
        result.AvailableSlots.First().From.ShouldBe(newFrom);
        result.AvailableSlots.First().To.ShouldBe(newTo);

        
    }
    
    [Fact]
    public async Task Handler_Should_Update_All_Slots_In_Group_Successfully()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var groupId = 1;
        var schedules = TestDataSeeder.CreateSchedules();
        foreach (var schedule in schedules)
        {
            schedule.EmployeeId = employee.Id;
            schedule.GroupId = groupId;
        }
        await _dbContext.AvailabilitySchedules.AddRangeAsync(schedules);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var newFrom = DateTimeOffset.UtcNow.Date.AddHours(9);
        var newTo = DateTimeOffset.UtcNow.Date.AddHours(17);

        var command = new UpdateAvailabilityScheduleCommand(
            schedules.First().Id,
            "Updated group description",
            RepeatSlot.None,
            0,
            new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(newFrom, newTo)
            },
            true 
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();

        var updatedSchedules = await _dbContext.AvailabilitySchedules
            .Where(s => s.GroupId == groupId)
            .ToListAsync();
        
        updatedSchedules.Count.ShouldBe(2);
        foreach (var schedule in updatedSchedules)
        {
            schedule.Description.ShouldBe(command.Description);
            schedule.AvailableSlots.First().From.ShouldBe(newFrom);
            schedule.AvailableSlots.First().To.ShouldBe(newTo);
        }
    }
        
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Schedule_Not_Found()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var command = new UpdateAvailabilityScheduleCommand(
            999, 
            "Description",
            RepeatSlot.None,
            0,
            new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1))
            },
            false
        );

        // Act 

        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain(nameof(AvailabilityScheduleEntity));
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Employee_Not_Found()
    {
        // Arrange

        var existingSchedule = TestDataSeeder.CreateSchedule();
        await _dbContext.AvailabilitySchedules.AddAsync(existingSchedule);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        
        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found"); 
        
        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);

        var command = new UpdateAvailabilityScheduleCommand(
            1,
            "Description",
            RepeatSlot.None,
            0,
            new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1))
            },
            false
        );

        // Act 

        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("Employee not found");
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_No_AvailableSlots_Provided()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var schedule = TestDataSeeder.CreateSchedule();
        await _dbContext.AvailabilitySchedules.AddAsync(schedule);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var command = new UpdateAvailabilityScheduleCommand(
            schedule.Id,
            "Description",
            RepeatSlot.None,
            0,
            new List<Interval<DateTimeOffset>>(),
            false
        );

        // Act 

        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<Exception>();

        exception.Message.ShouldContain("At least one available time slot must be provided");
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_From_And_To_Are_Same()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var schedule = TestDataSeeder.CreateSchedule();
        await _dbContext.AvailabilitySchedules.AddAsync(schedule);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var sameTime = DateTimeOffset.UtcNow.Date.AddHours(12);

        var command = new UpdateAvailabilityScheduleCommand(
            schedule.Id,
            "Description",
            RepeatSlot.None,
            0,
            new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(sameTime, sameTime)
            },
            false
        );

        // Act 

        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<Exception>();

        exception.Message.ShouldContain("'From' must be earlier than 'To'");
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_RepeatDuration_Invalid()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var existingSchedule = TestDataSeeder.CreateSchedule();
        existingSchedule.EmployeeId = employee.Id;
        await _dbContext.AvailabilitySchedules.AddAsync(existingSchedule);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var command = new UpdateAvailabilityScheduleCommand(
            existingSchedule.Id,
            "Description",
            RepeatSlot.Daily,
            0, 
            new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1))
            },
            false
        );

        // Act 

        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<Exception>();

        exception.Message.ShouldContain("Repeat duration must be greater than 0");
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Schedule_Overlaps_Existing()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

      
        var existingFrom = DateTimeOffset.UtcNow.Date.AddHours(10);
        var existingTo = DateTimeOffset.UtcNow.Date.AddHours(14);
        
        var existingSchedule = new AvailabilityScheduleEntity
        {
            EmployeeId = employee.Id,
            GroupId = null,
            Description = "Existing schedule",
            AvailableSlots = new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(existingFrom, existingTo)
            },
            RepeatSlot = RepeatSlot.None,
            RepeatDuration = 0,
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.AvailabilitySchedules.AddAsync(existingSchedule);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        
        var scheduleToUpdate = TestDataSeeder.CreateSchedule();
        scheduleToUpdate.AvailableSlots = new List<Interval<DateTimeOffset>>
        {
            new Interval<DateTimeOffset>(DateTimeOffset.UtcNow.Date.AddHours(8), DateTimeOffset.UtcNow.Date.AddHours(9))
        };
        await _dbContext.AvailabilitySchedules.AddAsync(scheduleToUpdate);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

       
        var newFrom = DateTimeOffset.UtcNow.Date.AddHours(11);
        var newTo = DateTimeOffset.UtcNow.Date.AddHours(15);

        var command = new UpdateAvailabilityScheduleCommand(
            scheduleToUpdate.Id,
            "Overlapping update",
            RepeatSlot.None,
            0,
            new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(newFrom, newTo)
            },
            false
        );

        // Act 

        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<Exception>();

        exception.Message.ShouldContain("This time slot already exists or overlaps with an existing schedule");
    }

    [Fact]
    public async Task Handler_Should_Update_Schedule_With_Cross_Day_Time()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var schedule = TestDataSeeder.CreateSchedule();
        await _dbContext.AvailabilitySchedules.AddAsync(schedule);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var newFrom = DateTimeOffset.UtcNow.Date.AddHours(22);
        var newTo = DateTimeOffset.UtcNow.Date.AddHours(2); 

        var command = new UpdateAvailabilityScheduleCommand(
            schedule.Id,
            "Night shift",
            RepeatSlot.None,
            0,
            new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(newFrom, newTo)
            },
            false
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.AvailableSlots.First().From.ShouldBe(newFrom);
        result.AvailableSlots.First().To.ShouldBe(newTo.AddDays(1)); 
    }

    [Fact]
    public async Task Handler_Should_Update_Repeat_Slot_From_None_To_Daily()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var schedule = TestDataSeeder.CreateSchedule();
        schedule.RepeatSlot = RepeatSlot.None;
        schedule.RepeatDuration = 0;
        await _dbContext.AvailabilitySchedules.AddAsync(schedule);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var fromTime = DateTimeOffset.UtcNow.Date.AddHours(9);
        var toTime = DateTimeOffset.UtcNow.Date.AddHours(17);

        var command = new UpdateAvailabilityScheduleCommand(
            schedule.Id,
            "Daily schedule",
            RepeatSlot.Daily,
            3,
            new List<Interval<DateTimeOffset>>
            {
                new Interval<DateTimeOffset>(fromTime, toTime)
            },
            true
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.RepeatSlot.ShouldBe(RepeatSlot.Daily);
        result.RepeatDuration.ShouldBe(3);
        result.GroupId.ShouldNotBeNull();
        
    }
    

}