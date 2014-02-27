using JetBrains.Annotations;
using Orchard.Localization;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;

namespace Orchard.Gallery.Handlers {
    [UsedImplicitly]
    public class ReportAbuseMessageAlteration : IMessageEventHandler {
        public Localizer T { get; set; }

        public ReportAbuseMessageAlteration() {
            T = NullLocalizer.Instance;
        }

        public void Sending(MessageContext context) {
            if (context.Type != GalleryMessageTypes.ReportAbuse) {
                return;
            }
            string packageId = context.Properties["PackageId"];
            string packageVersion = context.Properties["PackageVersion"];
            string reportBody = context.Properties["ReportBody"];
            string reporterUserName = context.Properties["ReporterUserName"];
            string reporterEmail = context.Properties["ReporterEmail"];
            LocalizedString subject = T("Abuse Report for Package '{0}' Version '{1}'", packageId, packageVersion);
            LocalizedString body = T("The user <strong>{2}</strong> ({3}) has reported abuse on Package '{0}', version '{1}'." +
                " {2} left the following information in the report:<br /><br />{4}<br /><br />" +
                "- The Orchard Gallery Team",
                packageId, packageVersion, reporterUserName, reporterEmail, reportBody);

            context.MailMessage.Subject = subject.Text;
            context.MailMessage.Body = body.Text;
            context.MessagePrepared = true;
        }

        public void Sent(MessageContext context) { }
    }
}