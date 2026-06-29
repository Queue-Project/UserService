using System.Net;
using BranchService.Contracts.Interfaces;
using BranchService.Contracts.Requests;
using BranchService.Contracts.Responses;
using MagicOnion;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.UseCases.Employees.Queries.GetBranchEmployees;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.EmployeeTests;

public class GetBranchEmployeesQueryHandlerTests
{
    private readonly Mock<ILogger<GetBranchEmployeesQueryHandler>> _mockLogger;
    private readonly Mock<IBranchService> _mockBranchService;
    private readonly UserServiceDbContext _dbContext;
    private readonly GetBranchEmployeesQueryHandler _handler;

    public GetBranchEmployeesQueryHandlerTests()
    {
        _mockBranchService = new Mock<IBranchService>();
        _mockLogger = new Mock<ILogger<GetBranchEmployeesQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new GetBranchEmployeesQueryHandler(_mockLogger.Object, _dbContext, _mockBranchService.Object);
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

        var branchExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            BranchName = "Test Branch",
            IsValid = true,
            ErrorMessage = null
        };


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));

        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchExpectedResponse));

        var query = new GetBranchEmployeesQuery(1, 1, 1);

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

        var branchExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            BranchName = "Test Branch",
            IsValid = true,
            ErrorMessage = null
        };


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));

        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchExpectedResponse));

        var query = new GetBranchEmployeesQuery(1, 1, 1);

        //Act
        var result = await _handler.Handle(query, CancellationToken.None);

        //Assert

        result.TotalCount.ShouldBe(0);
        result.HasNextPage.ShouldBe(false);
        var companyList = result.Items;
        companyList.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Branch_Not_Found()
    {
        // Arrange
        var query = new GetBranchEmployeesQuery(1, 1, 1);


        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };

        var branchExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 99,
            IsValid = false,
            ErrorMessage = "Branch not found",
            BranchName = null
        };


        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));


        _mockBranchService
            .Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchExpectedResponse));

        // Act 
        var result = _handler.Handle(query, CancellationToken.None);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain($"Branch with Id {query.BranchId} not found");
    }

    [Fact]
    public async Task Handler_Should_Throw_Exception_When_Company_Not_Found()
    {
        // Arrange
        var query = new GetBranchEmployeesQuery(1, 1, 1);


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