using System.Collections.Generic;
using Contrib.Reviews.Models;
using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Services;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using System.Linq;

namespace Orchard.Gallery.Drivers {
    [UsedImplicitly]
    public class PackagePartDriver : ContentPartDriver<PackagePart> {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IUserPackageAuthorizer _userPackageAuthorizer;
        private readonly IRepository<ReviewRecord> _reviewRecordRepository;
        private readonly IOrchardServices _orchardServices;

        public PackagePartDriver(ITaxonomyService taxonomyService, IContentManager contentManager, IUserPackageAuthorizer userPackageAuthorizer,
            IRepository<ReviewRecord> reviewRecordRepository, IOrchardServices orchardServices) {
            _taxonomyService = taxonomyService;
            _orchardServices = orchardServices;
            _reviewRecordRepository = reviewRecordRepository;
            _userPackageAuthorizer = userPackageAuthorizer;
        }

        protected override DriverResult Display(PackagePart part, string displayType, dynamic shapeHelper) {
            var terms = _taxonomyService.GetTermsForContentItem(part.Id);
            var packageTypeTerms = terms.Where(t => t.GetLevels() == 0);
            var packageCategoryTerms = terms.Where(t => t.GetLevels() == 1);

            var packageTypeName = packageTypeTerms.Count() == 1 ? packageTypeTerms.First().Name : "";
            var packageTypeSlug = packageTypeTerms.Count() == 1 ? packageTypeTerms.First().Slug : "";
            var packageCategory = packageCategoryTerms.Count() == 1 ? packageCategoryTerms.First().Name : "None";

            var screenshots = _orchardServices.ContentManager.Query<ScreenshotPart, ScreenshotPartRecord>()
                .Where(sp => sp.PackageID == part.PackageID && sp.PackageVersion == part.PackageVersion)
                .List().Select(s => s.ScreenshotUri);
            var firstScreenshot = screenshots.FirstOrDefault();
            string firstScreenshotUrl = firstScreenshot ?? "";

            bool allowOwnerActions = false;
            if (_userPackageAuthorizer.AuthorizedToEditPackage(part.PackageID)) {
                allowOwnerActions = true;
            }
            bool isPublished = part.ContentItem.IsPublished();

            int numberOfReviews = _reviewRecordRepository.Count(r => r.ContentItemRecordId == part.ContentItem.Id);

            var shapes = new List<DriverResult> {
                ContentShape("Parts_PackageIsOwner", () => shapeHelper.Parts_PackageIsOwner(IsOwner: allowOwnerActions, IsAdmin: UserIsAdmin())),
                ContentShape("Parts_PackageTypeName", () => shapeHelper.Parts_PackageTypeName(PackageType: packageTypeName)),
                ContentShape("Parts_PackageIcon", () => shapeHelper.Parts_PackageIcon(IconUrl: part.IconUrl, FirstScreenshot: firstScreenshotUrl, PackageType: packageTypeName, PackageTypeSlug: packageTypeSlug, PackageID: part.PackageID)),
                ContentShape("Parts_PackageImages", () => shapeHelper.Parts_PackageImages(IconUrl: part.IconUrl, FirstScreenshot: firstScreenshotUrl, Screenshots: screenshots.ToList(), PackageType: packageTypeName, Title: part.Title)),
                ContentShape("Parts_PackageLink", () => shapeHelper.Parts_PackageLink(PackageID: part.PackageID, PackageTitle: part.Title, PackageVersion: part.PackageVersion, PackageTypeSlug: packageTypeSlug, Slug: part.Slug)),
                ContentShape("Parts_PackageAuthor", () => shapeHelper.Parts_PackageAuthor(Authors: part.Authors)),
                ContentShape("Parts_PackageDownloads", () => shapeHelper.Parts_PackageDownloads(TotalDownloads: part.TotalDownloadCount)),
                ContentShape("Parts_PackageReviews", () => shapeHelper.Parts_PackageReviews(TotalNumberOfReviews: numberOfReviews, PackageType: packageTypeSlug, PackageID: part.PackageID)),
                ContentShape("Parts_PackageActions", () => shapeHelper.Parts_PackageActions(DownloadUrl: part.DownloadUrl)),
                ContentShape("Parts_PackageOwnerActions", () => shapeHelper.Parts_PackageOwnerActions(PackageID: part.PackageID, PackageVersion: part.PackageVersion, AllowOwnerActions: allowOwnerActions, IsPublished: isPublished)),

                ContentShape("Parts_PackageDetails", () => shapeHelper.Parts_PackageDetails(Package: part, PackageType: packageTypeName, PackageCategory: packageCategory, IsPublished: isPublished)),
                ContentShape("Parts_PackageDetailsHeader", () => shapeHelper.Parts_PackageDetailsHeader(Title: part.Title, PackageID: part.PackageID, Version: part.PackageVersion, AllowOwnerActions: allowOwnerActions, Authors: part.Authors, IsRecommendedVersion: part.IsRecommendedVersion, Description: part.Description, ProjectUrl: part.ProjectUrl, LicenseUrl: part.LicenseUrl, ReportAbuseUrl: part.ReportAbuseUrl, IsPublished: isPublished, PackageType: packageTypeName)),
                ContentShape("Parts_PackageDetailsActions", () => shapeHelper.Parts_PackageDetailsActions(DownloadUrl: part.DownloadUrl, PackageID: part.PackageID)),
                ContentShape("Parts_Packages_Package_DisplayDetail", () => shapeHelper.Parts_Packages_Package_DisplayDetail(ContentPart: part, PackageType: packageTypeName)),
                ContentShape("Parts_Packages_Package_List", () => shapeHelper.Parts_Packages_Package_List(ContentPart: part)),
            };

            if (displayType == "SummaryOwner") {
                shapes.Add(ContentShape("Parts_PackageOwnerSummary", () => shapeHelper.Parts_PackageOwnerSummary(Summary: part.Summary)));
            }
            else {
                shapes.Add(ContentShape("Parts_PackageSummary", () => shapeHelper.Parts_PackageSummary(Summary: part.Summary, PackageID: part.PackageID, PackageVersion: part.PackageVersion, PackageTypeSlug: packageTypeSlug)));
            }

            return Combined(shapes.ToArray());
        }

        private bool UserIsAdmin() {
            return _orchardServices.WorkContext.CurrentUser != null &&
                   _orchardServices.WorkContext.CurrentUser.UserName == _orchardServices.WorkContext.CurrentSite.SuperUser;
        }

        protected override DriverResult Editor(PackagePart part, dynamic shapeHelper) {
            return ContentShape("Parts_Package", () => shapeHelper.EditorTemplate(TemplateName: "Parts/Gallery.Package.Fields", Model: part));
        }

        protected override DriverResult Editor(PackagePart packagePart, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(packagePart, Prefix, null, null);
            return Editor(packagePart, shapeHelper);
        }
    }
}