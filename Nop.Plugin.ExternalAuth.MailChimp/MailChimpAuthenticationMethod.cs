using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Plugin.ExternalAuth.MailChimp
{
    /// <summary>
    /// Represents method for the authentication with MailChimp account
    /// </summary>
    public class MailChimpAuthenticationMethod : BasePlugin, IExternalAuthenticationMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public MailChimpAuthenticationMethod(ILocalizationService localizationService, 
            ISettingService settingService,
            IWebHelper webHelper)
        {
            this._localizationService = localizationService;
            this._settingService = settingService;
            this._webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/MailChimpAuthentication/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return MailChimpAuthenticationDefaults.ViewComponentName;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new MailChimpAuthenticationSettings());

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.MailChimp.ClientId", "Client ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.MailChimp.ClientId.Hint", "Enter the OAuth2 client ID here.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.MailChimp.ClientSecret", "Client secret");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.MailChimp.ClientSecret.Hint", "Enter the OAuth2 client secret here.");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<MailChimpAuthenticationSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.ExternalAuth.MailChimp.ClientId");
            _localizationService.DeletePluginLocaleResource("Plugins.ExternalAuth.MailChimp.ClientId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.ExternalAuth.MailChimp.ClientSecret");
            _localizationService.DeletePluginLocaleResource("Plugins.ExternalAuth.MailChimp.ClientSecret.Hint");

            base.Uninstall();
        }

        #endregion
    }
}