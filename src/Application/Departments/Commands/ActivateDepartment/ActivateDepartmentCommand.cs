using FinFlow.Application.Common;
using FinFlow.Application.Departments.DTOs;
using FinFlow.Domain.Abstractions;

namespace FinFlow.Application.Departments.Commands.ActivateDepartment;

public record ActivateDepartmentCommand(
    Guid DepartmentId,
    Guid TenantId)
    : ICommand<Result<DepartmentSummaryDto>>;
