namespace Orchard.Gallery.Interfaces {
    public interface IPackageIconValidator : IDependency {
        void ValidateProjectIcon(string fileExtension);
    }
}