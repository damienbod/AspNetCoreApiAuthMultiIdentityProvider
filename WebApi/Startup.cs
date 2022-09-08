using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using OpenIddict.Validation.AspNetCore;
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
            options.Authority = "https://dev-damienbod.eu.auth0.com/";
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
        .AddPolicyScheme("UNKNOWN", "UNKNOWN", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                string authorization = context.Request.Headers[HeaderNames.Authorization];
                if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                {
                    var token = authorization.Substring("Bearer ".Length).Trim();
                    var jwtHandler = new JwtSecurityTokenHandler();

                    if(jwtHandler.CanReadToken(token)) // it's a self contained access token and not encrypted
                    {
                        var issuer = jwtHandler.ReadJwtToken(token).Issuer; //.Equals("B2C-Authority"))
                        if(issuer == Consts.MY_OPENIDDICT_ISS) // OpenIddict
                        {
                            return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
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

                // We don't know with it is
                return Consts.MY_AAD_SCHEME;
            };
        });

        // Register the OpenIddict validation components.
        services.AddOpenIddict() // Scheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
            .AddValidation(options =>
            {
                // Note: the validation handler uses OpenID Connect discovery
                // to retrieve the address of the introspection endpoint.
                options.SetIssuer("https://localhost:44318/");
                options.AddAudiences("rs_dataEventRecordsApi");

                // Configure the validation handler to use introspection and register the client
                // credentials used when communicating with the remote introspection endpoint.
                //options.UseIntrospection()
                //        .SetClientId("rs_dataEventRecordsApi")
                //        .SetClientSecret("dataEventRecordsSecret");

                // disable access token encyption for this
                options.UseAspNetCore();

                // Register the System.Net.Http integration.
                options.UseSystemNetHttp();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
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
