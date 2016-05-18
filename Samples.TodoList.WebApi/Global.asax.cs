using System.Web.Http;
using Akka.Actor;

namespace Samples.TodoList.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            ActorSystemRefs.CreateActorSystem();

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_Stop()
        {
            
        }

    }
}
