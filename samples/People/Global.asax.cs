using System;
using System.IO;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.MySql;
using System.Configuration;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Validation;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Rum.People
{
    /// <summary>
    /// Global HttpApplication
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        private const string pattern = @"%-4x %d %-5p [%c{7}] (%F:%L) - %m%n";
        private static readonly log4net.ILog m_log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

		string soon = DateTime.Now.AddSeconds(10).ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");
                SetConfig(new EndpointHostConfig
                {
                    GlobalResponseHeaders = {
                         { "Access-Control-Allow-Origin", "*" },
                         { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
                         { "Access-Control-Allow-Headers", "Content-Type" },
			 { "Cache-Control", "no-cache, no-store, must-revalidate"},
			 { "Pragma", "no-cache"},
			 { "Expires", "0"},
                         { "X-Powered-On", Environment.MachineName}
                    },
                    DefaultContentType = "application/json"
                });
         
                // Auto Add all Routes
                Routes.AddFromAssembly(typeof(Person).Assembly);

		        // Fluent Validation
                Plugins.Add(new ValidationFeature());
                container.RegisterValidators(typeof(Person).Assembly);

                // Database Connection
                ConnectionStringSettingsCollection css = ConfigurationManager.ConnectionStrings;
                string cs = css["storage"].ConnectionString;
                container.Register<IDbConnectionFactory>(c =>
                    new OrmLiteConnectionFactory(cs, MySqlDialectProvider.Instance));

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
            String logConf = Path.Combine(Server.MapPath("/"), "log4net.xml");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(logConf));
            String logFile = Path.Combine(Server.MapPath("/"), "people.log");

            log4net.Appender.FileAppender fa = new log4net.Appender.FileAppender();
            fa.Name = "FileAppender";
            fa.File = logFile;
            fa.Layout = new log4net.Layout.PatternLayout(pattern);
            fa.Threshold = log4net.Core.Level.Debug;
            fa.AppendToFile = true;
            fa.ActivateOptions();

            log4net.Repository.Hierarchy.Hierarchy hierarchy = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            hierarchy.Root.AddAppender(fa);
            m_log.Info("Copyright Gary MacDonald 2012.");

            ServicePointManager.ServerCertificateValidationCallback +=
                delegate(object requestSender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    m_log.Info("Certificate Validation Callback!");
                    // **** Always accept for now XXX
                    return true; 
                };
            var appHost = new HelloAppHost();
            appHost.Init();
        }
    }
}
