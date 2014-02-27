using System;

namespace Orchard.Gallery.Interfaces {
    public interface ILastSynchronizedDate : IDependency {
        DateTime Get();
        void Set(DateTime lastSynchronizationDate);
    }
}