using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.Environment.Extensions;
using Orchard;

namespace NKD.Fields
{
    //[OrchardFeature("UserGuidField")]
    public class UserGuidField : ContentField
    {
        public IOrchardServices Services { get; set; }
        public UserGuidField(IOrchardServices services)
        {
            Services = services;
        }

        public Guid? Guid
        {
            get
            {
                var value = Storage.Get<string>();
                Guid o;

                if (System.Guid.TryParse(value, out o))
                {

                    return o;
                }

                return null;
            }

            set
            {
                Storage.Set(value == null ?
                    String.Empty
                    :
                    value.Value.ToString());
            }
        }

    }
}