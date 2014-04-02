using Autofac;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Proligence.SignalR.Core;

namespace Proligence.SignalR.Autofac
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder moduleBuilder)
        {
            moduleBuilder
                .RegisterType<AutofacDependencyResolver>()
                .As<IDependencyResolver>()
                .InstancePerDependency();

            moduleBuilder.RegisterSource(new HubsRegistrationSource());
            moduleBuilder.RegisterSource(new PersistentConnectionRegistrationSource());

            moduleBuilder.RegisterType<DefaultHubDescriptorProvider>()
                .As<IHubDescriptorProvider>()
                .InstancePerLifetimeScope()
                .InstancePerMatchingLifetimeScope("shell");

            moduleBuilder.RegisterType<NullAssemblyLocator>()
                .As<IAssemblyLocator>()
                .InstancePerLifetimeScope()
                .InstancePerMatchingLifetimeScope("shell");

        }
    }
}