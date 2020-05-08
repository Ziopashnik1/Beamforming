using System;
using System.Runtime.Serialization.Formatters;

namespace BeamService.Functions
{
    public class NormalRandomSignal : AmplitudeSignalFunction
    {
        private const int SamplesCount = 1000000;

        private static readonly double[] __RandomSamples;

        static NormalRandomSignal()
        {
            var rnd = new Random();
            __RandomSamples = rnd.NextNormal(SamplesCount);
        }

        private readonly int _Step;
        private int _Position;

        private double _Mu;

        public double Mu { get => _Mu; set => Set(ref _Mu, value); }

        public int M = 0;

        public NormalRandomSignal() : this(0.01) { }

        public NormalRandomSignal(double Amplitude) : base(Amplitude)
        {
            var rnd = new Random();
            _Position = _Step = rnd.Next(17, 74);
        }


        //public override double Value(double t)
        //{
        //    var N = f_Random.Next(SamplesCount);
        //    var M = 0;
        //    if (N == 0)
        //    {
        //        M = 1;
        //    }

        //    N += M;
        //    var A = __RandomSamples[N] * Amplitude;
        //    return A;
        //}


        public override double Value(double t)
        {
            _Position += _Step;
            _Position %= SamplesCount;
            return __RandomSamples[_Position] * Amplitude + _Mu;
        }

        //for (var i = 0; i<SamplesCount - 1; i++)
        //{
        //    var N = i;
        //}
    }
}