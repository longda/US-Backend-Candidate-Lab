[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Notesy.Api.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Notesy.Api.App_Start.NinjectWebCommon), "Stop")]

namespace Notesy.Api.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    using Notesy.Core.Services.Concrete;
    using Notesy.Core.Services.Interfaces;
    using Notesy.Core.Services.Stubs;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                // Note: Look how easy that is to swap out the fake services (which might not hit a real db) for real ones.  I bet we could 
                // even do that with a config setting or db setting too (or not)!

                //RegisterServices(kernel);
                RegisterStubServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<INoteService>().To<NoteService>().InRequestScope();
            kernel.Bind<IApiUserService>().To<ApiUserService>().InRequestScope();
        }

        private static void RegisterStubServices(IKernel kernel)
        {
            kernel.Bind<INoteService>().To<NoteServiceStub>().InRequestScope();
            kernel.Bind<IApiUserService>().To<ApiUserServiceStub>().InRequestScope();
        }
    }
}
