using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.FavoriteEmployees.Commands.DeleteEmployeeFromFavorite;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.FavoriteEmployeesTests;

public class DeleteEmployeeFromFavoriteCommandHandlerTests
{
    private readonly Mock<ILogger<DeleteEmployeeFromFavoriteCommandHandler>> _mockLogger;
    private readonly UserServiceDbContext _dbContext;
    private readonly DeleteEmployeeFromFavoriteCommandHandler _handler;

    public DeleteEmployeeFromFavoriteCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<DeleteEmployeeFromFavoriteCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new DeleteEmployeeFromFavoriteCommandHandler(_mockLogger.Object, _dbContext);
    }

    [Fact]
    public async Task Handler_Should_Delete_Employee_From_Favorite_List_Successfully()
    {
        // Arrange
        var employee = TestDataSeeder.CreateFavoriteEmployee();
        await _dbContext.FavoriteEmployeeEntities.AddAsync(employee, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        

        var command = new DeleteEmployeeFromFavoriteCommand(
            1
        );
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(true);
    }
    
    
    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Employee_Not_Found()
    {
        // Arrange

        var command = new DeleteEmployeeFromFavoriteCommand(
            1
        );
        
        // Act
        var result =  _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe($"Employee with Id {command.EmployeeId} not found in favorite list");
    }
}