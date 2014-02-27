using System;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.ManagePackageIds {
    public class ExpiredPackageIdNotifier : IExpiredPackageIdNotifier {
        private readonly IOrchardServices _orchardServices;
        private readonly IExpiredPackageIdMessenger _expiredPackageIdMessenger;
        private readonly ITypeCaster _typeCaster;

        public ExpiredPackageIdNotifier(IOrchardServices orchardServices, IExpiredPackageIdMessenger expiredPackageIdMessenger, ITypeCaster typeCaster) {
            _orchardServices = orchardServices;
            _expiredPackageIdMessenger = expiredPackageIdMessenger;
            _typeCaster = typeCaster;
        }

        public void NotifyUserIfPackageIdIsAboutToExpire(UserkeyPackage userkeyPackage, DateTime utcNow, DateTime expirationDate,
            bool packageIdIsNotInUseOnFeed) {
            int? daysToWarn = _typeCaster.CastTo<GallerySettingsPart>(_orchardServices.WorkContext.CurrentSite).DaysInAdvanceToWarnUserOfExpiration;
            if (daysToWarn.HasValue) {
                bool packageIdAtWarnDate = utcNow.Date == expirationDate.Date.AddDays(-daysToWarn.Value);
                if (packageIdAtWarnDate && packageIdIsNotInUseOnFeed) {
                    _expiredPackageIdMessenger.SendMessage(userkeyPackage, expirationDate);
                }
            }
        }
    }
}