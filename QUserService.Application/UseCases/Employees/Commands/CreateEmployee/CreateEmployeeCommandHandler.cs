using System.Net;
using BranchService.Contracts.Interfaces;
using BranchService.Contracts.Requests;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QUserService.Application.Exceptions;
using QUserService.Application.Interfaces;
using QUserService.Application.Responses;
using QUserService.Contracts;
using QUserService.Contracts.Events.EmployeeEvent;
using QUserService.Domain.Models;

namespace QUserService.Application.UseCases.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeResponseModel>
{
    private readonly ILogger<CreateEmployeeCommandHandler> _logger;
    private readonly IUserServiceApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IBranchService _branchService;
    private readonly ICurrentUserService _currentUserService;

    public CreateEmployeeCommandHandler(ILogger<CreateEmployeeCommandHandler> logger,
        IUserServiceApplicationDbContext dbContext, IPublishEndpoint publishEndpoint, IBranchService branchService,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _branchService = branchService;
        _currentUserService = currentUserService;
    }

    public async Task<EmployeeResponseModel> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding new Employee with name {employeeName}", request.Firstname);
        if (await _dbContext.Employees.FirstOrDefaultAsync(s=>s.PhoneNumber == request.PhoneNumber, cancellationToken) != null)
        {
            _logger.LogWarning("Phone number is already exists.");
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Phone number already exists");
        }
        var currentEmployee = await _currentUserService.GetCurrentEmployeeAsync(_dbContext, cancellationToken);

        
        var serviceResult = await _branchService.CheckCompanyServiceId(new CompanyServiceRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = currentEmployee.CompanyId,
            CompanyServiceId = request.ServiceId,
            RequestedAt = DateTime.UtcNow
        });
        
        if (!serviceResult.IsValid)
        {
            _logger.LogInformation("CompanyService with Id {CompanyServiceId} not found", request.ServiceId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                serviceResult.ErrorMessage ?? "CompanyService not found");
        }

        var branchResult = await _branchService.CheckBranchId(new BranchRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = serviceResult.CompanyId,
            BranchId = request.BranchId,
            RequestedAt = DateTime.UtcNow
        });

        if (!branchResult.IsValid)
        {
            _logger.LogInformation("Branch with Id {BranchId} not found", request.BranchId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                serviceResult.ErrorMessage ?? "Branch not found");
        }

        
        var employee = new EmployeeEntity()
        {
            CompanyId = serviceResult.CompanyId,
            BranchId = branchResult.BranchId,
            FirstName = request.Firstname,
            LastName = request.Lastname,
            Position = request.Position,
            PhoneNumber = request.PhoneNumber,
            ServiceId = request.ServiceId,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Employees.AddAsync(employee, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee {employeeName} added successfully with Id {employeeId}", employee.FirstName,
            employee.Id);

        await _publishEndpoint.Publish(new EmployeeCreatedEvent
        {
            OccurredAt = DateTimeOffset.UtcNow,
            CompanyId = employee.CompanyId,
            BranchId = employee.BranchId,
            EmployeeId = employee.Id,
            ServiceId = employee.ServiceId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Position = employee.Position,
            PhoneNumber = employee.PhoneNumber,
            AuditData = new AuditData
            {
                PerformedByUserId = currentEmployee.Id,
                PerformedByUserName = $"{currentEmployee.FirstName} {currentEmployee.LastName}"
            }
        }, cancellationToken);

        var response = new EmployeeResponseModel()
        {
            Id = employee.Id,
            CompanyId = employee.CompanyId,
            BranchId = employee.BranchId,
            ServiceId = employee.ServiceId.Value,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Position = employee.Position,
            PhoneNumber = employee.PhoneNumber,
        };

        return response;
    }
}