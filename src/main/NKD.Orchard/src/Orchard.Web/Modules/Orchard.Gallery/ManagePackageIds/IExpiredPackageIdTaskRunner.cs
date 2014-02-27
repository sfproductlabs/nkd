namespace Orchard.Gallery.ManagePackageIds {
    public interface IExpiredPackageIdTaskRunner : IDependency {
        void Synchronize();
    }
}