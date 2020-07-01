using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using SideNotes.Services;
using SideNotes.Services.Abstract;
using SideNotes.Services.Templates;

namespace SideNotes.Application.Windsor.Installers
{
    public class ApplicationComponentsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<ITemplateLoader>().Instance(new TemplateLoader("Resources\\NotificationTemplates")).LifeStyle.Singleton);
            container.Register(Component.For<ICommentManager>().ImplementedBy<CommentManager>().LifeStyle.Transient);
            container.Register(Component.For<ICommentNotifier>().ImplementedBy<CommentNotifier>().LifeStyle.Transient);
            container.Register(Component.For<UserAvatarService>().ImplementedBy<UserAvatarService>().LifeStyle.Transient);
            container.Register(Component.For<BookAvatarService>().ImplementedBy<BookAvatarService>().LifeStyle.Transient);
            container.Register(Component.For<ParserFactory>().ImplementedBy<ParserFactory>().LifeStyle.Transient);
            //container.Register(Component.For<IBookBuilder>().ImplementedBy<BookBuilderDummy>().LifeStyle.Transient);
            container.Register(Component.For<IBookBuilder>().ImplementedBy<BookBuilder>().LifeStyle.Transient);
            //container.Register(Component.For<IInflector>().ImplementedBy<Inflector>().DependsOn(new { yaCaser = new YaInflector() }).LifeStyle.Transient);
            //container.Register(Component.For<INotificationSender>().ImplementedBy<NotificationSender>().LifeStyle.Transient);
        }
    }
}