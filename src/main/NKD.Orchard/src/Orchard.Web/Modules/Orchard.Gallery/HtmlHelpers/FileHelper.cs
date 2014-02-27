using System.Web.Mvc;

namespace Orchard.Gallery.HtmlHelpers
{
    public static class FileHelpers
    {
        public static MvcHtmlString File(this HtmlHelper helper, string name)
        {
            return MvcHtmlString.Create(string.Format(@"<input type=""file"" id=""{0}"" name=""{0}"" />",
                helper.AttributeEncode(name)));
        }
    }
}