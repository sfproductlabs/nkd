using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using NKD.Models;

namespace NKD.Drivers {
    [UsedImplicitly]
    public class ProjectPartDriver : ContentPartDriver<ProjectPart> {
        protected override string Prefix {
            get { return "ProjectPart"; }
        }

        protected override DriverResult Display(ProjectPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_NKD_Projects",
                    () => shapeHelper.Parts_NKD_Projects())
                );
        }

        protected override DriverResult Editor(ProjectPart part, dynamic shapeHelper)
        {
            var results = new List<DriverResult> {
                ContentShape("Parts_NKD_Projects_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Add", Model: part, Prefix: Prefix))
            };


            if (part.Id > 0)
                results.Add(ContentShape("NKD_Projects_DeleteButton",
                    deleteButton => deleteButton));

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(ProjectPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(ProjectPart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            //var description = context.Attribute(part.PartDefinition.Name, "Description");
        }

        protected override void Exporting(ProjectPart part, Orchard.ContentManagement.Handlers.ExportContentContext context) {
            //context.Element(part.PartDefinition.Name).SetAttributeValue("Description", part.Description);
        }
    }
}