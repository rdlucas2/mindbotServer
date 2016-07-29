using Microsoft.AspNet.SignalR;
using System;

namespace signalRServer.Hubs
{
    public class ModeHub : Hub
    {
        public void GetMode()
        {
            Console.WriteLine("Mode Requested...");
            Clients.All.reportMode();
        }

        public void ModeBroadcast(string mode)
        {
            Console.WriteLine("Chosen Mode: {0}", mode);
            Clients.All.setMode(mode);
        }

        public void PiConnected()
        {
            Console.WriteLine("Raspberry Pi Is Connected!");
        }

    }
}
