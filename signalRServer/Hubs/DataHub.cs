using Microsoft.AspNet.SignalR;
using signalRServer.Models;
using System;

namespace signalRServer.Hubs
{
    public class DataHub : Hub
    {
        public void ClientBroadcast(BrainData braindata)
        {
            DisplayBrainData(braindata);
            Clients.All.updateData(braindata);
        }

        public void BlinkBroadcast(double blinkStrength)
        {
            Console.WriteLine("BlinkStrength Received! Value: {0}", blinkStrength);
            Clients.All.getBlink(blinkStrength);
        }

        void DisplayBrainData(BrainData brainData)
        {
            Console.WriteLine("SignalR Server");

            if (brainData.DevicePoorSignal > 60)
                Console.WriteLine("DevicePoorSignal: {0} - BAD CONNECTION", brainData.DevicePoorSignal);
            else
                Console.WriteLine("DevicePoorSignal: {0} - CONNECTION OK", brainData.DevicePoorSignal);

            Console.WriteLine("TimeStamp: {0}", brainData.TimeStamp.ToString("yyyy-MM-dd hh:mm:ss.fff"));
            Console.WriteLine("Attention: {0}", brainData.Attention);
            Console.WriteLine("Meditation: {0}", brainData.Meditation);
            Console.WriteLine("BlinkStrength: {0}", brainData.BlinkStrength);
            Console.WriteLine("EegPowerAlpha1: {0}", brainData.EegPowerAlpha1);
            Console.WriteLine("EegPowerAlpha2: {0}", brainData.EegPowerAlpha2);
            Console.WriteLine("EegPowerBeta1: {0}", brainData.EegPowerBeta1);
            Console.WriteLine("EegPowerBeta2: {0}", brainData.EegPowerBeta2);
            Console.WriteLine("EegPowerDelta: {0}", brainData.EegPowerDelta);
            Console.WriteLine("EegPowerGamma1: {0}", brainData.EegPowerGamma1);
            Console.WriteLine("EegPowerGamma2: {0}", brainData.EegPowerGamma2);
            Console.WriteLine("EegPowerTheta: {0}", brainData.EegPowerTheta);

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
