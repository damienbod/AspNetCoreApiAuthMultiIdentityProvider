namespace WebApi
{
    public static class Consts
    {
        public const string AAD_MULTI_SCHEME = "multiTenantApiScheme";
        public const string AAD_SINGLE_SCHEME = "singleTenantApiScheme";

        public const string ALL_MY_SCHEMES = AAD_MULTI_SCHEME + "," + AAD_MULTI_SCHEME;

        public const string MUTLI_AAD_POLICY = "myAadPolicy";
        public const string SINGLE_AAD_POLICY = "myAuth0Policy";

        public const string MULTI_AAD_ISS = "https://login.microsoftonline.com/7ff95b15-dc21-4ba6-bc92-824856578fc1/v2.0";
        public const string SINGLE_AAD_ISS = "https://login.microsoftonline.com/7ff95b15-dc21-4ba6-bc92-824856578fc1/v2.0";

    }
}
