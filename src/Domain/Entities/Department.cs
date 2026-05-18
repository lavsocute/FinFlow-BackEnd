using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Events;
using FinFlow.Domain.Interfaces;

namespace FinFlow.Domain.Entities;

public sealed class Department : Entity, IMultiTenant, ISoftDeletable
{
    private const int MaxNameLength = 100;

    private Department(Guid id, string name, Guid idTenant, Guid? parentId)
    {
        Id = id;
        Name = name;
        IdTenant = idTenant;
        ParentId = parentId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    private Department() { }

    public string Name { get; private set; } = null!;
    public Guid IdTenant { get; private set; }
    public Guid? ParentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    public static Result<Department> Create(string name, Guid idTenant, Guid? parentId = null)
    {
        if (idTenant == Guid.Empty)
            return Result.Failure<Department>(DepartmentErrors.TenantRequired);
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Department>(DepartmentErrors.NameRequired);
        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
            return Result.Failure<Department>(DepartmentErrors.NameTooLong);

        var department = new Department(Guid.NewGuid(), trimmed, idTenant, parentId);
        department.RaiseDomainEvent(new DepartmentCreatedDomainEvent(department.Id, department.Name, department.IdTenant));
        return department;
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(DepartmentErrors.NameRequired);
        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
            return Result.Failure(DepartmentErrors.NameTooLong);
        if (string.Equals(Name, trimmed, StringComparison.Ordinal))
            return Result.Success();   // idempotent

        var oldName = Name;
        Name = trimmed;
        RaiseDomainEvent(new DepartmentRenamedDomainEvent(Id, IdTenant, oldName, Name));
        return Result.Success();
    }

    /// <summary>
    /// Change the parent in the tree. Cycle detection is enforced at the
    /// application layer (it needs to query the full ancestor chain). This
    /// method only blocks the trivial self-loop and applies the change.
    /// </summary>
    public Result ChangeParent(Guid? parentId)
    {
        if (parentId == Id)
            return Result.Failure(DepartmentErrors.CannotBeOwnParent);
        if (Nullable.Equals(ParentId, parentId))
            return Result.Success();   // idempotent

        var oldParentId = ParentId;
        ParentId = parentId;
        RaiseDomainEvent(new DepartmentParentChangedDomainEvent(Id, IdTenant, oldParentId, parentId));
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive) return Result.Failure(DepartmentErrors.AlreadyDeactivated);
        IsActive = false;
        RaiseDomainEvent(new DepartmentDeactivatedDomainEvent(Id));
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive) return Result.Failure(DepartmentErrors.AlreadyActive);
        IsActive = true;
        RaiseDomainEvent(new DepartmentActivatedDomainEvent(Id));
        return Result.Success();
    }
}
