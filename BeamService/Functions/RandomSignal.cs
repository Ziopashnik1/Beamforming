using System;

namespace BeamService.Functions
{
    public class RandomSignal : AmplitudeSignalFunction
    {
        private const int SamplesCount = 1024;

        private readonly double[] f_RandomSamples;
        private readonly Random f_Random = new Random();

        private double f_Mu;

        public double Mu
        {
            get => f_Mu;
            set => Set(ref f_Mu, value);
        }

        public RandomSignal() : this(0.01) { }

        public RandomSignal(double Amplitude) : base(Amplitude) => f_RandomSamples = f_Random.NormalVector(SamplesCount);

        public override double Value(double t) => f_RandomSamples[f_Random.Next(SamplesCount)] * Amplitude + f_Mu;
    }
}