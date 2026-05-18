using FinFlow.Application.Common;
using FinFlow.Application.Departments.DTOs;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Departments;
using FinFlow.Domain.Entities;

namespace FinFlow.Application.Departments.Commands.ActivateDepartment;

public sealed class ActivateDepartmentCommandHandler : ICommandHandler<ActivateDepartmentCommand, Result<DepartmentSummaryDto>>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateDepartmentCommandHandler(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DepartmentSummaryDto>> Handle(ActivateDepartmentCommand request, CancellationToken cancellationToken)
    {
        // Must bypass query filters to load an inactive entity.
        var department = await _departmentRepository.GetEntityByIdIncludingInactiveAsync(
            request.DepartmentId, request.TenantId, cancellationToken);

        if (department is null)
            return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.NotFound);

        // If the parent is inactive, we cannot re-activate a child under it.
        if (department.ParentId.HasValue)
        {
            var parent = await _departmentRepository.GetEntityByIdIncludingInactiveAsync(
                department.ParentId.Value, request.TenantId, cancellationToken);
            if (parent is not null && !parent.IsActive)
                return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.Inactive);
        }

        var activateResult = department.Activate();
        if (activateResult.IsFailure)
            return Result.Failure<DepartmentSummaryDto>(activateResult.Error);

        _departmentRepository.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DepartmentSummaryDto(
            department.Id,
            department.Name,
            department.ParentId,
            department.IsActive));
    }
}
