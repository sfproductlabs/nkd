using Orchard.Environment.Extensions;
using Orchard.Themes;
using System.Web.Mvc;

namespace Proligence.SignalR.Controllers {
    [OrchardFeature("Proligence.SignalR.Core.Samples")]
    [Themed]
    public class SamplesController : Controller
    {
        public ActionResult DrawingPad()
        {
            return View();
        }

        public ActionResult Benchmark()
        {
            return View();
        }

        public ActionResult Chat()
        {
            return View();
        }

        public ActionResult Raw()
        {
            return View();
        }
    }
}
