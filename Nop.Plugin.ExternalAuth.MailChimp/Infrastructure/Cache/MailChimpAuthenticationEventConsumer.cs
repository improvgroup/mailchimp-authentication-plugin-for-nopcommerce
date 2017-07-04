using System.Linq;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.StaticFiles;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Authentication.External;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;

namespace Nop.Plugin.ExternalAuth.MailChimp.Infrastructure.Cache
{
    /// <summary>
    /// MailChimp authentication event consumer (used for saving customer fields on registration)
    /// </summary>
    public partial class MailChimpAuthenticationEventConsumer : IConsumer<CustomerAutoRegisteredByExternalMethodEvent>
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public MailChimpAuthenticationEventConsumer(CustomerSettings customerSettings,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IPictureService pictureService)
        {
            this._customerSettings = customerSettings;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._logger = logger;
            this._pictureService = pictureService;
        }

        #endregion

        #region Methods

        public void HandleEvent(CustomerAutoRegisteredByExternalMethodEvent eventMessage)
        {
            if (eventMessage?.Customer == null || eventMessage?.AuthenticationParameters == null)
                return;

            //handle event only for this authentication method
            if (!eventMessage.AuthenticationParameters.ProviderSystemName.Equals(MailChimpAuthenticationDefaults.ProviderSystemName))
                return;

            //store some of the customer fields
            var firstName = eventMessage.AuthenticationParameters.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
            if (!string.IsNullOrEmpty(firstName))
                _genericAttributeService.SaveAttribute(eventMessage.Customer, SystemCustomerAttributeNames.FirstName, firstName);

            //upload avatar
            var avatarUrl = eventMessage.AuthenticationParameters.Claims?.FirstOrDefault(claim => claim.Type == MailChimpAuthenticationDefaults.AvatarClaimType)?.Value;
            if (string.IsNullOrEmpty(avatarUrl))
                return;

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return;

            try
            {
                //try to get byte array of the user avatar
                byte[] customerPictureBinary;
                using (var webClient = new WebClient())
                {
                    customerPictureBinary = webClient.DownloadData(avatarUrl);
                }
                if (customerPictureBinary.Length > _customerSettings.AvatarMaximumSizeBytes)
                {
                    _logger.Error(string.Format(_localizationService.GetResource("Account.Avatar.MaximumUploadedFileSize"), _customerSettings.AvatarMaximumSizeBytes));
                    return;
                }

                //save avatar
                new FileExtensionContentTypeProvider().TryGetContentType(avatarUrl, out string mimeType);
                var customerAvatar = _pictureService.InsertPicture(customerPictureBinary, mimeType ?? MimeTypes.ImagePng, null);
                _genericAttributeService.SaveAttribute(eventMessage.Customer, SystemCustomerAttributeNames.AvatarPictureId, customerAvatar.Id);
            }
            catch { }
        }

        #endregion
    }
}