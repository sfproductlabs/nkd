using System;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Security;

namespace Orchard.Gallery.Handlers {
    public class GalleryFeatureHandler : IFeatureEventHandler {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IOrchardServices _services;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IMembershipService _membershipService;
        private readonly IUserkeyService _userkeyService;

        public GalleryFeatureHandler(ITaxonomyService taxonomyService, IOrchardServices services, IContentDefinitionManager contentDefinitionManager,
            IMembershipService membershipService, IUserkeyService userkeyService) {
            _taxonomyService = taxonomyService;
            _userkeyService = userkeyService;
            _membershipService = membershipService;
            _services = services;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public void Installing(Feature feature) {

        }

        public void Installed(Feature feature) {

        }

        public void Enabling(Feature feature) {

        }

        public void Enabled(Feature feature) {
            SetInitialGallerySettings();
            SetupPackageTaxonomy();
            GenerateKeyForSuperUser();
        }

        private void SetInitialGallerySettings() {
            var gallerySettingsPart = _services.WorkContext.CurrentSite.As<GallerySettingsPart>();
            if (string.IsNullOrWhiteSpace(gallerySettingsPart.ServiceRoot)) {
                gallerySettingsPart.ServiceRoot = "http://localhost/GalleryServer/";
            }
            if (string.IsNullOrWhiteSpace(gallerySettingsPart.FeedUrl)) {
                gallerySettingsPart.FeedUrl = "http://localhost/GalleryServer/FeedService.svc";
            }
        }

        private void SetupPackageTaxonomy() {
            const string taxonomyName = "Package Types";

            if (_taxonomyService.GetTaxonomyByName(taxonomyName) == null)
            {
                var taxonomy = _services.ContentManager.New<TaxonomyPart>("Taxonomy");
                taxonomy.Name = taxonomyName;
                taxonomy.Slug = "PackageTypes";

                _services.ContentManager.Create(taxonomy, VersionOptions.Published);
                _taxonomyService.CreateTermContentType(taxonomy);

                _contentDefinitionManager.AlterPartDefinition("PackagePart", builder =>
                    builder.WithField("PackageType", fieldBuilder => fieldBuilder
                        .OfType("TaxonomyField")
                        .WithSetting("TaxonomyFieldSettings.TaxonomyId", taxonomy.Id.ToString())
                        .WithSetting("TaxonomyFieldSettings.LeavesOnly", false.ToString())
                        .WithSetting("TaxonomyFieldSettings.SingleChoice", true.ToString())));
            }
        }

        private void GenerateKeyForSuperUser() {
            IUser superUser = _membershipService.GetUser(_services.WorkContext.CurrentSite.SuperUser);
            _userkeyService.SaveKeyForUser(superUser.Id, Guid.NewGuid());
        }

        public void Disabling(Feature feature) {
        }

        public void Disabled(Feature feature) {
        }

        public void Uninstalling(Feature feature) {
        }

        public void Uninstalled(Feature feature) {
        }
    }
}