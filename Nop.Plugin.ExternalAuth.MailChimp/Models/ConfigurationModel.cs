using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.ExternalAuth.MailChimp.Models
{
    /// <summary>
    /// Represents a configuration model
    /// </summary>
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.ExternalAuth.MailChimp.ClientId")]
        public string ClientId { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.MailChimp.ClientSecret")]
        [DataType(DataType.Password)]
        [NoTrim]
        public string ClientSecret { get; set; }
    }
}