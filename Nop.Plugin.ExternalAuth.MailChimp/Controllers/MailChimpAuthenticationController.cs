using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.ExternalAuth.MailChimp.Models;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;

namespace Nop.Plugin.ExternalAuth.MailChimp.Controllers
{
    public class MailChimpAuthenticationController : BasePluginController
    {
        #region Fields

        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly MailChimpAuthenticationSettings _mailChimpAuthenticationSettings;

        #endregion

        #region Ctor

        public MailChimpAuthenticationController(IExternalAuthenticationService externalAuthenticationService,
            ILocalizationService localizationService,
            ISettingService settingService,            
            IStoreService storeService,
            IWorkContext workContext,
            MailChimpAuthenticationSettings mailChimpAuthenticationSettings)
        {
            this._externalAuthenticationService = externalAuthenticationService;
            this._localizationService = localizationService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._workContext = workContext;
            this._mailChimpAuthenticationSettings = mailChimpAuthenticationSettings;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area("Admin")]
        public IActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<MailChimpAuthenticationSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ClientId = settings.ClientId,
                ClientSecret = settings.ClientSecret,
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.ClientId_OverrideForStore = _settingService.SettingExists(settings, setting => setting.ClientId, storeScope);
                model.ClientSecret_OverrideForStore = _settingService.SettingExists(settings, setting => setting.ClientSecret, storeScope);
            }

            return View("~/Plugins/ExternalAuth.MailChimp/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area("Admin")]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<MailChimpAuthenticationSettings>(storeScope);

            //save settings
            settings.ClientId = model.ClientId;
            settings.ClientSecret = model.ClientSecret;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(settings, setting => setting.ClientId, model.ClientId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, setting => setting.ClientSecret, model.ClientSecret_OverrideForStore, storeScope, false);
           
            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public IActionResult Login(string returnUrl)
        {
            if (!_externalAuthenticationService.ExternalAuthenticationMethodIsAvailable(MailChimpAuthenticationDefaults.ProviderSystemName))
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
            var authenticateContext = new AuthenticateContext(MailChimpAuthenticationDefaults.AuthenticationScheme);
            await this.HttpContext?.Authentication?.AuthenticateAsync(authenticateContext);

            //get authenticated user identity
            var userIdentity = authenticateContext.Principal?.Identities?.FirstOrDefault(identity => identity.IsAuthenticated);
            if (userIdentity == null || !userIdentity.Claims.Any())
                return RedirectToRoute("Login");

            //create external authentication parameters
            var authenticationParameters = new ExternalAuthenticationParameters
            {
                ProviderSystemName = MailChimpAuthenticationDefaults.ProviderSystemName,
                AccessToken = new AuthenticationProperties(authenticateContext.Properties).GetTokenValue("access_token"),
                Email = userIdentity.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value,
                ExternalIdentifier = userIdentity.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
                ExternalDisplayIdentifier = userIdentity.FindFirst(claim => claim.Type == ClaimTypes.Name)?.Value,
                Claims = userIdentity.Claims.Select(claim => new ExternalAuthenticationClaim(claim.Type, claim.Value)).ToList()
            };

            //authenticate Nop user
            return _externalAuthenticationService.Authenticate(authenticationParameters, returnUrl);
        }

        #endregion
    }
}