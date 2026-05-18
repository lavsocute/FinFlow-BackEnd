using FinFlow.Domain.Abstractions;

namespace FinFlow.Domain.Entities;

public static class DepartmentErrors
{
    public static readonly Error NotFound = new("Department.NotFound", "The department with the specified ID was not found");
    public static readonly Error NameRequired = new("Department.NameRequired", "Department name is required");
    public static readonly Error NameTooLong = new("Department.NameTooLong", "Department name cannot exceed 100 characters");
    public static readonly Error TenantRequired = new("Department.TenantRequired", "Tenant ID is required");
    public static readonly Error Inactive = new("Department.Inactive", "The department is inactive");
    public static readonly Error AlreadyDeactivated = new("Department.AlreadyDeactivated", "The department is already deactivated");
    public static readonly Error AlreadyActive = new("Department.AlreadyActive", "The department is already active");
    public static readonly Error CannotBeOwnParent = new("Department.CannotBeOwnParent", "A department cannot be its own parent");
    public static readonly Error CycleDetected = new("Department.CycleDetected", "Changing parent would create a cycle in the department tree.");
    public static readonly Error HasActiveChildren = new("Department.HasActiveChildren", "Cannot deactivate department while it still has active sub-departments. Re-parent or deactivate them first.");
    public static readonly Error HasActiveMembers = new("Department.HasActiveMembers", "Cannot deactivate department while active members are assigned. Reassign members first.");
    public static readonly Error HasActiveBudgets = new("Department.HasActiveBudgets", "Cannot deactivate department while it has active budgets. Archive the budgets first.");
    public static readonly Error DuplicateName = new("Department.DuplicateName", "A department with this name already exists in the tenant.");
}
