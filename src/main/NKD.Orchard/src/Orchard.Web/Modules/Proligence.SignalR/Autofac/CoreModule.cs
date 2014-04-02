using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Orchard.Mvc;
using Proligence.SignalR.Core;
using Proligence.SignalR.Core.Hubs;

namespace Proligence.SignalR.Autofac
{
    [UsedImplicitly]
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder moduleBuilder)
        {
            moduleBuilder
                .RegisterType<AutofacDependencyResolver>()
                .As<IDependencyResolver>()
                .InstancePerMatchingLifetimeScope("shell");

            moduleBuilder
                .RegisterType<TaskFriendlyHttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .InstancePerLifetimeScope();

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