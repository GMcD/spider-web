using System;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.MySql;
using System.Configuration;
using ServiceStack.ServiceInterface.Validation;

namespace Rum.People
{
    /// <summary>
    /// Global HttpApplication
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        /// <summary>
        /// Web Service Singleton AppHost
        /// </summary>
        public class HelloAppHost : AppHostBase
        {
            /// <summary>
            /// Tell Service Stack the name of your application and where to find your web services
            /// </summary>
            public HelloAppHost()
                : base("Cloud Money Services", typeof(PersonsService).Assembly) {
            }

            /// <summary>
            /// The Configure method of the HelloAppHost is the central IoC container location.
            /// All configured services, plugins and resources are centrally managed from here.
            /// </summary>
            /// <param name="container"></param>
            public override void Configure(Funq.Container container) {

                SetConfig(new EndpointHostConfig
                {
                    GlobalResponseHeaders = {
                         { "Access-Control-Allow-Origin", "*" },
                         { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
                         { "Access-Control-Allow-Headers", "Content-Type" },
                         { "X-Powered-On", Environment.MachineName}
                    },
                    DefaultContentType = "application/json"
                });
         
                // Database Connection
                ConnectionStringSettingsCollection css = ConfigurationManager.ConnectionStrings;
                string cs = css["storage"].ConnectionString;
                container.Register<IDbConnectionFactory>(c =>
                    new OrmLiteConnectionFactory(cs, MySqlDialectProvider.Instance));

                Plugins.Add(new ValidationFeature());
                container.RegisterValidators(typeof(Person).Assembly);

                var loadStore = container.Resolve<StorageService>();
                loadStore.Get(null);
            }
        }

        /// <summary>
        /// Simple Application Start Event Handler which just initializes the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Start(object sender, EventArgs e)
        {
            var appHost = new HelloAppHost();
            appHost.Init();
        }
    }
}