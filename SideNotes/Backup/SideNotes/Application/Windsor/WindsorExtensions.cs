using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.Windsor;
using Castle.Core;
using System.Reflection;
using Castle.MicroKernel.Registration;

namespace SideNotes.Application.Windsor
{
    public static class WindsorExtensions
    {
        public static IWindsorContainer RegisterControllers(this IWindsorContainer container, params Type[] controllerTypes)
        {
            foreach (var type in controllerTypes)
            {
                if (type.Name.EndsWith("Controller") && !type.IsAbstract)
                {
                    container.Register(Component.For(type).Named(type.FullName.ToLower()).LifeStyle.Is(LifestyleType.Transient));
                }
            }

            return container;
        }

        public static IWindsorContainer RegisterControllers(this IWindsorContainer container, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                container.RegisterControllers(assembly.GetExportedTypes());
            }
            return container;
        }

        public static T ResolveOrDefault<T>(this IWindsorContainer container)
        {
            return container.ResolveAll<T>().FirstOrDefault();
        }
    }
}