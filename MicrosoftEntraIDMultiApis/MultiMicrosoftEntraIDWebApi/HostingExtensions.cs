using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Serilog;

namespace WebApi;

internal static class HostingExtensions
{
    private static IWebHostEnvironment? _env;
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        _env = builder.Environment;

        services.AddAuthentication(Consts.MICROSOFT_ENTRA_ID_MULTI_SCHEME)
    .AddMicrosoftIdentityWebApi(configuration, "AzureADMultiApi", Consts.MICROSOFT_ENTRA_ID_MULTI_SCHEME);

        services.AddAuthentication(Consts.MICROSOFT_ENTRA_ID_SINGLE_SCHEME)
            .AddMicrosoftIdentityWebApi(configuration, "AzureADSingleApi", Consts.MICROSOFT_ENTRA_ID_SINGLE_SCHEME);

        services.AddAuthorization(policies =>
        {
            policies.AddPolicy(Consts.MUTLI_MICROSOFT_ENTRA_ID_POLICY, p =>
            {
                // application access token
                // "roles": [
                //  "application-api-role"
                // ],
                // "azp": "967925d5-87ea-46e6-b0eb-1223c001fd77",
                p.RequireClaim("azp", "967925d5-87ea-46e6-b0eb-1223c001fd77");

                // client secret = 1, 2 if certificate is used
                p.RequireClaim("azpacr", "1");
            });

            policies.AddPolicy(Consts.SINGLE_MICROSOFT_ENTRA_ID_POLICY, p =>
            {
                // delegated access token => "scp": "access_as_user",
                // "azp": "46d2f651-813a-4b5c-8a43-63abcb4f692c",
                p.RequireClaim("azp", "46d2f651-813a-4b5c-8a43-63abcb4f692c");

                // client secret = 1, 2 if certificate is used
                p.RequireClaim("azpacr", "1");
            });
        });

        services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(Consts.MICROSOFT_ENTRA_ID_MULTI_SCHEME, Consts.MICROSOFT_ENTRA_ID_SINGLE_SCHEME)
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        IdentityModelEventSource.ShowPII = true;
        //JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        app.UseSerilogRequestLogging();
        
        if (_env!.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}