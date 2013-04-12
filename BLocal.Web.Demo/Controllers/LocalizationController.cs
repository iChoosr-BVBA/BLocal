namespace BLocal.Web.Demo.Controllers
{
    public class LocalizationController : MvcLocalizationController
    {
        public LocalizationController(ILocalizationContext context) : base(context)
        {
        }
    }
}
