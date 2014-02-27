using System;

namespace Orchard.Gallery.Exceptions {
    public class PackageCreationException : Exception {
        public PackageCreationException(Exception innerException)
            : base("Package could not be created.", innerException)
        { }
    }
}