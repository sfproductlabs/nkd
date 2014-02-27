using System;

namespace Orchard.Gallery.Exceptions {
    public class PackageScreenshotUploadFailedException : Exception
    {
        public PackageScreenshotUploadFailedException(Exception innerException)
            : base("The Screenshot could not be saved. Please check to make sure it is a valid file type.", innerException)
        { }
    }
}