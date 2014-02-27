using JetBrains.Annotations;
using Orchard.Localization;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;

namespace Orchard.Gallery.Handlers {
    [UsedImplicitly]
    public class ContactOwnersMessageAlteration : IMessageEventHandler {
        public Localizer T { get; set; }

        public ContactOwnersMessageAlteration() {
            T = NullLocalizer.Instance;
        }

        public void Sending(MessageContext context) {
            if (context.Type != GalleryMessageTypes.ContactOwners) {
                return;
            }
            string packageId = context.Properties["PackageId"];
            string reportBody = context.Properties["ReportBody"];
            string reporterUserName = context.Properties["ReporterUserName"];
            string reporterEmail = context.Properties["ReporterEmail"];
            LocalizedString subject = T("Message for owners of package '{0}'", packageId);
            LocalizedString body = T("The user <strong>{1}</strong> ({2}) has submitted the following message to the owners of Package '{0}'." +
                "<br /><br />{3}<br /><br />" +
                "- The Orchard Gallery Team",
                packageId, reporterUserName, reporterEmail, reportBody);

            context.MailMessage.Subject = subject.Text;
            context.MailMessage.Body = body.Text;
            context.MessagePrepared = true;
        }

        public void Sent(MessageContext context) { }
    }
}