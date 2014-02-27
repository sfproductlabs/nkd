using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Gallery.Interfaces;
using Orchard.Media.Models;
using Orchard.Media.Services;

namespace Orchard.Gallery.Impl {
    public class PackageScreenshotDeleter : IPackageScreenshotDeleter {
        private readonly IGalleryScreenshotService _galleryScreenshotService;
        private readonly IMediaService _mediaService;
        private readonly IPackageMediaDirectoryHelper _packageMediaDirectoryHelper;

        public PackageScreenshotDeleter(IGalleryScreenshotService galleryScreenshotService, IMediaService mediaService,
            IPackageMediaDirectoryHelper packageMediaDirectoryHelper) {
            _galleryScreenshotService = galleryScreenshotService;
            _packageMediaDirectoryHelper = packageMediaDirectoryHelper;
            _mediaService = mediaService;
        }

        public void DeletePackageScreenshot(string packageId, string packageVersion, string screenshotId, string screenshotUrl) {
            Uri screenshotUri;

            if (!Uri.TryCreate(screenshotUrl, UriKind.Absolute, out screenshotUri)) {
                string screenshotFileName = GetFilenameFromUrl(screenshotUrl);
                string packageScreenshotDirectory = _packageMediaDirectoryHelper.GetPackageScreenshotsDirectory(packageId, packageVersion);
                DeleteMediaFileIfItExists(packageScreenshotDirectory, screenshotFileName);
            }
            
            _galleryScreenshotService.DeleteScreenshot(screenshotId);
        }

        private void DeleteMediaFileIfItExists(string packageScreenshotDirectory, string screenshotFilename) {

            IEnumerable<MediaFile> mediaFiles = null;

            try {
                mediaFiles = _mediaService.GetMediaFiles(packageScreenshotDirectory);
            }
            catch (ArgumentException ex) {
                SwallowErrorIfDirectoryNotFoundOtherwiseRethrow(ex);
            }
            if (mediaFiles != null) {
                var existingMediaFile = mediaFiles.SingleOrDefault(mf => mf.Name == screenshotFilename);
                if (existingMediaFile != null) {
                    _mediaService.DeleteFile(existingMediaFile.Name, existingMediaFile.FolderName);
                }
            }
        }

        private void SwallowErrorIfDirectoryNotFoundOtherwiseRethrow(Exception ex) {
            if (!ex.Message.Contains("does not exist")) {
                throw ex;
            }
        }

        private string GetFilenameFromUrl(string url) {
            int indexOfLastSlash = url.LastIndexOf('/');
            if (indexOfLastSlash < url.Length) {
                return url.Substring(indexOfLastSlash + 1);
            } else {
                return string.Empty;
            }

        }
    }
}