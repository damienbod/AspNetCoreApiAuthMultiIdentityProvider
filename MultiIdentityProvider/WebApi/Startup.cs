using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

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
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "UNKNOWN";
            options.DefaultChallengeScheme = "UNKNOWN";

        })
        .AddJwtBearer(Consts.MY_AUTH0_SCHEME, options =>
        {
            options.Authority = Consts.MY_AUTH0_ISS;
            options.Audience = "https://auth0-api1";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidAudiences = Configuration.GetSection("ValidAudiences").Get<string[]>(),
                ValidIssuers = Configuration.GetSection("ValidIssuers").Get<string[]>()
            };
        })
        .AddJwtBearer(Consts.MY_AAD_SCHEME, jwtOptions =>
        {
            jwtOptions.MetadataAddress = Configuration["AzureAd:MetadataAddress"]; 
            jwtOptions.Authority = Configuration["AzureAd:Authority"];
            jwtOptions.Audience = Configuration["AzureAd:Audience"]; 
            jwtOptions.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidAudiences = Configuration.GetSection("ValidAudiences").Get<string[]>(),
                ValidIssuers = Configuration.GetSection("ValidIssuers").Get<string[]>()
            };
        })
        .AddJwtBearer(Consts.MY_OPENIDDICT_SCHEME, options =>
        {
            options.Authority = Consts.MY_OPENIDDICT_ISS;
            options.Audience = "rs_dataEventRecordsApi";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidAudiences = Configuration.GetSection("ValidAudiences").Get<string[]>(),
                ValidIssuers = Configuration.GetSection("ValidIssuers").Get<string[]>()
            };
        })
        .AddPolicyScheme("UNKNOWN", "UNKNOWN", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                string authorization = context.Request.Headers[HeaderNames.Authorization];
                if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                {
                    var token = authorization.Substring("Bearer ".Length).Trim();
                    var jwtHandler = new JwtSecurityTokenHandler();

                    // it's a self contained access token and not encrypted
                    if (jwtHandler.CanReadToken(token)) 
                    {
                        var issuer = jwtHandler.ReadJwtToken(token).Issuer;
                        if(issuer == Consts.MY_OPENIDDICT_ISS) // OpenIddict
                        {
                            return Consts.MY_OPENIDDICT_SCHEME;
                        }

                        if (issuer == Consts.MY_AUTH0_ISS) // Auth0
                        {
                            return Consts.MY_AUTH0_SCHEME;
                        }

                        if (issuer == Consts.MY_AAD_ISS) // AAD
                        {
                            return Consts.MY_AAD_SCHEME;
                        }
                    }
                }

                // We don't know what it is
                return Consts.MY_AAD_SCHEME;
            };
        });

        services.AddSingleton<IAuthorizationHandler, AllSchemesHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Consts.MY_POLICY_ALL_IDP, policyAllRequirement =>
            {
                policyAllRequirement.Requirements.Add(new AllSchemesRequirement());
            });
        });

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        IdentityModelEventSource.ShowPII = true;
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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
