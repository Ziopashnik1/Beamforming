using System;
using System.Numerics;
using MathCore.Annotations;

namespace DSP.Lib
{
    public class IIR : DigitalFilter
    {
        /// <summary>Коэффициенты полинома знаменателя</summary>
        [NotNull] private readonly double[] _A;
        /// <summary>Коэффициенты полинома Числителя</summary>
        [NotNull] private readonly double[] _B;
        /// <summary>Вектор состояния фильтра (ячейки линии задержки)</summary>
        [NotNull] private readonly double[] _State;

        /// <summary>Порядок фильтра</summary>
        public int Order => _State.Length - 1;

        /// <summary>
        /// Инициализация нового цифрового фильтра с бесконечной импульсной характеристикой
        /// </summary>
        /// <param name="a">Коэффициенты полинома знаменателя</param>
        /// <param name="b">Коэффициенты полинома числителя</param>
        public IIR([NotNull] double[] a, [NotNull] double[] b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));

            var order = Math.Max(a.Length, b.Length);
            _A = new double[order];
            _B = new double[order];
            _State = new double[order];

            Array.Copy(a, _A, a.Length);
            Array.Copy(b, _B, b.Length);
        }

        public Complex GetTransmissionCoefficient(double f, double dt)
        {
            var w = -2 * Math.PI * f * dt;
            var e = new Complex(Math.Cos(w), Math.Sin(w));

            Complex Sum(double[] V)
            {
                Complex s = V[V.Length-1];
                for (var i = V.Length - 2; i >= 0; i--)
                    s = s * e + V[i];
                return s;
            }

            return Sum(_B) / Sum(_A);
        }

        #region Overrides of DigitalFilter

        /// <summary>
        /// Фильтрация очередного отсчёта входного сигнала
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override double GetSample(double sample)
        {
            var input = 0d;
            var output = 0d;

            for (var i = _State.Length - 1; i >= 1; i--)
            {
                _State[i] = _State[i - 1];
                input += _State[i] * _A[i];
                output += _State[i] * _B[i];
            }                 
            _State[0] = sample - input;

            return output + _State[0] * _B[0];
        }

        public override void Reset() => Array.Clear(_State, 0, _State.Length);

        #endregion
    }
}