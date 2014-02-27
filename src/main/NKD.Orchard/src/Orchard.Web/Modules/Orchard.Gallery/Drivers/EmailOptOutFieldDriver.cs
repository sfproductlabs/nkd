using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Gallery.Fields;

namespace Orchard.Gallery.Drivers {
    public class EmailOptOutFieldDriver : ContentFieldDriver<EmailOptOutField> {

        private static string GetPrefix(EmailOptOutField field, ContentPart part)
        {
            return part.PartDefinition.Name + "." + field.Name;
        }

        // GET
        protected override DriverResult Editor(ContentPart part, EmailOptOutField field, dynamic shapeHelper)
        {
            return ContentShape("Fields_EmailOptOut_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Fields/EmailOptOut", Model: field, Prefix: GetPrefix(field, part)));
        }

        //POST
        protected override DriverResult Editor(ContentPart part, EmailOptOutField field, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(field, GetPrefix(field, part), null, null);
            return Editor(part, field, shapeHelper);
        }
    }
}