using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using SideNotes.Services.Abstract;
using SideNotes.Services;

namespace SideNotes.Application.Windsor.Installers
{
    public class WebSecurityInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IUserSession>().ImplementedBy<UserSession>().LifeStyle.Transient);
            container.Register(
                Component.For<IAuthorizationService>().ImplementedBy<AuthorizationService>().LifeStyle.Transient);
        }
    }
}