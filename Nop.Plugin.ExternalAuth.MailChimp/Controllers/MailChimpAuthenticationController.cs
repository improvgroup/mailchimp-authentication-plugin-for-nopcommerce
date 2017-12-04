using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nop.Core;
using Nop.Plugin.ExternalAuth.MailChimp.Models;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.ExternalAuth.MailChimp.Controllers
{
    public class MailChimpAuthenticationController : BasePluginController
    {
        #region Fields

        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly IOptionsMonitorCache<OAuthOptions> _optionsCache;
        private readonly ISettingService _settingService;
        private readonly MailChimpAuthenticationSettings _mailChimpAuthenticationSettings;

        #endregion

        #region Ctor

        public MailChimpAuthenticationController(IExternalAuthenticationService externalAuthenticationService,
            ILocalizationService localizationService,
            IOptionsMonitorCache<OAuthOptions> optionsCache,
            ISettingService settingService,
            MailChimpAuthenticationSettings mailChimpAuthenticationSettings)
        {
            this._externalAuthenticationService = externalAuthenticationService;
            this._localizationService = localizationService;
            this._optionsCache = optionsCache;
            this._settingService = settingService;
            this._mailChimpAuthenticationSettings = mailChimpAuthenticationSettings;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            //prepare model
            var model = new ConfigurationModel
            {
                ClientId = _mailChimpAuthenticationSettings.ClientId,
                ClientSecret = _mailChimpAuthenticationSettings.ClientSecret
            };

            return View("~/Plugins/ExternalAuth.MailChimp/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _mailChimpAuthenticationSettings.ClientId = model.ClientId;
            _mailChimpAuthenticationSettings.ClientSecret = model.ClientSecret;
            _settingService.SaveSetting(_mailChimpAuthenticationSettings);

            //clear MailChimp authentication options cache (workaround to force changes authentication scheme parameters)
            _optionsCache.TryRemove(MailChimpAuthenticationDefaults.AuthenticationScheme);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public IActionResult Login(string returnUrl)
        {
            if (!_externalAuthenticationService.ExternalAuthenticationMethodIsAvailable(MailChimpAuthenticationDefaults.SystemName))
                throw new NopException("MailChimp authentication module cannot be loaded");

            if (string.IsNullOrEmpty(_mailChimpAuthenticationSettings.ClientId) || string.IsNullOrEmpty(_mailChimpAuthenticationSettings.ClientSecret))
                throw new NopException("MailChimp authentication module not configured");

            //configure login callback action
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("LoginCallback", "MailChimpAuthentication", new { returnUrl = returnUrl })
            };

            return Challenge(authenticationProperties, MailChimpAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> LoginCallback(string returnUrl)
        {
            //authenticate MailChimp user
            var authenticateResult = await this.HttpContext.AuthenticateAsync(MailChimpAuthenticationDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || !authenticateResult.Principal.Claims.Any())
                return RedirectToRoute("Login");

            //create external authentication parameters
            var authenticationParameters = new ExternalAuthenticationParameters
            {
                ProviderSystemName = MailChimpAuthenticationDefaults.SystemName,
                AccessToken = await this.HttpContext
                    .GetTokenAsync(MailChimpAuthenticationDefaults.AuthenticationScheme, MailChimpAuthenticationDefaults.AccessTokenName),
                Email = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value,
                ExternalIdentifier = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
                ExternalDisplayIdentifier = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Name)?.Value,
                Claims = authenticateResult.Principal.Claims.Select(claim => new ExternalAuthenticationClaim(claim.Type, claim.Value)).ToList()
            };

            //authenticate Nop user
            return _externalAuthenticationService.Authenticate(authenticationParameters, returnUrl);
        }

        #endregion
    }
}