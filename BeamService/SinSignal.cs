using System;

namespace BeamService
{
    public class SinSignal : HarmonicSignalFunction
    {

        public SinSignal() { }
        public SinSignal(double Amplitude, double Frequency, double Phase = 0) : base(Amplitude, Frequency, Phase) { }

        public override double Value(double t) => Amplitude * Math.Sin(2 * Math.PI * (Frequency * t + Phase));
    }
}