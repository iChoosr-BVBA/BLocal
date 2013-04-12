using System;
using System.IO;
using System.Web;
using BLocal.Core;
using BLocal.Providers;
using BLocal.Web.Demo.App_Start;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace BLocal.Web.Demo.App_Start
{
    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            // configure required providers using out-of-the-box implementations
            var localizationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "localization.xml");
            var valueProvider = new XmlValueProvider(localizationFilePath, true);
            var notifier = new ExceptionNotifier();
            var localeProvider = new MvcLocaleProvider("en", "nl");
            var partProvider = new MvcPartProvider(new Part("General"));
            var logger = new VoidLogger();

            // bind all required providers as singletons
            kernel.Bind<ILocalizedValueProvider>().ToConstant(valueProvider).InSingletonScope();
            kernel.Bind<INotifier>().ToConstant(notifier).InSingletonScope();
            kernel.Bind<ILocaleProvider>().ToConstant(localeProvider).InSingletonScope();
            kernel.Bind<IPartProvider>().ToConstant(partProvider).InSingletonScope();
            kernel.Bind<ILocalizationLogger>().ToConstant(logger).InSingletonScope();

            // set up the repository and context, this is actually our goal here!
            kernel.Bind<LocalizationRepository>().ToSelf().InRequestScope();
            kernel.Bind<ILocalizationContext>().To<AlwaysDebuggingContext>().InRequestScope();
        }
    }
}
