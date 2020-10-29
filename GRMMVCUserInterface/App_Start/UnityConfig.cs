using GRMMVCUserInterface.Library.API;
using GRMMVCUserInterface.Library.Helpers;
using GRMMVCUserInterface.Library.Models;
using System.Web.Mvc;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Lifetime;
//using Unity.Mvc5;

namespace GRMMVCUserInterface
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();                

            container.RegisterSingleton<ILoggedInUserModel, LoggedInUserModel>()
                     .RegisterSingleton<IConfigHelper, ConfigHelper>()
                     .RegisterSingleton<IAPIHelper, APIHelper>();

            container.RegisterType<IProductEndpoint, ProductEndpoint>(new PerRequestLifetimeManager())
                     .RegisterType<ISaleEndPoint, SaleEndPoint>(new PerRequestLifetimeManager());

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}