using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Gallery.Models;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Gallery.Services {
    [UsedImplicitly]
    public class MissingSettingsBanner: INotificationProvider {
        private readonly IOrchardServices _orchardServices;

        public MissingSettingsBanner(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {

            var gallerySettings = _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>();

            if (gallerySettings == null || string.IsNullOrWhiteSpace(gallerySettings.ServiceRoot) ||
                !Uri.IsWellFormedUriString(gallerySettings.ServiceRoot, UriKind.Absolute))
            {
                yield return new NotifyEntry
                {
                    Message = T("The Gallery Server Service Root needs to be configured."),
                    Type = NotifyType.Warning
                };
            }

            if (gallerySettings == null || string.IsNullOrWhiteSpace(gallerySettings.FeedUrl) ||
                !Uri.IsWellFormedUriString(gallerySettings.FeedUrl, UriKind.Absolute))
            {
                yield return new NotifyEntry { Message = T("The Gallery Server Feed URL needs to be configured." ),
                    Type = NotifyType.Warning};
            }
        }
    }
}
