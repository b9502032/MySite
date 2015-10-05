using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Web.SignalR;

[assembly: OwinStartup(typeof(Web.Startup))]
namespace Web
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            app.MapSignalR<ChatHubConnection>("/chat",new ConnectionConfiguration(){});
        }
    }
}