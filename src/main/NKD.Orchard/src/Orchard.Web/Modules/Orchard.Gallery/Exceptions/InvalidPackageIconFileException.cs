namespace Orchard.Gallery.Exceptions {
    public class InvalidPackageIconFileException : InvalidMediaFileException {
        public InvalidPackageIconFileException()
            : base("The Package Icon selected for upload is invalid.") {}
    }
}