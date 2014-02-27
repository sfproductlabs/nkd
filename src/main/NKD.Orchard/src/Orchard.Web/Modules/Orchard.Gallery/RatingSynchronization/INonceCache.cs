namespace Orchard.Gallery.RatingSynchronization {
    public interface INonceCache : IDependency {
        string Nonce { get; set; }
    }
}