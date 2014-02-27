using System;
using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.Services;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Services;

namespace Orchard.Gallery.ManagePackageIds
{
    [UsedImplicitly]
    public class ExpiredPackageIdTaskRunner : ThreadSafeActionBase, IExpiredPackageIdTaskRunner {
        private readonly IOrchardServices _orchardServices;
        private readonly IClock _clock;
        private readonly IPackageIdExpirationCoordinator _packageIdExpirationCoordinator;
        private readonly ITypeCaster _typeCaster;

        public ILogger Logger { get; set; }

        public ExpiredPackageIdTaskRunner(IOrchardServices orchardServices, IClock clock, IPackageIdExpirationCoordinator packageIdExpirationCoordinator,
            ITypeCaster typeCaster) {
            _orchardServices = orchardServices;
            _clock = clock;
            _packageIdExpirationCoordinator = packageIdExpirationCoordinator;
            _typeCaster = typeCaster;

            Logger = NullLogger.Instance;
        }

        protected override void ExecuteThreadSafeAction() {
            DateTime utcNow = _clock.UtcNow;
            DateTime? lastChecked = _typeCaster.CastTo<GallerySettingsPart>(_orchardServices.WorkContext.CurrentSite).LastPackageIdExpirationCheckTime;

            if (!lastChecked.HasValue) {
                _typeCaster.CastTo<GallerySettingsPart>(_orchardServices.WorkContext.CurrentSite).LastPackageIdExpirationCheckTime = utcNow.Date.AddHours(2);
                return;
            }

            int? numberOfDaysUntilPackageIdExpires = _typeCaster.CastTo<GallerySettingsPart>(_orchardServices.WorkContext.CurrentSite)
                .NumberOfDaysUntilPreregisteredPackageIdExpires;
            if (!numberOfDaysUntilPackageIdExpires.HasValue || utcNow.Date == lastChecked.Value.Date) {
                return;
            }
            try {
                _packageIdExpirationCoordinator.ProcessExpirations(numberOfDaysUntilPackageIdExpires.Value, utcNow);
                _typeCaster.CastTo<GallerySettingsPart>(_orchardServices.WorkContext.CurrentSite).LastPackageIdExpirationCheckTime = utcNow;
            }
            catch (Exception ex) {
                Logger.Error(ex, "An error occurred while attempting to check for expired Package IDs.");
            }
        }
    }
}