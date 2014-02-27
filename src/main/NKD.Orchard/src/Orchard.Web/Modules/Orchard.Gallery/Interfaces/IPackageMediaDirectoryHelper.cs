namespace Orchard.Gallery.Interfaces {
    public interface IPackageMediaDirectoryHelper : IDependency {
        string GetPackageIconDirectory(string packageId, string packageVersion);
        string GetAbsolutePathToPackageIconDirectory(string packageId, string packageVersion);
        string GetPackageScreenshotsDirectory(string packageId, string packageVersion);
        string GetAbsolutePathtoPackageScreenshotsDirectory(string packageId, string packageVersion);
        string GetAbsolutePathToPackageMediaDirectory(string packageId, string packageVersion);
    }
}