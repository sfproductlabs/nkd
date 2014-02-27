using JetBrains.Annotations;
using Orchard.Localization;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;

namespace Orchard.Gallery.Handlers {
    [UsedImplicitly]
    public class PackageIdExpirationWarningMessageAlteration : IMessageEventHandler {
        public Localizer T { get; set; }

        public PackageIdExpirationWarningMessageAlteration() {
            T = NullLocalizer.Instance;
        }

        public void Sending(MessageContext context) {
            if (context.Type != GalleryMessageTypes.PackageIdExpirationWarning) {
                return;
            }

            string userName = context.Properties["UserName"];
            string siteName = context.Properties["SiteName"];
            string packageId = context.Properties["PackageId"];
            string expirationDate = context.Properties["ExpirationDate"];

            LocalizedString subject = T("Package ID Expiration Warning");
            LocalizedString body = T("Dear {0},<br />" +
                "You registered the Package ID {1} on {2}. This registration will expire on {3}. " +
                "If you intend to use this ID you will need to upload a package using the ID before the expiration date. " +
                "Otherwise the registration for ID {1} will be deleted and become available to the community.<br /><br />" +
                "- The {2} Team",
                userName, packageId, siteName, expirationDate);

            context.MailMessage.Subject = subject.Text;
            context.MailMessage.Body = body.Text;
            context.MessagePrepared = true;
        }

        public void Sent(MessageContext context) { }
    }
}