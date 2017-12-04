
namespace Nop.Plugin.ExternalAuth.MailChimp
{
    /// <summary>
    /// Represents constants of the MailChimp authentication method
    /// </summary>
    public class MailChimpAuthenticationDefaults
    {
        /// <summary>
        /// System name of the MailChimp authentication method
        /// </summary>
        public static string SystemName => "ExternalAuth.MailChimp";

        /// <summary>
        /// The logical name of authentication scheme
        /// </summary>
        public static string AuthenticationScheme => "MailChimp";

        /// <summary>
        /// The issuer that should be used for any claims that are created
        /// </summary>
        public static string ClaimsIssuer => "MailChimp";

        /// <summary>
        /// The name of the access token
        /// </summary>
        public static string AccessTokenName => "access_token";

        /// <summary>
        /// The claim type of the avatar
        /// </summary>
        public static string AvatarClaimType => "urn:mailchimp:avatar_url";

        /// <summary>
        /// Callback path
        /// </summary>
        public static string CallbackPath => "/signin-mailchimp";

        /// <summary>
        /// The URI where the client will be redirected to authenticate
        /// </summary>
        public static string AuthorizationEndpoint => "https://login.mailchimp.com/oauth2/authorize";

        /// <summary>
        /// The URI the middleware will access to exchange the OAuth token
        /// </summary>
        public static string TokenEndpoint => "https://login.mailchimp.com/oauth2/token";

        /// <summary>
        /// The URI the middleware will access to obtain the user information
        /// </summary>
        public static string UserInformationEndpoint => "https://login.mailchimp.com/oauth2/metadata";

        /// <summary>
        /// Name of the view component
        /// </summary>
        public const string ViewComponentName = "MailChimpAuthentication";
    }
}