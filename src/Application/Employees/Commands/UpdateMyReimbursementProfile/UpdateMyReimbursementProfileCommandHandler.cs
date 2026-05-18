using FinFlow.Application.Employees.DTOs;
using FinFlow.Application.Employees.Mapping;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Employees;
using MediatR;

namespace FinFlow.Application.Employees.Commands.UpdateMyReimbursementProfile;

internal sealed class UpdateMyReimbursementProfileCommandHandler
    : IRequestHandler<UpdateMyReimbursementProfileCommand, Result<ReimbursementProfileResponse>>
{
    private readonly IEmployeeReimbursementProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMyReimbursementProfileCommandHandler(
        IEmployeeReimbursementProfileRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReimbursementProfileResponse>> Handle(
        UpdateMyReimbursementProfileCommand request,
        CancellationToken cancellationToken)
    {
        // Auto-create profile on first update (lazy initialization).
        var profile = await _repository.GetByMembershipIdAsync(request.MembershipId, cancellationToken);
        var isNew = false;

        if (profile is null)
        {
            var createResult = EmployeeReimbursementProfile.Create(request.TenantId, request.MembershipId);
            if (createResult.IsFailure)
                return Result.Failure<ReimbursementProfileResponse>(createResult.Error);
            profile = createResult.Value;
            isNew = true;
        }

        var updateResult = profile.UpdateContactInfo(
            request.PreferredPaymentMethod,
            request.ContactPhone,
            request.ReimbursementEmail,
            request.TaxId);
        if (updateResult.IsFailure)
            return Result.Failure<ReimbursementProfileResponse>(updateResult.Error);

        if (isNew)
            _repository.Add(profile);
        else
            _repository.Update(profile);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ReimbursementProfileMapper.ToResponse(profile));
    }
}
