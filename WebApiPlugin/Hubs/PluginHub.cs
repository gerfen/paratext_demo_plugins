using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace WebApiPlugin.Hubs
{

    [HubName("Plugin")]
    public class PluginHub : Hub
    {
        private readonly IMediator _mediator;
        public PluginHub()
        {
            // TODO:  Investigate how to get SignalR to inject the mediator for us.
            //        I really hate this tight coupling.
            _mediator = WebHostStartup.ServiceProvider.GetService<IMediator>();
        }
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }

       
        public void SendVerse(string verse)
        {
            Clients.All.addMessage(verse);
        }

        public override async Task OnConnected()
        {
            // TODO;  Send data to clients on connection
            await base.OnConnected();
        }
    }
}
