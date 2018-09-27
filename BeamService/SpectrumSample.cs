using System.Numerics;

namespace BeamService
{
    public struct SpectrumSample
    {
        public readonly Complex value;
        public readonly double f;

        public double Abs => value.Magnitude;
        public double Arg => value.Phase;
        public double Frequency => f;

        public SpectrumSample(double f, Complex value)
        {
            this.f = f;
            this.value = value;
        }
    }
}