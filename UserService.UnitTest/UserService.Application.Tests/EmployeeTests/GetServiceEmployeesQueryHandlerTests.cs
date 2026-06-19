using System.Net;
using BranchService.Contracts.Interfaces;
using BranchService.Contracts.Requests;
using BranchService.Contracts.Responses;
using MagicOnion;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Employees.Queries.GetServiceEmployees;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.EmployeeTests;

public class GetServiceEmployeesQueryHandlerTests
{
    private readonly Mock<ILogger<GetServiceEmployeesQueryHandler>> _mockLogger;
    private readonly Mock<IBranchService> _mockBranchService;
    private readonly UserServiceDbContext _dbContext;
    private readonly GetServiceEmployeesQueryHandler _handler;

    public GetServiceEmployeesQueryHandlerTests()
    {
        _mockBranchService = new Mock<IBranchService>();
        _mockLogger = new Mock<ILogger<GetServiceEmployeesQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new GetServiceEmployeesQueryHandler(_mockLogger.Object, _dbContext, _mockBranchService.Object);
    }

    [Fact]
    public async Task Should_Return_BranchEmployees_Successfully()
    {
        //Arrange
        var employees = TestDataSeeder.CreateEmployees();
        await _dbContext.Employees.AddRangeAsync(employees, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };

        var serviceExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            CompanyServiceName = "Test Service",
            IsValid = true,
            ErrorMessage = null
        };


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(serviceExpectedResponse));

        var query = new GetServiceEmployeesQuery(1, 1, 1);

        //Act
        var result = await _handler.Handle(query, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.HasNextPage.ShouldBe(false);
        result.TotalPages.ShouldBe(1);
        result.TotalCount.ShouldBe(3);

        var employee = result.Items.FirstOrDefault();
        employee!.Id.ShouldBe(1);
        employee.FirstName.ShouldBe("Test Firstname");
    }

    [Fact]
    public async Task Should_Return_BranchEmployeeEmptyList()
    {
        //Arrange

        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };

        var serviceExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            CompanyServiceName = "Test Service",
            IsValid = true,
            ErrorMessage = null
        };


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(serviceExpectedResponse));

        var query = new GetServiceEmployeesQuery(1, 1, 1);

        //Act
        var result = await _handler.Handle(query, CancellationToken.None);

        //Assert

        result.TotalCount.ShouldBe(0);
        result.HasNextPage.ShouldBe(false);
        var companyList = result.Items;
        companyList.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_CompanyService_Not_Found()
    {
        // Arrange
        var query = new GetServiceEmployeesQuery(1, 1, 1);


        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };

        var serviceExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 99,
            CompanyServiceName = "Test Service",
            IsValid = false,
            ErrorMessage = "CompanyService not found"
        };


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));


        _mockBranchService
            .Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(serviceExpectedResponse));

        // Act 
        var result = _handler.Handle(query, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"Service with Id {query.ServiceId} not found");
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Company_Not_Found()
    {
        // Arrange
        var query = new GetServiceEmployeesQuery(1, 1, 1);


        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 99,
            CompanyName = "Test Name",
            IsValid = false,
            ErrorMessage = "Company not found"
        };


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));


        // Act 
        var result = _handler.Handle(query, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"Company with Id {query.CompanyId} not found");
    }
}