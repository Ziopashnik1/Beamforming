using System;

namespace BeamService
{
    public class CosSignal : HarmonicSignalFunction
    {
        public override double Value(double t) => Amplitude * Math.Cos(2 * Math.PI * (Frequency * t + Phase));
    }
}