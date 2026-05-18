using FinFlow.Application.Common.Abstractions;
using FinFlow.Application.Common.Security;
using FinFlow.Application.Employees.DTOs;
using FinFlow.Application.Employees.Mapping;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.EmailChallenges;
using FinFlow.Domain.Employees;
using FinFlow.Domain.Entities;
using FinFlow.Domain.Enums;
using MediatR;

namespace FinFlow.Application.Employees.Commands.ConfirmBankInfoUpdate;

internal sealed class ConfirmBankInfoUpdateCommandHandler
    : IRequestHandler<ConfirmBankInfoUpdateCommand, Result<ReimbursementProfileResponse>>
{
    private const int MaxBankEditsPerWindow = 3;
    private static readonly TimeSpan BankEditWindow = TimeSpan.FromDays(30);

    private readonly IEmployeeReimbursementProfileRepository _profileRepository;
    private readonly IEmailChallengeRepository _emailChallengeRepository;
    private readonly IEmailChallengeSecretService _secretService;
    private readonly IPiiEncryptionService _piiEncryption;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public ConfirmBankInfoUpdateCommandHandler(
        IEmployeeReimbursementProfileRepository profileRepository,
        IEmailChallengeRepository emailChallengeRepository,
        IEmailChallengeSecretService secretService,
        IPiiEncryptionService piiEncryption,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _profileRepository = profileRepository;
        _emailChallengeRepository = emailChallengeRepository;
        _secretService = secretService;
        _piiEncryption = piiEncryption;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ReimbursementProfileResponse>> Handle(
        ConfirmBankInfoUpdateCommand request,
        CancellationToken cancellationToken)
    {
        // Rate-limit: 3 bank changes per 30 days. Counter increments only after a
        // valid OTP — failed OTP attempts do NOT count.
        var rateLimitKey = $"profile-bank-edit:{request.MembershipId:N}";
        var currentCount = await _cacheService.IncrementWithExpiryAsync(
            rateLimitKey, BankEditWindow, cancellationToken);
        if (currentCount > MaxBankEditsPerWindow)
            return Result.Failure<ReimbursementProfileResponse>(ProfileErrors.BankEditRateLimited);

        // Load + verify OTP challenge.
        var challenge = await _emailChallengeRepository.GetByIdAsync(request.ChallengeId, cancellationToken);
        if (challenge is null
            || challenge.AccountId != request.AccountId
            || challenge.Purpose != EmailChallengePurpose.UpdateBankInfo)
        {
            return Result.Failure<ReimbursementProfileResponse>(EmailChallengeErrors.ChallengeNotFound);
        }

        var nowUtc = _clock.UtcNow;
        if (!challenge.IsUsableAt(nowUtc))
            return Result.Failure<ReimbursementProfileResponse>(EmailChallengeErrors.Expired);

        var providedHash = _secretService.HashChallengeOtp(request.Otp);
        if (!string.Equals(providedHash, challenge.OtpHash, StringComparison.Ordinal))
        {
            var failResult = challenge.RegisterFailedOtpAttempt(nowUtc);
            if (failResult.IsSuccess)
                _emailChallengeRepository.Update(challenge);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<ReimbursementProfileResponse>(EmailChallengeErrors.OtpInvalid);
        }

        var consumeResult = challenge.Consume(nowUtc);
        if (consumeResult.IsFailure)
            return Result.Failure<ReimbursementProfileResponse>(consumeResult.Error);
        _emailChallengeRepository.Update(challenge);

        // Apply change to profile (auto-create if needed).
        var profile = await _profileRepository.GetByMembershipIdAsync(request.MembershipId, cancellationToken);
        var isNew = false;
        if (profile is null)
        {
            var createResult = EmployeeReimbursementProfile.Create(request.TenantId, request.MembershipId);
            if (createResult.IsFailure)
                return Result.Failure<ReimbursementProfileResponse>(createResult.Error);
            profile = createResult.Value;
            isNew = true;
        }

        // Resolve clear vs update path. Clear path = all 3 bank fields blank.
        var isClearRequest =
            string.IsNullOrWhiteSpace(request.BankCode)
            && string.IsNullOrWhiteSpace(request.BankAccountNumber)
            && string.IsNullOrWhiteSpace(request.BankAccountHolderName);

        Result updateResult;
        if (isClearRequest)
        {
            updateResult = profile.UpdateBankInfo(null, null, null, null, null);
        }
        else
        {
            // Validate account number is digits, 6-20 chars after stripping whitespace/hyphens.
            var rawAccount = (request.BankAccountNumber ?? string.Empty)
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty);

            if (rawAccount.Length is < 6 or > 20 || !rawAccount.All(char.IsDigit))
                return Result.Failure<ReimbursementProfileResponse>(ProfileErrors.AccountNumberInvalidLength);

            var encrypted = _piiEncryption.Encrypt(rawAccount);
            var last4 = rawAccount[^4..];

            updateResult = profile.UpdateBankInfo(
                request.BankCode,
                encrypted,
                last4,
                request.BankAccountHolderName,
                request.BankBranch);
        }

        if (updateResult.IsFailure)
            return Result.Failure<ReimbursementProfileResponse>(updateResult.Error);

        if (isNew)
            _profileRepository.Add(profile);
        else
            _profileRepository.Update(profile);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ReimbursementProfileMapper.ToResponse(profile));
    }
}
