using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using System.Web.Mvc;
using System.Reflection;

namespace SideNotes.Application.Windsor.Installers
{
    public class ControllersInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .RegisterControllers(Assembly.GetExecutingAssembly())
                .Register(
                    Component.For<IControllerFactory>().ImplementedBy<WindsorControllerFactory>()
                    /*,Component.For<IFilterProvider>().ImplementedBy<WindsorFilterAttributeFilterProvider>()*/);
        }
    }
}