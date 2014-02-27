using System.Web;

namespace Orchard.Gallery.Interfaces {
    public interface IPackageScreenshotUploader : IDependency {
        void UploadPackageScreenshot(HttpPostedFileBase packageFile, string packageId, string packageVersion);
        void UploadPackageExternalScreenshot(string packageId, string packageVersion, string externalScreenshotUrl);
    }
}