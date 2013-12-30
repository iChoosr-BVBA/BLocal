using System;
using System.Linq;
using System.Web.Mvc;
using BLocal.Core;

namespace BLocal.Web
{
    /// <summary>
    /// Recommended useage: create a controller that inherits this and call it LocalizationController, the rest will practically solve itself.
    /// </summary>
    public abstract class MvcLocalizationController : Controller
    {
        private readonly ILocalizationContext _context;

        protected MvcLocalizationController(ILocalizationContext context)
        {
            _context = context;
        }

        public ActionResult ReloadLocalization(String previousUrl)
        {
            if (!_context.DebugMode)
                throw new Exception("Unauthorized!");

            _context.Repository.Values.Reload();
            return previousUrl == null
                ? RedirectToAction("Index", "Home")
                : (ActionResult)Redirect(previousUrl);
        }

        public JsonResult GetQualifiedValues(String part, String locale, String[] keys)
        {
            if (!_context.DebugMode)
                throw new Exception("Unauthorized: DebugMode not on.");
            var values = keys == null
                ? Enumerable.Empty<FlattenedQualifiedValue>()
                : keys.Select(key => new FlattenedQualifiedValue(_context.Repository.GetQualified(new Qualifier.Unique(Part.Parse(part), new Locale(locale), key))));
            return Json(new { Success = true, Values = values }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public JsonResult ChangeValue(String part, String locale, String key, String value)
        {
            if(!_context.DebugMode)
                throw new Exception("Unauthorized!");

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            _context.Repository.Values.SetValue(qualifier, value);
            return Json(new { Success = true, Value = _context.Repository.Values.GetValue(qualifier) }, JsonRequestBehavior.AllowGet);
        }

        public class FlattenedQualifiedValue
        {
            public String Part { get; set; }
            public String Locale { get; set; }
            public String Key { get; set; }
            public String Value { get; set; }

            public FlattenedQualifiedValue(QualifiedValue value)
            {
                Part = value.Qualifier.Part.ToString();
                Locale = value.Qualifier.Locale.ToString();
                Key = value.Qualifier.Key;
                Value = value.Value.DecodedContent;
            }
        }
    }
}
