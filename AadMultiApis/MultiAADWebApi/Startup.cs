using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;

namespace WebApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(Consts.AAD_MULTI_SCHEME)
            .AddMicrosoftIdentityWebApi(Configuration, "AzureADMultiApi", Consts.AAD_MULTI_SCHEME);

        services.AddAuthentication(Consts.AAD_SINGLE_SCHEME)
            .AddMicrosoftIdentityWebApi(Configuration, "AzureADSingleApi", Consts.AAD_SINGLE_SCHEME);

        services.AddAuthorization(policies =>
        {
            policies.AddPolicy(Consts.MUTLI_AAD_POLICY, p =>
            {
                // application access token
                // "azp": "967925d5-87ea-46e6-b0eb-1223c001fd77",
                p.RequireClaim("azp", "967925d5-87ea-46e6-b0eb-1223c001fd77");

                // client secret = 1, 2 if certificate is used
                p.RequireClaim("azpacr", "1"); 
            });

            policies.AddPolicy(Consts.SINGLE_AAD_POLICY, p =>
            {
                // delegated access token
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
                .AddAuthenticationSchemes(Consts.AAD_MULTI_SCHEME, Consts.AAD_SINGLE_SCHEME)
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        IdentityModelEventSource.ShowPII = true;
        //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        if (env.IsDevelopment())
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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireAuthorization();
        });
    }
}
