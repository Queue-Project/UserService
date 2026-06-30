using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.API.Controllers;
using QUserService.Application.Responses;
using QUserService.Application.UseCases.Employees.Queries.GetAllEmployees;
using QUserService.Application.UseCases.FavoriteEmployees.Queries.GetAllEmployeesFromFavoriteList;
using Shouldly;

namespace UserService.UnitTest.UserService.API.Tests.FavoriteEmployeesControllerTests;

public class GetAllEmployeesFromFavoriteListEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<FavoriteEmployeesController>> _mockLogger;
    private readonly FavoriteEmployeesController _employeeController;

    public GetAllEmployeesFromFavoriteListEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<FavoriteEmployeesController>>();
        _employeeController = new FavoriteEmployeesController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_AllEmployees_WithOkStatusCode()
    {
        var pageNumber = 1;
        var query = new GetAllEmployeesFromFavoriteListQuery(pageNumber);

        var expectedResponse = new PagedResponse<EmployeeResponseModel>
        {
            Items = [
                new EmployeeResponseModel
                {
                    Id = 1,
                    CompanyId = 1,
                    BranchId = 1,
                    ServiceId = 1,
                    FirstName = "Test Firstname",
                    LastName = "Test Lastname",
                    Position = "Developer",
                    PhoneNumber = "992923324252"
                },
                new EmployeeResponseModel
                {
                    Id = 2,
                    CompanyId = 1,
                    BranchId = 1,
                    ServiceId = 1,
                    FirstName = "Test Firstname2",
                    LastName = "Test Lastname2",
                    Position = "Developer2",
                    PhoneNumber = "992923324212"
                }
            ],
            PageNumber = 1,
            PageSize = 15
        };

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _employeeController.GetAllEmployeesFromFavoriteListAsync(pageNumber);

        // Assert
        result.ShouldNotBeNull();
        
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
        
        var returnedValue = okResult.Value.ShouldBeOfType<PagedResponse<EmployeeResponseModel>>();
        returnedValue.Items.Count.ShouldBe(2);
        returnedValue.HasNextPage.ShouldBe(false);
        

    }

}