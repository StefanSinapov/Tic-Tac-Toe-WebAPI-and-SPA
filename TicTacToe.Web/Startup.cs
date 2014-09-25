using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(TicTacToe.Web.Startup))]

namespace TicTacToe.Web
{
    using System.Reflection;
    using System.Web.Http;

    using Ninject;
    using Ninject.Web.Common.OwinHost;
    using Ninject.Web.WebApi.OwinHost;

    using GameLogic;
    using Data;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            app.UseNinjectMiddleware(CreateKernel).UseNinjectWebApi(GlobalConfiguration.Configuration);
        }

        private static StandardKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            BindTypes(kernel);

            return kernel;
        }

        private static void BindTypes(StandardKernel kernel)
        {
            kernel.Bind<ITicTacToeData>().To<TicTacToeData>()
                .WithConstructorArgument("context", 
                        c => new ApplicationDbContext());

            kernel.Bind<IGameResultValidator>().To<GameResultValidator>();
        }
    }
}
