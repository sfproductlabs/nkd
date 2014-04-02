using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Microsoft.AspNet.SignalR;

namespace Proligence.SignalR.Autofac
{
    public class PersistentConnectionRegistrationSource : IRegistrationSource
    {
        /// <summary>
        /// Gets whether the registrations provided by this source are 1:1 adapters on top
        ///             of other components (I.e. like Meta, Func or Owned.)
        /// </summary>
        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var serviceWithType = service as IServiceWithType;
            if (serviceWithType == null)
                yield break;

            var serviceType = serviceWithType.ServiceType;
            if (!typeof(PersistentConnection).IsAssignableFrom(serviceType))
                yield break;

            var rb = RegistrationBuilder
                .ForType(serviceType)
                .As(typeof(PersistentConnection), serviceType)
                .OnRelease(instance =>
                {
                    var disposable = instance as IDisposable;
                    if (disposable == null) return;

                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                        // No exceptions during disposal 
                    }
                })
                .InstancePerDependency();

            yield return rb.CreateRegistration();
        }
    }
}