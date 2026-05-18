using FinFlow.Domain.Abstractions;

namespace FinFlow.Domain.Events;

public sealed record DepartmentCreatedDomainEvent(Guid DepartmentId, string Name, Guid IdTenant) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record DepartmentDeactivatedDomainEvent(Guid DepartmentId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record DepartmentActivatedDomainEvent(Guid DepartmentId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record DepartmentRenamedDomainEvent(
    Guid DepartmentId,
    Guid IdTenant,
    string OldName,
    string NewName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record DepartmentParentChangedDomainEvent(
    Guid DepartmentId,
    Guid IdTenant,
    Guid? OldParentId,
    Guid? NewParentId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
