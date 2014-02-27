namespace Orchard.Gallery.ManagePackageIds {
    public sealed class PackageIdStatus {
        public static readonly PackageIdStatus InUse = new PackageIdStatus("In Use");
        public static readonly PackageIdStatus Unfinished = new PackageIdStatus("Unfinished");
        public static readonly PackageIdStatus Registered = new PackageIdStatus("Registered");

        public readonly string Name;

        private PackageIdStatus(string name) {
            Name = name;
        }
    }
}