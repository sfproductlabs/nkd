using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.Gallery.Exceptions;
using Orchard.Gallery.Interfaces;
using Orchard.Localization;
using Orchard.Media.Models;
using Orchard.Media.Services;
using Gallery.Core.Extensions;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class PackageIconUploader : IPackageIconUploader {
        private readonly IMediaService _mediaService;
        private readonly IPackageIconValidator _packageIconValidator;
        private readonly IPackageMediaDirectoryHelper _packageMediaDirectoryHelper;

        public Localizer T { get; set; }

        public PackageIconUploader(IMediaService mediaService, IPackageIconValidator packageIconValidator, IPackageMediaDirectoryHelper packageMediaDirectoryHelper) {
            _mediaService = mediaService;
            _packageMediaDirectoryHelper = packageMediaDirectoryHelper;
            _packageIconValidator = packageIconValidator;

            T = NullLocalizer.Instance;
        }

        public string UploadPackageIcon(HttpPostedFileBase iconFile, string packageId, string packageVersion) {
            _packageIconValidator.ValidateProjectIcon(iconFile.FileName.GetFileExtension());
            string packageIconDirectory = _packageMediaDirectoryHelper.GetPackageIconDirectory(packageId, packageVersion);
            string absolutePathToPackageIconDirectory = _packageMediaDirectoryHelper.GetAbsolutePathToPackageIconDirectory(packageId, packageVersion);
            string mediaUrl;
            try {
                if (!Directory.Exists(absolutePathToPackageIconDirectory)) {
                    _mediaService.CreateFolder(null, packageIconDirectory);
                }
                DeleteOldIcons(absolutePathToPackageIconDirectory);
                mediaUrl = _mediaService.UploadMediaFile(packageIconDirectory, iconFile, false);
            }
            catch (Exception ex) {
                throw new PackageIconUploadFailedException(ex);
            }
            if (string.IsNullOrWhiteSpace(mediaUrl)) {
                throw new PackageIconUploadFailedException();
            }
            return mediaUrl;
        }

        private void DeleteOldIcons(string absolutePath) {
            try
            {
                foreach (var mediaFile in _mediaService.GetMediaFiles(absolutePath)) {
                    _mediaService.DeleteFile(mediaFile.Name, mediaFile.FolderName);
                }
            }
            catch (Exception) { }
        }
    }
}