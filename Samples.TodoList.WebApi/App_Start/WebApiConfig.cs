using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Akka.Actor;
using Autofac;
using Autofac.Core;
using Autofac.Integration.WebApi;
using Swashbuckle.Application;

namespace Samples.TodoList.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            var container = CreateContainer(config);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private static ILifetimeScope CreateContainer(HttpConfiguration config)
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(typeof (WebApiConfig).Assembly)
                .AsSelf()
                .InstancePerRequest()
                .PropertiesAutowired(PropertyWiringOptions.PreserveSetValues);

            builder.RegisterWebApiFilterProvider(config);

            builder.RegisterModule<ApiModule>();

            var container = builder.Build();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            return container;
        }
    }

    internal class ApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterInstance(ActorSystemRefs.ActorSystem)
                .AsSelf()
                .As<IActorRefFactory>();
        }
    }

    internal static class ActorSystemRefs
    {
        internal static ActorSystem ActorSystem;

        internal static IActorRef AggregateClassesActor;


        internal static void CreateActorSystem()
        {
            ActorSystem = ActorSystem.Create("MySystem");

            // AggregateClassesActor /// Hay que configurar el container antes de llegar aqui :(((
        }
    }
}
