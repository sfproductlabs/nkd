using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using NKD.Models;
using Orchard;

namespace NKD.Handlers
{
    public class NKDInfoWidgetHandler : ContentHandler
    {
		protected override void BuildDisplayShape(BuildDisplayContext context)
		{
			base.BuildDisplayShape(context);
			
			if (context.ContentItem.ContentType == "NKDInfoWidget")
			{
                dynamic packageDisplay = context.New.ProjectInfo(
					ProjectCount: 10					
				);

                context.Shape.Zones["Content"].Add(packageDisplay);
			}
		}
    }
}