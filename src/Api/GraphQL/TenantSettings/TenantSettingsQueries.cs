using FinFlow.Api.GraphQL.Auth;
using FinFlow.Domain.Entities;
using FinFlow.Domain.Enums;
using FinFlow.Domain.TenantSettings;
using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using DomainError = FinFlow.Domain.Abstractions.Error;

namespace FinFlow.Api.GraphQL.TenantSettings;

[ExtendObjectType(typeof(AuthQueries))]
public sealed class TenantSettingsQueries
{
    [Authorize]
    public async Task<TenantSettingsPayload> GetTenantSettingsAsync(
        [Service] ITenantSettingsRepository repository,
        IResolverContext context,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId(context);
        var role = GetRole(context);

        // Staff can read branding (for UI theming). Full settings only for Manager+.
        if (role == RoleType.Staff)
            throw new GraphQLException(new HotChocolate.Error("Only Manager and above can view tenant settings.", "TenantSettings.Forbidden"));

        var settings = await repository.GetByTenantIdAsync(tenantId, cancellationToken)
            ?? throw new GraphQLException(new HotChocolate.Error(TenantSettingsErrors.NotFound.Description, TenantSettingsErrors.NotFound.Code));

        return TenantSettingsMutations.ToPayload(settings);
    }

    [Authorize]
    public async Task<BrandingPayload> GetTenantBrandingAsync(
        [Service] ITenantSettingsRepository repository,
        IResolverContext context,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId(context);

        var settings = await repository.GetByTenantIdAsync(tenantId, cancellationToken)
            ?? throw new GraphQLException(new HotChocolate.Error(TenantSettingsErrors.NotFound.Description, TenantSettingsErrors.NotFound.Code));

        return new BrandingPayload(
            settings.LogoUrl, settings.FaviconUrl, settings.PrimaryColor,
            settings.CompanyDisplayName, settings.Locale, settings.Timezone);
    }

    private static Guid GetTenantId(IResolverContext context)
    {
        var user = context.Service<IHttpContextAccessor>().HttpContext?.User;
        var raw = user?.FindFirst("IdTenant")?.Value;
        if (Guid.TryParse(raw, out var id)) return id;
        throw new GraphQLException(new HotChocolate.Error("Unauthorized", "Account.Unauthorized"));
    }

    private static RoleType GetRole(IResolverContext context)
    {
        var user = context.Service<IHttpContextAccessor>().HttpContext?.User;
        var rawRole = user?.FindFirst(ClaimTypes.Role)?.Value ?? user?.FindFirst("role")?.Value;
        return Enum.TryParse<RoleType>(rawRole, out var role) ? role : RoleType.Staff;
    }
}
