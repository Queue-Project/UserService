using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.AvailabilitySchedule.Queries.GetAvailabilityScheduleById;
using QUserService.Domain.Enums;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AvailabilityScheduleTests;

public class GetAvailabilityScheduleByIdQueryHandlerTests
{
    private readonly Mock<ILogger<GetAvailabilityScheduleByIdQueryHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly UserServiceDbContext _dbContext;
    private readonly GetAvailabilityScheduleByIdQueryHandler _handler;

    public GetAvailabilityScheduleByIdQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetAvailabilityScheduleByIdQueryHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new GetAvailabilityScheduleByIdQueryHandler(_mockLogger.Object, _dbContext,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_Schedule_By_Id_Successfully()
    {
        //Arrange
        var employee = TestDataSeeder.CreateEmployee();
        var schedule = TestDataSeeder.CreateSchedule();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.AvailabilitySchedules.AddAsync(schedule, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);


        var query = new GetAvailabilityScheduleByIdQuery(1);
        
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        
        //Assert
        
        result.Id.ShouldBe(1);
        result.EmployeeId.ShouldBe(employee.Id);
        result.GroupId.ShouldBeNull();
        result.RepeatDuration.ShouldBe(0);
        result.RepeatSlot.ShouldBe(RepeatSlot.None);
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Employee_Not_Found()
    {
        //Arrange

        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found");
        
        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);
        
        var query = new GetAvailabilityScheduleByIdQuery(1);
        
        
        //Act
        var result =  _handler.Handle(query, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("Employee not found");
        
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Schedule_Not_Found()
    {
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);


        var query = new GetAvailabilityScheduleByIdQuery(1);
        
        
        //Act
        var result =  _handler.Handle(query, CancellationToken.None);
        
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("AvailabilityScheduleEntity");
        
    }
    
    
}