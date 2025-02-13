using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Serilog;

namespace WebApi;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

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
                ValidAudiences = configuration.GetSection("ValidAudiences").Get<string[]>(),
                ValidIssuers = configuration.GetSection("ValidIssuers").Get<string[]>()
            };
        })
        .AddJwtBearer(Consts.MY_MICROSOFT_ENTRA_ID_SCHEME, jwtOptions =>
        {
            jwtOptions.MetadataAddress = configuration["AzureAd:MetadataAddress"]!;
            jwtOptions.Authority = configuration["AzureAd:Authority"];
            jwtOptions.Audience = configuration["AzureAd:Audience"];
            jwtOptions.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidAudiences = configuration.GetSection("ValidAudiences").Get<string[]>(),
                ValidIssuers = configuration.GetSection("ValidIssuers").Get<string[]>()
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
                ValidAudiences = configuration.GetSection("ValidAudiences").Get<string[]>(),
                ValidIssuers = configuration.GetSection("ValidIssuers").Get<string[]>()
            };
        })
        .AddPolicyScheme("UNKNOWN", "UNKNOWN", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                string authorization = context.Request.Headers[HeaderNames.Authorization]!;
                if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                {
                    var token = authorization.Substring("Bearer ".Length).Trim();
                    var jwtHandler = new JsonWebTokenHandler();

                    // it's a self contained access token and not encrypted
                    if (jwtHandler.CanReadToken(token))
                    {
                        var issuer = jwtHandler.ReadJsonWebToken(token).Issuer;
                        if (issuer == Consts.MY_OPENIDDICT_ISS) // OpenIddict
                        {
                            return Consts.MY_OPENIDDICT_SCHEME;
                        }

                        if (issuer == Consts.MY_AUTH0_ISS) // Auth0
                        {
                            return Consts.MY_AUTH0_SCHEME;
                        }

                        if (issuer == Consts.MY_MICROSOFT_ENTRA_ID_ISS) // Microsoft Entra ID
                        {
                            return Consts.MY_MICROSOFT_ENTRA_ID_SCHEME;
                        }
                    }
                }

                // We don't know what it is
                return Consts.MY_MICROSOFT_ENTRA_ID_SCHEME;
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

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        IdentityModelEventSource.ShowPII = true;
        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
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

        app.MapControllers().RequireAuthorization();

        return app;
    }
}