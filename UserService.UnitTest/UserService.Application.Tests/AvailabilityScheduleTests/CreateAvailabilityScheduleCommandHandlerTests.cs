using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.AvailabilitySchedule.Commands.CreateAvailabilitySchedule;
using QUserService.Domain.Enums;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AvailabilityScheduleTests;

public class CreateAvailabilityScheduleCommandHandlerTests
{
    private readonly Mock<ILogger<CreateAvailabilityScheduleCommandHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly UserServiceDbContext _dbContext;
    private readonly CreateAvailabilityScheduleCommandHandler _handler;

    public CreateAvailabilityScheduleCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CreateAvailabilityScheduleCommandHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new CreateAvailabilityScheduleCommandHandler(_mockLogger.Object, _dbContext,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handler_Should_Create_Schedule_Successfully_With_No_Repeat()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var fromTime = DateTimeOffset.UtcNow.Date.AddHours(9);
        var toTime = DateTimeOffset.UtcNow.Date.AddHours(17); 

        var command = new CreateAvailabilityScheduleCommand
            ("Regular working hours", RepeatSlot.None, 0)
            {
                AvailableSlots = new List<Interval<DateTimeOffset>>
                {
                    new Interval<DateTimeOffset>(fromTime, toTime)
                }
            };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        var schedule = result.First();
        schedule.EmployeeId.ShouldBe(employee.Id);
        schedule.Description.ShouldBe(command.Description);
        schedule.RepeatSlot.ShouldBe(RepeatSlot.None);
        schedule.RepeatDuration.ShouldBe(0);
        schedule.GroupId.ShouldBeNull();
        schedule.AvailableSlots.ShouldNotBeNull();
        schedule.AvailableSlots.Count.ShouldBe(1);
        schedule.AvailableSlots.First().From.ShouldBe(fromTime);
        schedule.AvailableSlots.First().To.ShouldBe(toTime);

  
    }

    [Fact]
    public async Task Handler_Should_Create_Schedule_Successfully_With_Daily_Repeat()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var fromTime = DateTimeOffset.UtcNow.Date.AddHours(10);
        var toTime = DateTimeOffset.UtcNow.Date.AddHours(18); 


        var command = new CreateAvailabilityScheduleCommand
            ("Daily schedule", RepeatSlot.Daily, 5)
            {
                AvailableSlots = new List<Interval<DateTimeOffset>>
                {
                    new Interval<DateTimeOffset>(fromTime, toTime)
                }
            };


        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(6);

        var groupId = result.First().GroupId;
        groupId.ShouldNotBeNull();


