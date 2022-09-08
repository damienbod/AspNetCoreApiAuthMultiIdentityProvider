using OpenIddict.Validation.AspNetCore;

namespace WebApi
{
    public static class Consts
    {
        public const string MY_AAD_SCHEME = "myAadScheme";
        public const string MY_AUTH0_SCHEME = "myAuth0Scheme";
        // OpenIddict scheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;

        public const string ALL_MY_SCHEMES = MY_AAD_SCHEME + "," + MY_AUTH0_SCHEME + "," + OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;

        public const string MY_AAD_POLICY = "myAadPolicy";
        public const string MY_AUTH0_POLICY = "myAuth0Policy";
        public const string MY_OPENIDDICT_POLICY = "myOpenIddictPolicy";

        public const string MY_POLICY_ALL_IDP = "myPolicyForAllIdp";

        public const string MY_AAD_ISS = "https://login.microsoftonline.com/7ff95b15-dc21-4ba6-bc92-824856578fc1/v2.0";
        public const string MY_AUTH0_ISS = "https://dev-damienbod.eu.auth0.com/";
        public const string MY_OPENIDDICT_ISS = "https://localhost:44318/";
    }
}
