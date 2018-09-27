using System;

namespace BeamService
{
    public class SinSignal : HarmonicSignalFunction
    {
        public override double Value(double t) => Amplitude * Math.Sin(2 * Math.PI * (Frequency * t + Phase));
    }
}