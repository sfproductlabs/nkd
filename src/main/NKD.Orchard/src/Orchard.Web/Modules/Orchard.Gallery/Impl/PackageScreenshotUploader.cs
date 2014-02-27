using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using JetBrains.Annotations;
using Orchard.Gallery.Exceptions;
using Orchard.Gallery.Interfaces;
using Orchard.Localization;
using Orchard.Media.Models;
using Orchard.Media.Services;
using System.Linq;
using Gallery.Core.Extensions;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class PackageScreenshotUploader : IPackageScreenshotUploader
    {
        private readonly IMediaService _mediaService;
        private readonly IPackageScreenshotValidator _packageScreenshotValidator;
        private readonly IGalleryScreenshotService _galleryScreenshotService;
        private readonly IPackageMediaDirectoryHelper _packageMediaDirectoryHelper;

        public Localizer T { get; set; }

        public PackageScreenshotUploader(IMediaService mediaService, IPackageScreenshotValidator packageScreenshotValidator,
            IGalleryScreenshotService galleryScreenshotService, IPackageMediaDirectoryHelper packageMediaDirectoryHelper)
        {
            _mediaService = mediaService;
            _packageMediaDirectoryHelper = packageMediaDirectoryHelper;
            _galleryScreenshotService = galleryScreenshotService;
            _packageScreenshotValidator = packageScreenshotValidator;

            T = NullLocalizer.Instance;
        }

        public void UploadPackageScreenshot(HttpPostedFileBase packageFile, string packageId, string packageVersion)
        {
            _packageScreenshotValidator.ValidateProjectScreenshot(packageFile.FileName.GetFileExtension());
            string packageScreenshotDirectory = _packageMediaDirectoryHelper.GetPackageScreenshotsDirectory(packageId, packageVersion);
            string absolutePath = _packageMediaDirectoryHelper.GetAbsolutePathtoPackageScreenshotsDirectory(packageId, packageVersion);
            try
            {
                if (!Directory.Exists(absolutePath))
                {
                    _mediaService.CreateFolder(null, packageScreenshotDirectory);
                }
                IEnumerable<MediaFile> mediaFiles = _mediaService.GetMediaFiles(packageScreenshotDirectory);
                var existingMediaFile = mediaFiles.SingleOrDefault(mf => mf.Name == packageFile.FileName);
                if (existingMediaFile != null) {
                    _mediaService.DeleteFile(existingMediaFile.Name, existingMediaFile.FolderName);
                }
                string screenshotUrl = _mediaService.UploadMediaFile(packageScreenshotDirectory, packageFile, false);
                if (string.IsNullOrWhiteSpace(screenshotUrl)) {
                    throw new UploadMediaFileFailedException(packageFile.FileName);
                }
                if (existingMediaFile == null) {
                    _galleryScreenshotService.CreateScreenshot(packageId, packageVersion, screenshotUrl);
                }
            }
            catch (Exception ex)
            {
                throw new PackageScreenshotUploadFailedException(ex);
            }
        }

        public void UploadPackageExternalScreenshot(string packageId, string packageVersion, string externalScreenshotUrl) {
            _galleryScreenshotService.CreateScreenshot(packageId, packageVersion, externalScreenshotUrl);
        }
    }
}