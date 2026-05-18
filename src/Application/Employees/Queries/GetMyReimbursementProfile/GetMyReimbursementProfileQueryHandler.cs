using FinFlow.Application.Employees.DTOs;
using FinFlow.Application.Employees.Mapping;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Employees;
using MediatR;

namespace FinFlow.Application.Employees.Queries.GetMyReimbursementProfile;

internal sealed class GetMyReimbursementProfileQueryHandler
    : IRequestHandler<GetMyReimbursementProfileQuery, Result<ReimbursementProfileResponse?>>
{
    private readonly IEmployeeReimbursementProfileRepository _repository;

    public GetMyReimbursementProfileQueryHandler(IEmployeeReimbursementProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ReimbursementProfileResponse?>> Handle(
        GetMyReimbursementProfileQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByMembershipIdAsync(request.MembershipId, cancellationToken);
        return Result.Success<ReimbursementProfileResponse?>(
            profile is null ? null : ReimbursementProfileMapper.ToResponse(profile));
    }
}
