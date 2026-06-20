using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.AvailabilitySchedule.Queries.GetAllAvailabilitySchedules;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.AvailabilityScheduleTests;

public class GetAllAvailabilitySchedulesQueryHandlerTests
{
    private readonly Mock<ILogger<GetAllAvailabilitySchedulesQueryHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly UserServiceDbContext _dbContext;
    private readonly GetAllAvailabilitySchedulesQueryHandler _handler;

    public GetAllAvailabilitySchedulesQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetAllAvailabilitySchedulesQueryHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new GetAllAvailabilitySchedulesQueryHandler(_mockLogger.Object, _dbContext,
            _mockCurrentUserService.Object);
    }


    [Fact]
    public async Task Handler_Should_Return_Schedules_Successfully()
    {
        //Arrange
        var employee = TestDataSeeder.CreateEmployee();
        var schedules = TestDataSeeder.CreateSchedules();
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.AvailabilitySchedules.AddRangeAsync(schedules, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var query = new GetAllAvailabilitySchedulesQuery(1);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        result.ShouldNotBeNull();
        result.HasNextPage.ShouldBe(false);
        result.TotalPages.ShouldBe(1);
        result.TotalCount.ShouldBe(2);

        var schedule = result.Items.FirstOrDefault();
        schedule!.Id.ShouldBe(1);
        schedule.GroupId.ShouldBe(null);
    }
    
    [Fact]
    public async Task Should_Return_BlockedCustomersEmptyList()
    {
        //Arrange
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var query = new GetAllAvailabilitySchedulesQuery(1);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        
        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        
        result.TotalCount.ShouldBe(0);
        result.HasNextPage.ShouldBe(false);
        var scheduleList = result.Items;
        scheduleList.ShouldBeEmpty();
    }
    
        
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_CurrentEmployeeNotFound()
    {
        //Arrange
        
        
        var query = new GetAllAvailabilitySchedulesQuery(1);

        var expectedResponse = new HttpStatusCodeException(HttpStatusCode.NotFound, "Employee not found");
        
        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedResponse);
        
        //Act
        var result = _handler.Handle(query, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"Employee not found");
    }
    
}