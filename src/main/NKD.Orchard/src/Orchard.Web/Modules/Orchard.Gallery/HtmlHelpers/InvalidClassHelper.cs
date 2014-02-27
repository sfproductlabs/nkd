using System.Linq;
using System.Web.Mvc;

namespace Orchard.Gallery.HtmlHelpers {
    public static class InvalidClassHelper {
        public static MvcHtmlString InvalidClass<T>(this HtmlHelper<T> helper, string name) {
            return MvcHtmlString.Create(helper.ViewData.ModelState[name] == null || !helper.ViewData.ModelState[name].Errors.Any() ? "" : "invalid");
        }
    }
}