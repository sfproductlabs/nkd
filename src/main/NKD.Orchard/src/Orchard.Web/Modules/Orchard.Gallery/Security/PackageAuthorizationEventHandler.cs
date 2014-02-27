using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Gallery.Security {
    [UsedImplicitly]
    public class PackageAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IUserkeyPackageService _userKeyPackageService;

        public PackageAuthorizationEventHandler(IUserkeyPackageService userkeyPackageService) {
            _userKeyPackageService = userkeyPackageService;
        }
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if (!context.Granted &&
                context.Content.Is<PackagePart>()) {

                if (OwnerVariationExists(context.Permission) &&
                    HasOwnership(context.User, context.Content)) {

                    context.Adjusted = true;
                    context.Permission = GetOwnerVariation(context.Permission);
                }
            }
        }

        private bool HasOwnership(IUser user, IContent content) {
            string packageId = content.ContentItem.As<PackagePart>().PackageID;
            return _userKeyPackageService.UserCanAccessPackage(packageId, user.Id);
        }

        private bool OwnerVariationExists(Permission permission) {
            return GetOwnerVariation(permission) != null;
        }

        private Permission GetOwnerVariation(Permission permission) {
            if (permission == Permissions.ManagePackages)
                return Permissions.ManageOwnPackages;

            return null;
        }
    }
}