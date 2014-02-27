using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Domain;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Security;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class PackageLogEntryService : IPackageLogEntryService {
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IGalleryPackageService _galleryPackageService;

        public PackageLogEntryService(IOrchardServices orchardServices, IMembershipService membershipService,
            IAuthenticationService authenticationService, IWorkContextAccessor workContextAccessor,
            IGalleryPackageService galleryPackageService) {
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _workContextAccessor = workContextAccessor;
            _galleryPackageService = galleryPackageService;
        }

        public IEnumerable<PackageLogEntry> GetUnprocessedLogEntries() {
            IUser superUser = _membershipService.GetUser(_workContextAccessor.GetContext().CurrentSite.SuperUser);
            _authenticationService.SetAuthenticatedUserForRequest(superUser);

            int lastPackageLogId = _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().LastPackageLogId;
            return _galleryPackageService.GetNewPackageLogs(lastPackageLogId).OrderBy(l => l.Id);
        }
    }
}