using FinFlow.Application.Common.Abstractions;
using FinFlow.Application.Employees.DTOs;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Accounts;
using FinFlow.Domain.EmailChallenges;
using FinFlow.Domain.Entities;
using FinFlow.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinFlow.Application.Employees.Commands.RequestBankInfoUpdateOtp;

internal sealed class RequestBankInfoUpdateOtpCommandHandler
    : IRequestHandler<RequestBankInfoUpdateOtpCommand, Result<OtpDispatchResponse>>
{
    private const int OtpLifetimeMinutes = 5;
    private const int CooldownSeconds = 60;

    private readonly IAccountRepository _accountRepository;
    private readonly IEmailChallengeRepository _emailChallengeRepository;
    private readonly IEmailChallengeSecretService _secretService;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<RequestBankInfoUpdateOtpCommandHandler> _logger;

    public RequestBankInfoUpdateOtpCommandHandler(
        IAccountRepository accountRepository,
        IEmailChallengeRepository emailChallengeRepository,
        IEmailChallengeSecretService secretService,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<RequestBankInfoUpdateOtpCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _emailChallengeRepository = emailChallengeRepository;
        _secretService = secretService;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<OtpDispatchResponse>> Handle(
        RequestBankInfoUpdateOtpCommand request,
        CancellationToken cancellationToken)
    {
        if (request.AccountId == Guid.Empty || request.MembershipId == Guid.Empty)
            return Result.Failure<OtpDispatchResponse>(EmailChallengeErrors.AccountRequired);

        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null || !account.IsActive)
            return Result.Failure<OtpDispatchResponse>(EmailChallengeErrors.AccountRequired);

        var nowUtc = _clock.UtcNow;

        // Revoke previous outstanding bank-update challenge to enforce single-use semantics.
        var latest = await _emailChallengeRepository.GetLatestByAccountIdAndPurposeForUpdateAsync(
            request.AccountId,
            EmailChallengePurpose.UpdateBankInfo,
            cancellationToken);

        if (latest is not null && latest.IsUsableAt(nowUtc))
        {
            var revokeResult = latest.Revoke(nowUtc);
            if (revokeResult.IsFailure)
                return Result.Failure<OtpDispatchResponse>(revokeResult.Error);
            _emailChallengeRepository.Update(latest);
        }

        var rawToken = _secretService.GenerateVerificationToken();
        var rawOtp = _secretService.GenerateVerificationOtp();
        var tokenHash = _secretService.HashChallengeToken(rawToken);
        var otpHash = _secretService.HashChallengeOtp(rawOtp);

        var challengeResult = EmailChallenge.Create(
            account.Id,
            EmailChallengePurpose.UpdateBankInfo,
            nowUtc,
            nowUtc.AddMinutes(OtpLifetimeMinutes),
            email: account.Email,
            tokenHash: tokenHash,
            otpHash: otpHash,
            lastSentAtUtc: nowUtc);

        if (challengeResult.IsFailure)
            return Result.Failure<OtpDispatchResponse>(challengeResult.Error);

        try
        {
            await _emailSender.SendBankInfoUpdateOtpAsync(account.Email, rawOtp, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send bank info update OTP to account {AccountId}", account.Id);
            return Result.Failure<OtpDispatchResponse>(EmailChallengeErrors.EmailDeliveryFailed);
        }

        _emailChallengeRepository.Add(challengeResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new OtpDispatchResponse(challengeResult.Value.Id, CooldownSeconds));
    }
}
