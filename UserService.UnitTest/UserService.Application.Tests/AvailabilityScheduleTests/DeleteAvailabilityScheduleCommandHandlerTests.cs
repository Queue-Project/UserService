using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.AvailabilitySchedule.Commands.DeleteAvailabilitySchedule;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AvailabilityScheduleTests;

public class DeleteAvailabilityScheduleCommandHandlerTests
{
    private readonly Mock<ILogger<DeleteAvailabilityScheduleCommandHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly UserServiceDbContext _dbContext;
    private readonly DeleteAvailabilityScheduleCommandHandler _handler;

    public DeleteAvailabilityScheduleCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<DeleteAvailabilityScheduleCommandHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new DeleteAvailabilityScheduleCommandHandler(_mockLogger.Object, _dbContext,
            _mockCurrentUserService.Object);
    }
    
    [Fact]
    public async Task Handler_Should_Delete_Schedule_Single_Slot()
    {
        //Arrange
        var schedule = TestDataSeeder.CreateSchedule();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.AvailabilitySchedules.AddAsync(schedule, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteAvailabilityScheduleCommand(1, false);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        

        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert
        result.ShouldBe(true);
    }
    
    [Fact]
    public async Task Handler_Should_Delete_Schedule_All_Slots()
    {
        //Arrange
        var schedules = TestDataSeeder.CreateSchedules();
        var employee = TestDataSeeder.CreateEmployee();

        foreach (var schedule in schedules)
        {
            schedule.GroupId = 1;
        }
        
        await _dbContext.AvailabilitySchedules.AddRangeAsync(schedules, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteAvailabilityScheduleCommand(1, true);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        

        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert
        result.ShouldBe(true);
    }
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Employee_Not_Found()
    {
        //Arrange
        
        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found");
        
        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);
        
        var command = new DeleteAvailabilityScheduleCommand(1, false);
        
        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"Employee not found");
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Schedule_NotFound()
    {
        //Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteAvailabilityScheduleCommand(1, false);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        

        //Act
        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("AvailabilityScheduleEntity");
    }


    [Fact]
    public async Task Handler_Should_Throw_Exception_When_GroupId_Null()
    {
        //Arrange
        var schedules = TestDataSeeder.CreateSchedules();
        var employee = TestDataSeeder.CreateEmployee();
        
        await _dbContext.AvailabilitySchedules.AddRangeAsync(schedules, CancellationToken.None);
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteAvailabilityScheduleCommand(1, true);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        

        //Act
        var result =  _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe("This schedule is not part of a group, nothing to delete in group.");
    }
    
}