using System.Net;
using BranchService.Contracts.Events.Enums;
using BranchService.Contracts.Interfaces;
using BranchService.Contracts.Requests;
using BranchService.Contracts.Responses;
using MagicOnion;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.UseCases.Employees.Commands.DeleteEmployee;
using QUserService.Contracts.Events.EmployeeEvent;
using QUserService.Domain.Models;
using QUserService.Infrastructure.Persistence.Database;
using Shouldly;
using UserService.UnitTest.UserService.Application.Tests.Infrastructure;

namespace UserService.UnitTest.UserService.Application.Tests.EmployeeTests;

public class DeleteEmployeeCommandHandlerTests
{
    private readonly UserServiceDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<DeleteEmployeeCommandHandler>> _mockLogger;
    private readonly DeleteEmployeeCommandHandler _handler;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IBranchService> _mockBranchService;

    public DeleteEmployeeCommandHandlerTests()
    {
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<DeleteEmployeeCommandHandler>>();
        _mockBranchService = new Mock<IBranchService>();
        _handler = new DeleteEmployeeCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object,
            _mockCurrentUserService.Object, _mockBranchService.Object);
    }

    [Fact]
    public async Task Handler_Should_Delete_Employee()
    {
        //Arrange

        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyCategory = CompanyCategory.Beauty,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };
        _mockBranchService
            .Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));
        var companyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        
        var userEmployee = TestDataSeeder.CreateUserEmployeeRole();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Users.AddAsync(userEmployee, CancellationToken.None);
        await _dbContext.Employees.AddRangeAsync([employee, companyAdmin], CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(companyAdmin);
        
        var command = new DeleteEmployeeCommand(1);


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert
        result.ShouldBe(true);
    }


    [Fact]
    public async Task Handler_Should_Return_NotFound()
    {
        //Arrange
        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyCategory = CompanyCategory.Beauty,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };
        _mockBranchService
            .Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));
        var command = new DeleteEmployeeCommand(1);


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = result.ShouldThrow<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task Handler_Should_Publish_Event()
    {
        //Arrange
        var companyExpectedResponse = new CompanyResponse()
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyCategory = CompanyCategory.Beauty,
            CompanyName = "Test Name",
            IsValid = true,
            ErrorMessage = null
        };
        _mockBranchService
            .Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyExpectedResponse));
        var companyAdmin = TestDataSeeder.CreateEmployeeCompanyAdmin();
        
        var userEmployee = TestDataSeeder.CreateUserEmployeeRole();
        var employee = TestDataSeeder.CreateEmployee();
        await _dbContext.Users.AddAsync(userEmployee, CancellationToken.None);
        await _dbContext.Employees.AddRangeAsync([employee, companyAdmin], CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockCurrentUserService.Setup(s => s.GetCurrentEmployeeAsync(_dbContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(companyAdmin);

        var command = new DeleteEmployeeCommand(1);


        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert

        _mockPublishEndpoint.Verify(s => s.Publish(It.IsAny<EmployeeDeletedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}