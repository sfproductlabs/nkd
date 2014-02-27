using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace NKD.Helpers
{
    public class JsonHelper
    {
        public class JsonNetResult : JsonResult
        {
            public JsonNetResult(object data, JsonRequestBehavior jsonRequestBehavior)
                : base()
            {
                this.Data = data;
                this.JsonRequestBehavior = jsonRequestBehavior;
            }
            public override void ExecuteResult(ControllerContext context)
            {
                if (context == null)
                    throw new ArgumentNullException("context");

                var response = context.HttpContext.Response;

                response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

                if (ContentEncoding != null)
                    response.ContentEncoding = ContentEncoding;

                if (Data == null)
                    return;

                // If you need special handling, you can call another form of SerializeObject below
                var serializedObject = JsonConvert.SerializeObject(Data, Formatting.Indented);
                response.Write(serializedObject);
            }
        }
    }
}