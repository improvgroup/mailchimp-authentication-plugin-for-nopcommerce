using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.ExternalAuth.MailChimp.Components
{
    [ViewComponent(Name = "MailChimpAuthentication")]
    public class MailChimpAuthenticationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/ExternalAuth.MailChimp/Views/PublicInfo.cshtml");
        }
    }
}