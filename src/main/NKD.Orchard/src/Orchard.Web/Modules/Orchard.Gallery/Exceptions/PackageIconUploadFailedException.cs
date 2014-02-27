using System;

namespace Orchard.Gallery.Exceptions {
    public class PackageIconUploadFailedException : Exception {
        public PackageIconUploadFailedException()
            : this(null)
        { }
        public PackageIconUploadFailedException(Exception innerException)
            : base("The icon file could not be saved. Please check to make sure it is a valid file type.", innerException)
        { }
    }
}