        foreach (var schedule in result)
        {
            schedule.GroupId.ShouldBe(groupId);
            schedule.EmployeeId.ShouldBe(employee.Id);
            schedule.RepeatSlot.ShouldBe(RepeatSlot.Daily);
            schedule.RepeatDuration.ShouldBe(5);
            schedule.AvailableSlots.ShouldNotBeNull();
            schedule.AvailableSlots.Count.ShouldBe(1);
        }
    }

    [Fact]
    public async Task Handler_Should_Create_Schedule_Successfully_With_Weekly_Repeat()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var fromTime = DateTimeOffset.UtcNow.Date.AddHours(9);
        var toTime = DateTimeOffset.UtcNow.Date.AddHours(17);


        var command = new CreateAvailabilityScheduleCommand
            ("Weekly schedule", RepeatSlot.Weekly, 3)
            {
                AvailableSlots = new List<Interval<DateTimeOffset>>
                {
                    new Interval<DateTimeOffset>(fromTime, toTime)
                }
            };


        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(4);

        var groupId = result.First().GroupId;
        groupId.ShouldNotBeNull();

        foreach (var schedule in result)
        {
            schedule.GroupId.ShouldBe(groupId);
            schedule.RepeatSlot.ShouldBe(RepeatSlot.Weekly);
            schedule.RepeatDuration.ShouldBe(3);
        }
    }


    [Fact]
    public async Task Handler_Should_Handle_BiWeekly_Repeat()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var fromTime = DateTimeOffset.UtcNow.Date.AddHours(9);
        var toTime = DateTimeOffset.UtcNow.Date.AddHours(17);


        var command = new CreateAvailabilityScheduleCommand
            ("Bi-Weekly schedule", RepeatSlot.BiWeekly, 2)
            {
                AvailableSlots = new List<Interval<DateTimeOffset>>
                {
                    new Interval<DateTimeOffset>(fromTime, toTime)
                }
            };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.First().RepeatSlot.ShouldBe(RepeatSlot.BiWeekly);
        result.First().RepeatDuration.ShouldBe(2);
    }

    [Fact]
    public async Task Handler_Should_Handle_Monthly_Repeat()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var fromTime = DateTimeOffset.UtcNow.Date.AddHours(9);
        var toTime = DateTimeOffset.UtcNow.Date.AddHours(17);

        var command = new CreateAvailabilityScheduleCommand
            ("Monthly schedule", RepeatSlot.Monthly, 3)
            {
                AvailableSlots = new List<Interval<DateTimeOffset>>
                {
                    new Interval<DateTimeOffset>(fromTime, toTime)
                }
            };


        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(4);
        result.First().RepeatSlot.ShouldBe(RepeatSlot.Monthly);
        result.First().RepeatDuration.ShouldBe(3);
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_No_AvailableSlots_Provided()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var command = new CreateAvailabilityScheduleCommand
            ("Invalid schedule", RepeatSlot.None, 0)
            {
                AvailableSlots = new List<Interval<DateTimeOffset>>()
            };

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

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var sameTime = DateTimeOffset.UtcNow.Date.AddHours(12);

        var command = new CreateAvailabilityScheduleCommand
            ("Invalid schedule", RepeatSlot.None, 0)
            {
                AvailableSlots = new List<Interval<DateTimeOffset>>
                {
                    new Interval<DateTimeOffset>(sameTime, sameTime)
                }
            };


        // Act 
        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<Exception>();

        exception.Message.ShouldContain("'From' and 'To' cannot be the same time");
        
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_RepeatDuration_Is_Invalid()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var fromTime = DateTimeOffset.UtcNow.Date.AddHours(9);
        var toTime = DateTimeOffset.UtcNow.Date.AddHours(17);

        var command = new CreateAvailabilityScheduleCommand
            ("Invalid repeat schedule", RepeatSlot.Daily, 0)
            {
                AvailableSlots = new List<Interval<DateTimeOffset>>
                {
                    new Interval<DateTimeOffset>(fromTime, toTime)
                }
            };

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

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);


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


        var newFrom = DateTimeOffset.UtcNow.Date.AddHours(11);
        var newTo = DateTimeOffset.UtcNow.Date.AddHours(15);

        var command = new CreateAvailabilityScheduleCommand
            ("Overlapping schedule", RepeatSlot.None, 0)
            {
                AvailableSlots = new List<Interval<DateTimeOffset>>
                {
                    new Interval<DateTimeOffset>(newFrom, newTo)
                }
            };


        // Act 
        var result = _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<Exception>();

        exception.Message.ShouldContain("This time slot already exists or overlaps with an existing schedule");
    }

    [Fact]
    public async Task Handler_Should_Create_Schedule_With_Cross_Day_Time()
    {
        // Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService
            .Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var fromTime = DateTimeOffset.UtcNow.Date.AddHours(22);
        var toTime = DateTimeOffset.UtcNow.Date.AddHours(2);

        var command = new CreateAvailabilityScheduleCommand
            ("Night shift", RepeatSlot.None, 0)
            {
                AvailableSlots = new List<Interval<DateTimeOffset>>
                {
                    new Interval<DateTimeOffset>(fromTime, toTime)
                }
            };


        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        var schedule = result.First();
        schedule.AvailableSlots.First().From.ShouldBe(fromTime);
        schedule.AvailableSlots.First().To.ShouldBe(toTime.AddDays(1));
    }
}