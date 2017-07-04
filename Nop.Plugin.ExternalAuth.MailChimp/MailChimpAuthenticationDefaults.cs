
namespace Nop.Plugin.ExternalAuth.MailChimp
{
    /// <summary>
    /// Default values used by the MailChimp authentication middleware
    /// </summary>
    public static class MailChimpAuthenticationDefaults
    {
        /// <summary>
        /// System name of the external authentication method
        /// </summary>
        public const string ProviderSystemName = "ExternalAuth.MailChimp";

        /// <summary>
        /// The logical name for an authentication scheme
        /// </summary>
        public const string AuthenticationScheme = "MailChimp";

        /// <summary>
        /// The issuer that should be used for any claims that are created
        /// </summary>
        public const string ClaimsIssuer = "MailChimp";

        /// <summary>
        /// The claim type of the avatar
        /// </summary>
        public const string AvatarClaimType = "urn:mailchimp:avatar_url";

        /// <summary>
        /// Callback path
        /// </summary>
        public const string CallbackPath = "/signin-mailchimp";

        /// <summary>
        /// The URI where the client will be redirected to authenticate
        /// </summary>
        public const string AuthorizationEndpoint = "https://login.mailchimp.com/oauth2/authorize";

        /// <summary>
        /// The URI the middleware will access to exchange the OAuth token
        /// </summary>
        public const string TokenEndpoint = "https://login.mailchimp.com/oauth2/token";

        /// <summary>
        /// The URI the middleware will access to obtain the user information
        /// </summary>
        public const string UserInformationEndpoint = "https://login.mailchimp.com/oauth2/metadata";
    }
}
