using System;
using DSP.Lib;
using MathCore;
using MathCore.Vectors;

namespace BeamService.Digital
{
    /// <summary>Диаграммообразующая схема</summary>
    public abstract class BeamForming
    {
        public abstract (DigitalSignal I, DigitalSignal Q) GetSignal(DigitalSignal[] Signals);
    }

    public class MatrixBeamForming : BeamForming
    {
        private readonly Vector3D[] _AntennaElementLocations;
        private readonly int _SamplesCount;
        private readonly double _fd;
        private readonly MatrixComplex _Wt;
        private readonly MatrixComplex _Wt_inv;
        private MatrixComplex _PhasingMatrix;
        private const double pi2 = Consts.pi2;
        private const double c = Consts.SpeedOfLight;
        private SpaceAngle _PhasingАngle;

        public SpaceAngle PhasingАngle
        {
            get => _PhasingАngle;
            set
            {
                if (_PhasingMatrix != null && _PhasingАngle.InRad == value.InRad) return;
                _PhasingАngle = value;
                _PhasingMatrix = GetPhasingMatrix(value.InRad);
            }
        }

        public MatrixBeamForming(Vector3D[] AntennaElementLocations, int SamplesCount, double fd)
        {
            _AntennaElementLocations = AntennaElementLocations;
            _SamplesCount = SamplesCount;
            _fd = fd;
            _Wt = MatrixComplex.Create(SamplesCount, SamplesCount, (i, j) => Complex.Exp(-pi2 * i * j / SamplesCount) / SamplesCount);
            _Wt_inv = MatrixComplex.Create(SamplesCount, SamplesCount, (i, j) => Complex.Exp(pi2 * i * j / SamplesCount));
            PhasingАngle = new SpaceAngle(0, 0);
        }

        private static MatrixComplex GetPhasingMatrix(int ElementsCount, int SamplesCount, double df, Vector3D[] Locations, SpaceAngle Angle) =>
            MatrixComplex.Create(ElementsCount, SamplesCount, (i, m) =>
            {
                var fm = (m <= SamplesCount / 2 ? m : m - SamplesCount) * df;
                var location = Locations[i];
                return Complex.Exp(pi2 / c * fm * location.GetProjectionTo(Angle));
            });

        public override (DigitalSignal I, DigitalSignal Q) GetSignal(DigitalSignal[] Signals)
        {
            var ss = GetSignalMatrix(Signals);
            var SS = ss * _Wt;
            var QQ = ElementMultiply(SS, _PhasingMatrix);
            var Q = SumRows(QQ);
            var q = Q * _Wt_inv;

            var samples_i = new double[q.M];
            var samples_q = new double[q.M];
            for (var i = 0; i < samples_i.Length; i++)
            {
                samples_i[i] = q[0, i].Re;
                samples_q[i] = q[0, i].Im;
                //if(Math.Abs(q[0, i].Im) > 1e-10)
                //    throw new InvalidOperationException("Ошибка преобразования - получен комплексный результат");
            }
            return (new DigitalSignal(1 / _fd, samples_i), new DigitalSignal(1 / _fd, samples_q));
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

        private static MatrixComplex ElementMultiply(MatrixComplex A, MatrixComplex B)
        {
            if (A.M != B.M) throw new InvalidOperationException("Число столбцов матриц не совпадает");
            if (A.N != B.N) throw new InvalidOperationException("Число строк матриц не совпадает");

            //return A * B;

            var N = A.N;
            var M = B.M;

            var result = new Complex[N, M];
            for (var i = 0; i < N; i++)
                for (var j = 0; j < M; j++)
                    result[i, j] = A[i, j] * B[i, j];
            return new MatrixComplex(result);
        }

        /// <summary>
        /// Создание фазирующей матрицы
        /// </summary>
        /// <returns></returns>
        private MatrixComplex GetPhasingMatrix(SpaceAngle angle)
        {
            var matrix = new Complex[_AntennaElementLocations.Length, _SamplesCount];
            var df = _fd / _SamplesCount;

            for (var sample = 0; sample < _SamplesCount; sample++)
            {
                //var f = (sample <= _SamplesCount / 2 ? sample : sample - _SamplesCount) * df;
                var f = df;
                if (sample <= _SamplesCount / 2)
                    f *= sample;
                else
                    f *= sample - _SamplesCount;

                for (var element = 0; element < _AntennaElementLocations.Length; element++)
                {
                    var location = _AntennaElementLocations[element];
                    var projection = location.GetProjectionTo(angle);

                    matrix[element, sample] = Complex.Exp(f * pi2 / c * projection);
                }
            }

            return new MatrixComplex(matrix);
        }

        /// <summary>
        /// Получение строки усиленного сигнала 
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        private static MatrixComplex SumRows(MatrixComplex A)
        {
            var result = new Complex[1, A.M];
            for (var j = 0; j < A.M; j++)
            {
                var summ = new Complex();
                for (var i = 0; i < A.N; i++)
                    summ += A[i, j];
                result[0, j] = summ;
            }
            return new MatrixComplex(result);
        }
    }
}
