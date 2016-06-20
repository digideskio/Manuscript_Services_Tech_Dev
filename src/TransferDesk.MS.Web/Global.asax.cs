//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Optimization;
//using System.Web.Routing;

//namespace TransferDesk.MS.Web
//{
//    public class MvcApplication : System.Web.HttpApplication
//    {
//        protected void Application_Start()
//        {
//            AreaRegistration.RegisterAllAreas();
//            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
//            RouteConfig.RegisterRoutes(RouteTable.Routes);
//            BundleConfig.RegisterBundles(BundleTable.Bundles);
//        }
//    }
//}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Optimization;
//using System.Web.Routing;

//namespace TransferDesk.MS.Web
//{
//    public class MvcApplication : System.Web.HttpApplication
//    {
//        protected void Application_Start()
//        {
//            AreaRegistration.RegisterAllAreas();
//            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
//            RouteConfig.RegisterRoutes(RouteTable.Routes);
//            BundleConfig.RegisterBundles(BundleTable.Bundles);
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using TransferDesk.Services;
using TransferDesk.Services.Manuscript;
using SimpleInjector.Integration.Web.Mvc;
using TransferDesk.Contracts.Logging;
using TransferDesk.DAL.Manuscript.Repositories;
using TransferDesk.Logger;

namespace TransferDesk.MS.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //List<string> listOfString = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);



            //compositon root for object graph using the IoC Container
            var simpleInjectorcontainer = new SimpleInjector.Container();

            //Register all components
            RegisterAllComponents(stringBuilder, simpleInjectorcontainer);

            var logger = simpleInjectorcontainer.GetInstance<ILogger>();

            var fileLogger = logger as IFileLogger;
            fileLogger.FilePath = "d:\\TransferdeskLog\\";
            fileLogger.FileName = "TransferDeskLog";

            fileLogger.WriteStringBuilderToDiskAndClear(stringBuilder);

            stringBuilder.Length = 0;

            stringBuilder.AppendLine("Try Register the container as  IDependencyResolver.");

            fileLogger.WriteStringBuilderToDiskAndClear(stringBuilder);

            try
            {
                // Register the container as  IDependencyResolver.
                DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(simpleInjectorcontainer));
            }
            catch (Exception exception)
            {
                fileLogger.LogException(exception);
            }

        }

        private void RegisterAllComponents(StringBuilder stringBuilder, Container simpleInjectorContainer)
        {
            try
            {
                // Set the scoped lifestyle one directly after creating the container//web request works per request
                simpleInjectorContainer.Options.DefaultScopedLifestyle = new WebRequestLifestyle(true);

                var conString = Convert.ToString(ConfigurationManager.AppSettings["dbTransferDeskService"]);

                //below is a static class registered as singleton to manage mutiple thread instance sync locked.
                simpleInjectorContainer.RegisterSingleton<ILogger, LogHelper>();

                stringBuilder.AppendLine("Application started on computer " + System.Environment.MachineName + " DomainName " + System.Environment.UserDomainName);

                stringBuilder.AppendLine("Try Register all objects");

                //////simpleInjectorContainer.Register<ManuscriptService>(() => new ManuscriptService(conString, conString), Lifestyle.Scoped);

                //////stringBuilder.AppendLine("Registered Manuscript Service");

                //////simpleInjectorContainer.Register<ManuscriptDBRepositoryReadSide>(() => new ManuscriptDBRepositoryReadSide(conString), Lifestyle.Scoped);

                //////stringBuilder.AppendLine("Registered Manuscript DB repository for readside");

                //simpleInjectorcontainer.RegisterMvcControllers(Assembly.GetExecutingAssembly());

                stringBuilder.AppendLine("Registered MVC Controllers");

                stringBuilder.AppendLine("DI container try verify");

                //verify the container's configuration success for given dependencies.
                simpleInjectorContainer.Verify();

                stringBuilder.AppendLine("DI container verify done");

            }
            catch (Exception exception)
            {
                //throw; essential to catch before first trace log, stringbuilder instance will identify steps skipped
                stringBuilder.AppendLine("Exception in app_start " + exception.ToString());
            }
        }
    }
}

