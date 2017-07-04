using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;

namespace Nop.Plugin.ExternalAuth.MailChimp.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring MailChimp authentication middleware on application startup
    /// </summary>
    public class MailChimpAuthenticationStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
        {
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            var settings = EngineContext.Current.Resolve<MailChimpAuthenticationSettings>();
            if (string.IsNullOrEmpty(settings?.ClientId) || string.IsNullOrEmpty(settings?.ClientSecret))
                return;

            //add the OAuth2 middleware
            application.UseOAuthAuthentication(new OAuthOptions
            {
                AuthenticationScheme = MailChimpAuthenticationDefaults.AuthenticationScheme,
                CallbackPath = new PathString(MailChimpAuthenticationDefaults.CallbackPath),
                ClaimsIssuer = MailChimpAuthenticationDefaults.ClaimsIssuer,
                SaveTokens = true,

                //configure the OAuth2 Client ID and Client Secret
                ClientId = settings.ClientId,
                ClientSecret = settings.ClientSecret,                

                //configure the MailChimp endpoints                
                AuthorizationEndpoint = MailChimpAuthenticationDefaults.AuthorizationEndpoint,
                TokenEndpoint = MailChimpAuthenticationDefaults.TokenEndpoint,
                UserInformationEndpoint = MailChimpAuthenticationDefaults.UserInformationEndpoint,

                Events = new OAuthEvents
                {
                    // The OnCreatingTicket event is called after the user has been authenticated and the OAuth middleware has
                    // created an auth ticket. We need to manually call the UserInformationEndpoint to retrieve the user's information,
                    // parse the resulting JSON to extract the relevant information, and add the correct claims.
                    OnCreatingTicket = async context =>
                    {
                        //try to retrieve user info
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                        {
                            var logger = EngineContext.Current.Resolve<ILogger>();
                            logger.Error($"An error occurred while retrieving the MailChimp user profile: {response.Headers.ToString()} {responseBody}.");
                            return;
                        }

                        //extract the user info object
                        var user = JObject.Parse(responseBody);

                        //set external identifier of the user as a claim with type ClaimTypes.NameIdentifier
                        var externalIdentifier = user?.Value<JObject>("login")?.Value<string>("login_id");
                        if (!string.IsNullOrEmpty(externalIdentifier))
                            context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, externalIdentifier));

                        //set username of the user
                        var name = user?.Value<JObject>("login")?.Value<string>("login_name");
                        if (!string.IsNullOrEmpty(name))
                            context.Identity.AddClaim(new Claim(ClaimTypes.Name, name));

                        //set email
                        var email = user?.Value<JObject>("login")?.Value<string>("login_email");
                        if (!string.IsNullOrEmpty(email))
                            context.Identity.AddClaim(new Claim(ClaimTypes.Email, email));

                        //try to set avatar URL
                        var avatar = user?.Value<JObject>("login")?.Value<string>("avatar");
                        if (!string.IsNullOrEmpty(avatar))
                            context.Identity.AddClaim(new Claim(MailChimpAuthenticationDefaults.AvatarClaimType, avatar));
                    }
                }
            });
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order
        {
            get { return 501; }
        }
    }
}
