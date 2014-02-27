namespace Orchard.Gallery.RatingSynchronization {
    public interface IRatingSynchronizer : IDependency {
        void Synchronize();
    }
}