using Microsoft.AspNetCore.Authentication.Cookies;
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
                p.RequireClaim("azp", "AScjLo16UadTQRIt2Zm1xLHVaEaE1feA");
            });

            policies.AddPolicy(Consts.SINGLE_AAD_POLICY, p =>
            {
                p.RequireClaim("azp", "AScjLo16UadTQRIt2Zm1xLHVaEaE1feA");
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
