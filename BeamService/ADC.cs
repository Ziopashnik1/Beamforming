using System;

namespace BeamService
{
    /// <summary>АЦП</summary>
    public class ADC
    {
        private static readonly Random sf_Random = new Random((int)DateTime.Now.Ticks);

        /// <summary>Динамический диапазон</summary>
        public double D => MaxValue / ((1 << N) - 1);
        /// <summary>
        /// Число разрядов кода
        /// </summary>
        public int N { get; set; }
        /// <summary>Частота дискретизации данного АЦП</summary>
        public double Fd { get; set; }
        /// <summary>
        /// Период дискретизации
        /// </summary>
        public double dt => 1 / Fd;
        /// <summary>Максимальная амплитуда аналогового сигнала, которую способен обработать АЦП</summary>
        public double MaxValue { get; set; }
        /// <summary>Величина джиттера в секкундах</summary>
        public double tj { get; set; }

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
            Fd = fd;
            this.MaxValue = MaxValue;
            this.tj = tj;
        }

        /// <summary>
        /// Продискретизировать источник
        /// </summary>
        /// <param name="src">Дискретизируемый источник сигнала</param>
        /// <param name="Count">Число отсчётов сигнала, которые надо получить</param>
        /// <returns>Массив значений отсчётов сигнала</returns>
        public double[] GetDiscretSignalValues(AnalogSignalSource src, int Count)
        {
            var result = new double[Count];
            var dt = 1 / Fd;
            for (int i = 0; i < Count; i++)
            {
                var tj = (sf_Random.NextDouble() - 0.5) * this.tj * dt;
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
        public SignalValue[] GetDiscretSignal(AnalogSignalSource src, int Count)
        {
            var result = new SignalValue[Count];
            var dt = 1 / Fd;
            for (int i = 0; i < Count; i++)
            {
                var tj = (sf_Random.NextDouble() - 0.5) * this.tj;
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
