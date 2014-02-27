using System.Linq;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Services;

namespace Orchard.Gallery.Drivers {
    public class GallerySummaryWidgetPartDriver : ContentPartDriver<GallerySummaryWidgetPart> {
        private readonly IClock _clock;
        private readonly IRepository<PackagePartRecord> _packageRepository;
        private readonly IPackageService _packageService;

        public GallerySummaryWidgetPartDriver(IRepository<PackagePartRecord> packageRepository, IPackageService packageService, IClock clock) {
            _packageRepository = packageRepository;
            _packageService = packageService;
            _clock = clock;
        }

        protected override DriverResult Display(GallerySummaryWidgetPart part, string displayType, dynamic shapeHelper) {
            int totalPackages = _packageService.CountOfPackages();
            int packagesAddedInLastDay = _packageService.CountOfPackages(p => p.Created > _clock.UtcNow.Date);
            int totalDownloads = _packageRepository.Fetch(p => true).Sum(p => p.TotalDownloadCount);
            int uniquePackages = _packageService.CountOfPackages(p => p.IsRecommendedVersion);

            return ContentShape("Parts_GallerySummaryWidget",
                () => shapeHelper.Parts_GallerySummaryWidget(TotalPackages: totalPackages, UniquePackages: uniquePackages, TotalDownloads: totalDownloads,
                    NewPackages: packagesAddedInLastDay));
        }
    }
}