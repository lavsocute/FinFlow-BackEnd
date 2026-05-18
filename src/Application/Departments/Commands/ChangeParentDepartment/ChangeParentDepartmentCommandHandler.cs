using FinFlow.Application.Common;
using FinFlow.Application.Departments.DTOs;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Departments;
using FinFlow.Domain.Entities;

namespace FinFlow.Application.Departments.Commands.ChangeParentDepartment;

public sealed class ChangeParentDepartmentCommandHandler
    : ICommandHandler<ChangeParentDepartmentCommand, Result<DepartmentSummaryDto>>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeParentDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DepartmentSummaryDto>> Handle(
        ChangeParentDepartmentCommand request,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetEntityByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null)
            return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.NotFound);

        if (department.IdTenant != request.TenantId)
            return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.NotFound);

        if (request.NewParentId.HasValue)
        {
            var parent = await _departmentRepository.GetEntityByIdAsync(request.NewParentId.Value, cancellationToken);
            if (parent is null)
                return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.NotFound);
            if (parent.IdTenant != request.TenantId)
                return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.NotFound);
            if (!parent.IsActive)
                return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.Inactive);

            // Cycle detection — walk from new-parent up the ancestor chain;
            // bail out if we ever land on the department being moved.
            if (await WouldCreateCycleAsync(request.DepartmentId, request.NewParentId.Value, request.TenantId, cancellationToken))
                return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.CycleDetected);
        }

        var changeResult = department.ChangeParent(request.NewParentId);
        if (changeResult.IsFailure)
            return Result.Failure<DepartmentSummaryDto>(changeResult.Error);

        _departmentRepository.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DepartmentSummaryDto(
            department.Id,
            department.Name,
            department.ParentId,
            department.IsActive));
    }

    private async Task<bool> WouldCreateCycleAsync(
        Guid departmentId,
        Guid newParentId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (newParentId == departmentId) return true;

        var parentMap = (await _departmentRepository.GetParentMapAsync(tenantId, cancellationToken))
            .ToDictionary(x => x.Id, x => x.ParentId);

        var visited = new HashSet<Guid>();
        Guid? cursor = newParentId;
        while (cursor.HasValue)
        {
            if (cursor.Value == departmentId) return true;
            if (!visited.Add(cursor.Value)) return true;   // pre-existing cycle data
            parentMap.TryGetValue(cursor.Value, out cursor);
        }
        return false;
    }
}
