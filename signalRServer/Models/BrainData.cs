using System;

namespace signalRServer.Models
{
    public class BrainData
    {
        public DateTime TimeStamp { get; set; }
        public byte DevicePoorSignal { get; set; }
        public double EegPowerDelta { get; set; }
        public double EegPowerTheta { get; set; }
        public double EegPowerAlpha1 { get; set; }
        public double EegPowerAlpha2 { get; set; }
        public double EegPowerBeta1 { get; set; }
        public double EegPowerBeta2 { get; set; }
        public double EegPowerGamma1 { get; set; }
        public double EegPowerGamma2 { get; set; }
        public double Attention { get; set; }
        public double Meditation { get; set; }
        public double BlinkStrength { get; set; }
    }
}
