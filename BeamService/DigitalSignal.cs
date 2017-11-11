using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamService
{
    public class DigitalSignal : IEnumerable<Sample>
    {
        private readonly Sample[] f_Samples;

        public int Count => f_Samples.Length;
        public double StartTime => f_Samples.Min(s => s.t);
        public double EndTime => f_Samples.Max(s => s.t);
        public double Min => f_Samples.Min(s => s.V);
        public double Max => f_Samples.Max(s => s.V);
        public double Power => f_Samples.Sum(s => s.V * s.V) / Count;
        public Sample this[int i] => f_Samples[i];

        public DigitalSignal(double[] samples, double dt = 1, double t0 = 0)
        {
            if (samples == null) throw new ArgumentNullException(nameof(samples));

            var N = samples.Length;
            f_Samples = new Sample[N];
            for (var i = 0; i < N; i++)
                f_Samples[i] = new Sample(t0 + dt * i, samples[i]);
        }

        public IEnumerator<Sample> GetEnumerator() => ((IEnumerable<Sample>)f_Samples).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Sample>)f_Samples).GetEnumerator();

        public override string ToString()
        {
            return $"Тсчётов {Count}, мощность {Power}, амплитуда {Max - Min}";
        }
    }

    /// <summary>
    /// Отсчёт цифрового сигнала
    /// </summary>
    public struct Sample
    {
        /// <summary>
        /// Время
        /// </summary>
        public readonly double t;
        /// <summary>
        /// Амплитуда
        /// </summary>
        public readonly double V;
        /// <summary>
        /// Время
        /// </summary>
        public double Time => t;
        /// <summary>
        /// Время в нс
        /// </summary>
        public double Time_ns => t * 1e9;
        /// <summary>
        /// Амплитуда
        /// </summary>
        public double Value => V;
        /// <summary>
        /// Новое значение отсчта цифрового сигнала
        /// </summary>
        /// <param name="t">Время</param>
        /// <param name="v">Амплитуда</param>
        public Sample(double t, double v)
        {
            this.t = t;
            V = v;
        }

        public override string ToString() => $"{t}:{V}";
    }
}
