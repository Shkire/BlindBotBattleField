using Owin;
using System.Web.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi
{
    //Class for WebApi configuration
    public static class Startup
    {
        //Configures WebApi for SelfHosting
        public static void ConfigureApp(IAppBuilder i_appBuilder)
        {
            //Creates a new configuration of HttpServer
            HttpConfiguration config = new HttpConfiguration();

            //Maps the route template and sets default route values
            //config.Routes.MapHttpRoute("DefaultApi","api/{controller}/{id}", new {id = RouteParameter.Optional });

            config.MapHttpAttributeRoutes();

            i_appBuilder.UseWebApi(config);
        }
    }
}
