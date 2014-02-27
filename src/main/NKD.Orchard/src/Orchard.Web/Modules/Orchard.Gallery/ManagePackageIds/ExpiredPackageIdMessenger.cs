using System;
using System.Collections.Generic;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Messaging.Services;
using Orchard.Users.Models;

namespace Orchard.Gallery.ManagePackageIds {
    public class ExpiredPackageIdMessenger : IExpiredPackageIdMessenger {
        private readonly IUserkeyService _userkeyService;
        private readonly IOrchardServices _orchardServices;
        private readonly IMessageManager _messageManager;

        public ExpiredPackageIdMessenger(IUserkeyService userkeyService, IOrchardServices orchardServices, IMessageManager messageManager) {
            _userkeyService = userkeyService;
            _orchardServices = orchardServices;
            _messageManager = messageManager;
        }

        public void SendMessage(UserkeyPackage userkeyPackage, DateTime expirationDate) {
            UserPart user = _userkeyService.GetUserForUserKey(userkeyPackage.UserkeyId);
            if (user == null) {
                return;
            }
            var terms = new Dictionary<string, string> {
                {"UserName", user.UserName},
                {"SiteName", _orchardServices.WorkContext.CurrentSite.SiteName},
                {"PackageId", userkeyPackage.PackageId},
                {"ExpirationDate", expirationDate.ToShortDateString()}
            };
            _messageManager.Send(user.ContentItem.Record, GalleryMessageTypes.PackageIdExpirationWarning, "email", terms);
        }
    }
}