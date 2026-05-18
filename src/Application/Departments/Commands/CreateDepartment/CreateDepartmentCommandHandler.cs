using FinFlow.Application.Common;
using FinFlow.Application.Departments.DTOs;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Departments;
using FinFlow.Domain.Entities;

namespace FinFlow.Application.Departments.Commands.CreateDepartment;

public sealed class CreateDepartmentCommandHandler : ICommandHandler<CreateDepartmentCommand, Result<DepartmentSummaryDto>>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentCommandHandler(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DepartmentSummaryDto>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentId.HasValue)
        {
            var parentExists = await _departmentRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parentExists is null)
                return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.NotFound);
            if (parentExists.IdTenant != request.TenantId)
                return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.NotFound);
            if (!parentExists.IsActive)
                return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.Inactive);
        }

        // Application-side uniqueness check. The DB unique index is the
        // ultimate enforcer (race-safe), this just gives a clean error before
        // hitting it.
        if (await _departmentRepository.NameExistsAsync(request.TenantId, request.Name, excludeDepartmentId: null, cancellationToken))
            return Result.Failure<DepartmentSummaryDto>(DepartmentErrors.DuplicateName);

        var createResult = Department.Create(request.Name, request.TenantId, request.ParentId);
        if (createResult.IsFailure)
            return Result.Failure<DepartmentSummaryDto>(createResult.Error);

        var department = createResult.Value;
        _departmentRepository.Add(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DepartmentSummaryDto(
            department.Id,
            department.Name,
            department.ParentId,
            department.IsActive));
    }
}
