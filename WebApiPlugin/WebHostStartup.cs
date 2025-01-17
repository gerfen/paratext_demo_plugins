﻿using Owin;
using System.Net.Http.Headers;
using System.Web.Http;
using MediatR;
using Serilog;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
//using System.Web.Http.Controllers;
//using ClearDashboard.WebApiParatextPlugin.Features.Project;
//using ClearDashboard.WebApiParatextPlugin.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Owin.Cors;
using Paratext.PluginInterfaces;
using WebApiPlugin.Features.Verse;

namespace WebApiPlugin
{
    public class WebHostStartup
    {
        // The following are used to inject Singleton instances
        private static IProject _project;
        private static IVerseRef _verseRef;
        private static MainWindow _mainWindow;
        private static IWindowPluginHost _pluginHost;

        public static IServiceProvider ServiceProvider { get; private set; }

        //public Microsoft.AspNet.SignalR.DefaultDependencyResolver SignalRServiceResolver { get; private set; }

        public WebHostStartup(IProject project, IVerseRef verseRef, MainWindow mainWindow, IWindowPluginHost pluginHost)
        {
            _project = project;
            _verseRef = verseRef;
            _mainWindow = mainWindow;
            _pluginHost = pluginHost;
        }

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            try
            {
                ServiceProvider = SetupDependencyInjection();
                appBuilder.UseCors(CorsOptions.AllowAll);
                appBuilder.Map("/cors", map =>
                {
                    map.UseCors(CorsOptions.AllowAll);
                    map.MapSignalR(new HubConfiguration()
                    {
#if DEBUG
                        EnableDetailedErrors = true,
#endif
                    });
                });

                //var signalRServiceResolver = new CustomSignalRDependencyResolver(ServiceProvider);
                appBuilder.MapSignalR(new HubConfiguration()
                {
#if DEBUG
                    EnableDetailedErrors = true, 
                    //Resolver = signalRServiceResolver
#endif
                });

                //GlobalHost.DependencyResolver = signalRServiceResolver;

                var config = InitializeHttpConfiguration();
                appBuilder.UseWebApi(config);

            }
            catch(Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred while configuring Web API.");
            }
           
        }

        private HttpConfiguration InitializeHttpConfiguration()
        {
            var config = new HttpConfiguration();
            config.DependencyResolver = new DefaultDependencyResolver(ServiceProvider);
           // config.MessageHandlers.Add(new MessageLoggingHandler(_mainWindow));
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.EnsureInitialized(); //Nice to check for issues before first request

            return config;
        }

        private IServiceProvider SetupDependencyInjection()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            services.AddSingleton<MainWindow>(sp => _mainWindow);
            //services.AddSerilog();

            services.AddMediatR(typeof(GetCurrentVerseCommandHandler));
            
            
            services.AddSingleton<IProject>(sp => _project);
            services.AddSingleton<IVerseRef>(sp => _verseRef);
            services.AddSingleton<IWindowPluginHost>(sp =>_pluginHost);
           
            services.AddControllersAsServices(typeof(WebHostStartup).Assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => typeof(IHttpController).IsAssignableFrom(t)
                            || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)));

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }

    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddControllersAsServices(this IServiceCollection services,
            IEnumerable<Type> controllerTypes)
        {
            foreach (var type in controllerTypes)
            {
                services.AddScoped(type);
            }

            return services;
        }
    }

    public class CustomSignalRDependencyResolver : Microsoft.AspNet.SignalR.DefaultDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomSignalRDependencyResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return _serviceProvider.GetServices(serviceType);
        }
    }
}
