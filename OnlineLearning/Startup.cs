using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using OnlineLearning.Models;
using Owin;

[assembly: OwinStartupAttribute(typeof(OnlineLearning.Startup))]
namespace OnlineLearning
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            var idProvider = new CustomUserIdProvider();

            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => idProvider);

            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}
