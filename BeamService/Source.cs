using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamService
{
    /// <summary>Источник сигнала для АЦП</summary>
    public class Source
    {
        private readonly Func<double, double> f_F;

        public double this[double t] => f_F(t);

        public Source(Func<double, double> f) => f_F = f;

        public static Source operator +(Source a, Source b)
        {
            if (a == null) return b;
            if (b == null) return a;
            return new Source(t => a.f_F(t) + b.f_F(t));
        }
    }

    /// <summary>АЦП</summary>
    public class ADC
    {
        private readonly Random f_Random;

        /// <summary>
        /// Динамический диапазон
        /// </summary>
        public double D { get; }
        /// <summary>
        /// Число разрядов кода
        /// </summary>
        public int N { get; }
        /// <summary>Частота дискретизации данного АЦП</summary>
        public double Fd { get; }
        /// <summary>
        /// Период дискретизации
        /// </summary>
        public double dt => 1 / Fd;
        /// <summary>Максимальная амплитуда аналогового сигнала, которую способен обработать АЦП</summary>
        public double MaxValue { get; }
        /// <summary>Величина джиттера в секкундах</summary>
        public double tj { get; }

        /// <summary>
        /// Инициализация нового АЦП
        /// </summary>
        /// <param name="n">Число разрядов кода</param>
        /// <param name="fd">Частота дискретизации</param>
        /// <param name="MaxValue">Максимальная амплитуда сигнала</param>
        public ADC(int n, double fd, double MaxValue, double tj = 0d)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n), "Радрядность кода АЦП должна быть больше 0");
            if (fd <= 0)
                throw new ArgumentOutOfRangeException(nameof(fd), "Частота дискретизации должна быть больше 0");

            N = n;
            D = MaxValue / ((2 << N) - 1);
            Fd = fd;
            this.MaxValue = MaxValue;
            this.tj = tj;

            f_Random = new Random((int)DateTime.Now.Ticks);
        }

        /// <summary>
        /// Продискретизировать источник
        /// </summary>
        /// <param name="src">Дискретизируемый источник сигнала</param>
        /// <param name="Count">Число отсчётов сигнала, которые надо получить</param>
        /// <returns>Массив значений отсчётов сигнала</returns>
        public double[] GetDiscretSignalValues(Source src, int Count)
        {
            var result = new double[Count];
            var dt = 1 / Fd;
            for (int i = 0; i < Count; i++)
            {
                var tj = (f_Random.NextDouble() - 0.5) * this.tj;
                var t = i * dt + tj;
                result[i] = Quant(src[t]);
            }
            return result;
        }

        /// <summary>
        /// Продискретизировать источник
        /// </summary>
        /// <param name="src">Дискретизируемый источник сигнала</param>
        /// <param name="Count">Число отсчётов сигнала, которые надо получить</param>
        /// <returns>Массив значений отсчётов сигнала</returns>
        public SignalValue[] GetDiscretSignal(Source src, int Count)
        {
            var result = new SignalValue[Count];
            var dt = 1 / Fd;
            for (int i = 0; i < Count; i++)
            {
                var tj = (f_Random.NextDouble() - 0.5) * this.tj;
                var t = i * dt + tj;
                result[i] = new SignalValue { t = t, V = Quant(src[t]) };
            }
            return result;
        }

        private double threshold(double x)
        {
            if (Math.Abs(x) < MaxValue / 2) return x;
            return MaxValue / 2 * Math.Sign(x);
        }

        private double Quant(double x) => threshold(Math.Round(x / D) * D);
    }
}

public struct SignalValue
{
    public double t { get; set; }
    public double V { get; set; }
}