using MathCore;

namespace BeamService
{
    public struct SpectrumSample
    {
        public readonly Complex value;
        public readonly double f;

        public double Abs => value.Abs;
        public double Arg => value.Arg;
        public double Frequency => f;

        public SpectrumSample(double f, Complex value)
        {
            this.f = f;
            this.value = value;
        }
    }
}