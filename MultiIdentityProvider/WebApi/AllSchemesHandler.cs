using Microsoft.AspNetCore.Authorization;

namespace WebApi;

public class AllSchemesHandler : AuthorizationHandler<AllSchemesRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        AllSchemesRequirement requirement)
    {
        var issuer = string.Empty;

        var issClaim = context.User.Claims.FirstOrDefault(c => c.Type == "iss");
        if (issClaim != null)
            issuer = issClaim.Value;

        if (issuer == Consts.MY_OPENIDDICT_ISS) // OpenIddict
        {
            var scopeClaim = context.User.Claims.FirstOrDefault(c => c.Type == "scope"
                && c.Value == "dataEventRecords");
            if (scopeClaim != null)
            {
                // scope": "dataEventRecords",
                context.Succeed(requirement);
            }
        }

        if (issuer == Consts.MY_AUTH0_ISS) // Auth0
        {
            // add require claim "gty", "client-credentials"
            var azpClaim = context.User.Claims.FirstOrDefault(c => c.Type == "azp"
                && c.Value == "naWWz6gdxtbQ68Hd2oAehABmmGM9m1zJ");
            if (azpClaim != null)
            {
                context.Succeed(requirement);
            }
        }

        if (issuer == Consts.MY_MICROSOFT_ENTRA_ID_ISS) // AAD
        {
            // "azp": "--your-azp-claim-value--",
            var azpClaim = context.User.Claims.FirstOrDefault(c => c.Type == "azp"
                && c.Value == "46d2f651-813a-4b5c-8a43-63abcb4f692c");
            if (azpClaim != null)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
