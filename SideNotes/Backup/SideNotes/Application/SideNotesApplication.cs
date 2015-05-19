using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Installer;
using SideNotes.Application.Windsor;
using System.ComponentModel.DataAnnotations;
using SideNotes.Application.Validation;
using System.Transactions;
using SideNotes.Models;

namespace SideNotes.Application
{
    public abstract class SideNotesApplication : HttpApplication
    {
        static IWindsorContainer container;
        public IWindsorContainer Container
        {
            get { return container; }
        }

        protected abstract void RegisterRoutes(RouteCollection routes);

        protected void Application_Start()
        {
            InitContainer();
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(DataTypeAttribute), typeof(DataTypeAttributeAdapter));
            RegisterIgnores(RouteTable.Routes);
            RegisterRoutes(RouteTable.Routes);

            ConfigureOptionalServices();
        }

        protected void Application_BeginRequest()
        {
            try
            {
                var filepath = HttpContext.Current.Request.FilePath.ToLower();
                if (HttpContext.Current.Request.HttpMethod == "GET"
                    && !filepath.StartsWith("/scripts")
                    && !filepath.StartsWith("/content")
                    && !filepath.StartsWith("/usersimages")
                    && !filepath.StartsWith("/favicon.ico")
                    && !filepath.StartsWith("/bookadmin")
                    && !filepath.StartsWith("/notification")
                    && !filepath.StartsWith("/statistics"))
                {
                    UpdateStatistics();
                    WriteRequestLog();
                }
            }
            catch{}
        }
        protected void WriteRequestLog()
        {
            using (var context = new SideNotesEntities())
            {
                var from = HttpContext.Current.Request.ServerVariables.AllKeys.Contains("HTTP_FROM") ?
                    HttpContext.Current.Request.ServerVariables["HTTP_FROM"] : null;
                var user = HttpContext.Current.Request.ServerVariables.AllKeys.Contains("REMOTE_USER") ?
                    HttpContext.Current.Request.ServerVariables["REMOTE_USER"] : null;
                var ip = HttpContext.Current.Request.ServerVariables.AllKeys.Contains("REMOTE_ADDR") ?
                    HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] : null;
                var log = new RequestLog()
                {
                    Url = HttpContext.Current.Request.Url.ToString(),
                    HttpFrom = from,
                    Remote_addr = ip,
                    CurrentUser = user,
                    RequestDate = DateTime.Now
                };
                context.RequestLogs.AddObject(log);
                context.SaveChanges();
            }
        }
        protected void UpdateStatistics()
        {
            using (var context = new SideNotesEntities())
            {
                using (var scope = new TransactionScope())
                {
                    IncrementHits("Total", context);
                    if (HttpContext.Current.Request.QueryString.AllKeys.Contains("campaign"))
                    {
                        IncrementHits(HttpContext.Current.Request.QueryString["campaign"], context);
                    }
                    context.SaveChanges();
                    scope.Complete();
                }
            }
        }
        private void IncrementHits(string Id, SideNotesEntities context)
        {
            var campaign = context.DailyHits.FirstOrDefault(e => e.CampaignId == Id);
            if (campaign == null)
            {
                campaign = new DailyHit() { hits = 1, CampaignId = Id };
                context.DailyHits.AddObject(campaign);
            }
            else campaign.hits++;
        }

        private void ConfigureOptionalServices()
        {
            
            var controllerFactory = Container.ResolveOrDefault<IControllerFactory>();
            if (controllerFactory != null)
                ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            var binder = Container.ResolveOrDefault<IModelBinder>();
            if (binder != null)
                ModelBinders.Binders.DefaultBinder = binder;

            var filterProvider = Container.ResolveOrDefault<IFilterProvider>();
            if (filterProvider != null)
            {
                var oldProvider = FilterProviders.Providers.Single(f => f is FilterAttributeFilterProvider);
                FilterProviders.Providers.Remove(oldProvider);
                FilterProviders.Providers.Add(filterProvider);
            }
            
        }

        private void InitContainer()
        {
            container = new WindsorContainer(new XmlInterpreter());
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.Install(FromAssembly.This());
        }

        private void RegisterIgnores(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("Content/{*pathInfo}");
            routes.IgnoreRoute("UsersImages/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");
        }
    }
}