namespace Orchard.Gallery.PackageSynchronization {
    public interface IPackageSynchronizer : IDependency {
        void Synchronize();
    }
}