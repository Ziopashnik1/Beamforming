using System;

namespace BeamService.Functions
{
    public class NormalRandomSignal : AmplitudeSignalFunction
    {
        private const int SamplesCount = 1000000;

        private static readonly double[] __RandomSamples = new Random().NextNormal(SamplesCount);

        private readonly int _Step;

        private int _Position;

        private double _Mu;

        public double Mu { get => _Mu; set => Set(ref _Mu, value); }

        public NormalRandomSignal() : this(0.01) { }

        public NormalRandomSignal(double Amplitude) : base(Amplitude)
        {
            var rnd = new Random();
            _Position = _Step = rnd.Next(17, 74);
        }

        public override double Value(double t)
        {
            _Position += _Step;
            _Position %= SamplesCount;
            return __RandomSamples[_Position] * Amplitude + _Mu;
        }
    }
}