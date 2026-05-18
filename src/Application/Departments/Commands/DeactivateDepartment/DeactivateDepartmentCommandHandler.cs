using FinFlow.Application.Common;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Budgets;
using FinFlow.Domain.Departments;
using FinFlow.Domain.Entities;
using FinFlow.Domain.TenantMemberships;

namespace FinFlow.Application.Departments.Commands.DeactivateDepartment;

public sealed class DeactivateDepartmentCommandHandler : ICommandHandler<DeactivateDepartmentCommand, Result>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITenantMembershipRepository _membershipRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ITenantMembershipRepository membershipRepository,
        IBudgetRepository budgetRepository,
        IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _membershipRepository = membershipRepository;
        _budgetRepository = budgetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetEntityByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null)
            return Result.Failure(DepartmentErrors.NotFound);

        if (department.IdTenant != request.TenantId)
            return Result.Failure(DepartmentErrors.NotFound);

        // Pre-checks — block if cascade would orphan rows. We surface the
        // first blocker rather than aggregating to keep the error message
        // actionable for the admin.
        if (await _departmentRepository.HasActiveChildrenAsync(department.Id, request.TenantId, cancellationToken))
            return Result.Failure(DepartmentErrors.HasActiveChildren);

        if (await _membershipRepository.HasActiveMembersInDepartmentAsync(request.TenantId, department.Id, cancellationToken))
            return Result.Failure(DepartmentErrors.HasActiveMembers);

        if (await _budgetRepository.HasActiveBudgetsForDepartmentAsync(request.TenantId, department.Id, cancellationToken))
            return Result.Failure(DepartmentErrors.HasActiveBudgets);

        var deactivateResult = department.Deactivate();
        if (deactivateResult.IsFailure)
            return deactivateResult;

        _departmentRepository.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
