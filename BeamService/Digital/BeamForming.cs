using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSP.Lib;
using MathService;
using MathService.Vectors;

namespace BeamService.Digital
{
    /// <summary>Диаграммообразующая схема</summary>
    public abstract class BeamForming
    {
        public abstract DigitalSignal GetSignal(DigitalSignal[] Signals);
    }

    public class MatrixBeamForming : BeamForming
    {
        private readonly Vector3D[] _AntennaElementLocations;
        private readonly int _SamplesCount;
        private MatrixComplex _Wt;
        private const double pi2 = Consts.pi2;
        private const double c = Consts.SpeedOfLigth;



        public MatrixBeamForming(Vector3D[] AntennaElementLocations, int SamplesCount)
        {
            _AntennaElementLocations = AntennaElementLocations;
            _SamplesCount = SamplesCount;
            _Wt = MatrixComplex.Create(SamplesCount, SamplesCount, (i, j) => Complex.Exp(-pi2 * i * j / SamplesCount) / SamplesCount);
        }

        private static MatrixComplex GetPhasingMatrix(int ElementsCount, int SamplesCount, double df, Vector3D[] Locations, SpaceAngle Angle) => 
            MatrixComplex.Create(ElementsCount, SamplesCount, (i, m) =>
            {
                var fm = (m <= SamplesCount / 2 ? m : m - SamplesCount) * df;
                var location = Locations[i];
                return Complex.Exp(pi2 / c * fm * location.GetProjectionTo(Angle));
            });

        public override DigitalSignal GetSignal(DigitalSignal[] Signals)
        {
            var ss = GetSignalMatrix(Signals);
            var SS = ss * _Wt;
            throw new NotImplementedException();
        }

        private Matrix GetSignalMatrix(DigitalSignal[] Signals)
        {
            var Nd = Signals[0].SamplesCount;
            var N = _AntennaElementLocations.Length;
            var s_data = new double[N, Nd];

            for (var i = 0; i < N; i++)
            {
                var s = Signals[i].Samples;
                for (var j = 0; j < Nd; j++)
                    s_data[i, j] = s[j];
            }
            return new Matrix(s_data);
        }

    }
}
