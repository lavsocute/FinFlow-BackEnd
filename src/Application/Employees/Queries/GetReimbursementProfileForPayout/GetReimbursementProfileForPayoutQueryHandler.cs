using FinFlow.Application.Common.Security;
using FinFlow.Application.Employees.DTOs;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Audit;
using FinFlow.Domain.Employees;
using FinFlow.Domain.Entities;
using MediatR;

namespace FinFlow.Application.Employees.Queries.GetReimbursementProfileForPayout;

internal sealed class GetReimbursementProfileForPayoutQueryHandler
    : IRequestHandler<GetReimbursementProfileForPayoutQuery, Result<ReimbursementProfilePayoutResponse>>
{
    private readonly IEmployeeReimbursementProfileRepository _repository;
    private readonly IPiiEncryptionService _piiEncryption;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetReimbursementProfileForPayoutQueryHandler(
        IEmployeeReimbursementProfileRepository repository,
        IPiiEncryptionService piiEncryption,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _piiEncryption = piiEncryption;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReimbursementProfilePayoutResponse>> Handle(
        GetReimbursementProfileForPayoutQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByMembershipIdAsync(request.MembershipId, cancellationToken);
        if (profile is null || !profile.HasBankInfo)
            return Result.Failure<ReimbursementProfilePayoutResponse>(ProfileErrors.NotFound);

        var bank = VietnamBanks.Find(profile.BankCode!);
        if (bank is null)
            return Result.Failure<ReimbursementProfilePayoutResponse>(ProfileErrors.UnsupportedBankCode);

        var accountNumber = _piiEncryption.Decrypt(profile.BankAccountNumberEncrypted!);

        // Audit log: record the access. Stores last-4 only in metadata.
        var auditLog = AuditLog.Create(
            action: "EMPLOYEE_BANK_INFO_ACCESSED",
            entityType: "EmployeeReimbursementProfile",
            entityId: profile.Id.ToString(),
            newValue: $"{{\"membershipId\":\"{profile.MembershipId}\",\"bankCode\":\"{profile.BankCode}\",\"last4\":\"{profile.BankAccountLast4}\"}}",
            idTenant: request.TenantId,
            idAccount: request.AccountantAccountId);
        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ReimbursementProfilePayoutResponse(
            MembershipId: profile.MembershipId,
            BankCode: profile.BankCode!,
            BankName: bank.Name,
            BankAccountNumber: accountNumber,
            BankAccountHolderName: profile.BankAccountHolderName ?? string.Empty,
            BankBranch: profile.BankBranch));
    }
}
