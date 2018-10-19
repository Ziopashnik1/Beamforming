using System;

namespace BeamService
{
    public class CosSignal : HarmonicSignalFunction
    {
        public CosSignal() { }
        public CosSignal(double Amplitude, double Frequency, double Phase = 0) : base(Amplitude, Frequency, Phase) { }

        public override double Value(double t) => Amplitude * Math.Cos(2 * Math.PI * (Frequency * t + Phase));
    }
}