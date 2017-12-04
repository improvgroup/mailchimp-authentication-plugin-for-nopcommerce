using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.ExternalAuth.MailChimp.Components
{
    /// <summary>
    /// Represents a view component to render 'Sign In with MailChimp' button on the login page
    /// </summary>
    [ViewComponent(Name = MailChimpAuthenticationDefaults.ViewComponentName)]
    public class MailChimpAuthenticationViewComponent : NopViewComponent
    {
        /// <summary>
        /// Invoke the external authentication view component
        /// </summary>
        /// <returns>View component result</returns>
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/ExternalAuth.MailChimp/Views/PublicInfo.cshtml");
        }
    }
}