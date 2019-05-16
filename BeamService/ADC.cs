using System;
using DSP.Lib;
using MathService.ViewModels;
// ReSharper disable InconsistentNaming

namespace BeamService
{
    /// <summary>АЦП</summary>
    public class ADC : ViewModel
    {
        private static readonly Random __Random = new Random((int)DateTime.Now.Ticks);

        /// <summary>Динамический диапазон</summary>
        public double D => MaxValue / ((1 << N) - 1);

        private int _N;

        /// <summary>
        /// Число разрядов кода
        /// </summary>
        public int N
        {
            get => _N;
            set => Set(ref _N, value);
        }

        private double _fd;

        /// <summary>Частота дискретизации данного АЦП</summary>
        public double Fd
        {
            get => _fd;
            set
            {
                if(Set(ref _fd, value))
                    OnPropertyChanged(nameof(dt));
            }
        }

        /// <summary>
        /// Период дискретизации
        /// </summary>
        public double dt => 1 / Fd;

        private double _MaxValue;

        /// <summary>Максимальная амплитуда аналогового сигнала, которую способен обработать АЦП</summary>
        public double MaxValue
        {
            get => _MaxValue;
            set => Set(ref _MaxValue, value);
        }

        private double _tj;

        /// <summary>Величина джиттера в секкундах</summary>
        public double tj
        {
            get => _tj;
            set => Set(ref _tj, value);
        }

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
                var tj = (__Random.NextDouble() - 0.5) * this.tj * dt;
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
                var tj = (__Random.NextDouble() - 0.5) * this.tj;
                var t = i * dt + tj;
                result[i] = new SignalValue { t = t, V = Quant(src[t]) };
            }
            return result;
        }

        public DigitalSignal GetDigitalSignal(AnalogSignalSource src, int Count)
        {
            var samples = new double[Count];
            var dt = 1 / Fd;
            for (var i = 0; i < Count; i++)
            {
                var tj = (__Random.NextDouble() - 0.5) * this.tj;
                var t = i * dt + tj;
                samples[i] = Quant(src[t]);
            }
            return new DigitalSignal(dt, samples);
        }

        private double threshold(double x)
        {
            if (Math.Abs(x) < MaxValue / 2) return x;
            return MaxValue / 2 * Math.Sign(x);
        }

        private double Quant(double x) => threshold(Math.Round(x / D) * D);
    }
}
