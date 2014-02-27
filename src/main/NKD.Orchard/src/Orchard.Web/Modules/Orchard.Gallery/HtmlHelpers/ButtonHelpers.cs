using System.Web.Mvc;

namespace Orchard.Gallery.HtmlHelpers
{
    public static class ButtonHelpers
    {
        public static MvcHtmlString Submit(this HtmlHelper helper, string name)
        {
            return MvcHtmlString.Create(string.Format(@"<input type=""submit"" value=""{0}"" />", helper.Encode(name)));
        }
    }
}