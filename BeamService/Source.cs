using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamService
{
    public class Source
    {
        private readonly Func<double, double> f_F;

        public double this[double t] => f_F(t);

        public Source(Func<double, double> f)
        {
            f_F = f;
        }
    }

    public class ADC
    {
        public double D { get; }
        public int N { get; }
        public double Fd { get; }
        public double MaxValue { get; }

        /// <summary>
        /// Инициализация нового АЦП
        /// </summary>
        /// <param name="n">Число разрядов кода</param>
        /// <param name="fd">Частота дискретизации</param>
        /// <param name="MaxValue">Максимальная амплитуда сигнала</param>
        public ADC(int n, double fd, double MaxValue)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n), "Радрядность кода АЦП должна быть больше 0");
            if (fd <= 0)
                throw new ArgumentOutOfRangeException(nameof(fd), "Частота дискретизации должна быть больше 0");

            N = n;
            D = MaxValue / ((2 << N) - 1);
            Fd = fd;
            this.MaxValue = MaxValue;
        }

        /// <summary>
        /// Продискретизировать источник
        /// </summary>
        /// <param name="src">Дискретизируемый источник сигнала</param>
        /// <param name="Count">Число отсчётов сигнала, которые надо получить</param>
        /// <returns>Массив значений отсчётов сигнала</returns>
        public double[] GetDiscretSignal(Source src, int Count)
        {
            var result = new double[Count];
            var dt = 1 / Fd;
            for (int i = 0; i < Count; i++)
                result[i] = Quant(src[i * dt]);
            return result;   
        }

        private double threshold(double x)
        {
            if (Math.Abs(x) < MaxValue / 2) return x;
            return MaxValue / 2 * Math.Sign(x);
        }

        private double Quant(double x)
        {
            return threshold(Math.Round(x / D) * D);
        }
    }
}
