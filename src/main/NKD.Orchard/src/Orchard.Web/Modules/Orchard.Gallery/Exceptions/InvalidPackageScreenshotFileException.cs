namespace Orchard.Gallery.Exceptions {
    public class InvalidPackageScreenshotFileException : InvalidMediaFileException {
        public InvalidPackageScreenshotFileException()
            : base("The Package Screenshot selected for upload is invalid.") {}
    }
}