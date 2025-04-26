using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;
using RazorMicrosoftEntraIDClient;
using Serilog;

namespace RazorMicrosoftEntraID;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddTransient<WebApiValuesService>();
        services.AddHttpClient();

        services.AddOptions();

        string[]? initialScopes = configuration.GetValue<string>("MyApiOne:ScopeForAccessToken")?.Split(' ');

        services.AddMicrosoftIdentityWebAppAuthentication(configuration)
            .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddInMemoryTokenCaches();

        services.AddRazorPages().AddMvcOptions(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        }).AddMicrosoftIdentityUI();

        services.AddSecurityHeaderPolicies()
           .SetPolicySelector((PolicySelectorContext ctx) =>
           {
               return SecurityHeadersDefinitions
                 .GetHeaderPolicyCollection(builder.Environment.IsDevelopment());
           });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        IdentityModelEventSource.ShowPII = true;
        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        app.UseSerilogRequestLogging();

        app.UseSecurityHeaders();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();

        return app;
    }
}