using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BeamService
{
    public class DigitalSignal : IEnumerable<Sample>
    {
        private readonly Sample[] f_Samples;
        private readonly double f_dt;
        private readonly double f_t0;

        public int Count => f_Samples.Length;
        public double StartTime => f_Samples.Min(s => s.t);
        public double EndTime => f_Samples.Max(s => s.t);
        public double Min => f_Samples.Min(s => s.V);
        public double Max => f_Samples.Max(s => s.V);
        public double Power => f_Samples.Sum(s => s.V * s.V) / Count;
        public Sample this[int i] => f_Samples[i];

        public DigitalSpectrum Spectrum => GetSpectrum();

        public DigitalSignal(double[] samples, double dt = 1, double t0 = 0)
        {
            if (samples == null) throw new ArgumentNullException(nameof(samples));
            f_dt = dt;
            f_t0 = t0;
            var N = samples.Length;
            f_Samples = new Sample[N];
            for (var i = 0; i < N; i++)
                f_Samples[i] = new Sample(t0 + dt * i, samples[i]);
        }

        public IEnumerator<Sample> GetEnumerator() => ((IEnumerable<Sample>)f_Samples).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Sample>)f_Samples).GetEnumerator();

        public override string ToString() => $"Тсчётов {Count}, мощность {Power}, амплитуда {Max - Min}";

        public DigitalSpectrum GetSpectrum()
        {
            var N = f_Samples.Length;
            var spectrum_samples = new Complex[N];
            var j2pi_N = Complex.ImaginaryOne * 2 * Math.PI/N;
            for (var m = 0; m < N; m++)
            {
                Complex sample = default(Complex);
                for (var n = 0; n < N; n++)
                    sample += f_Samples[n].V / N * Complex.Exp(-j2pi_N * m * n);
                if (sample.Magnitude < 1e-10) sample = default(Complex);
                spectrum_samples[m] = sample;
            }
            return new DigitalSpectrum(spectrum_samples, 1 / (N * f_dt));
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

    public class DigitalSpectrum : IEnumerable<SpectrumSample>
    {
        private readonly SpectrumSample[] f_Samples;

        public DigitalSpectrum(Complex[] samples, double df, double f0 = 0)
        {
            df /= 1e9;
            f_Samples = new SpectrumSample[samples.Length];
            for (var i = 0; i < f_Samples.Length; i++)
                f_Samples[i] = new SpectrumSample(df * i + f0, samples[i]);
        }

        public IEnumerator<SpectrumSample> GetEnumerator() => ((IEnumerable<SpectrumSample>)f_Samples).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<SpectrumSample>)f_Samples).GetEnumerator();
    }

    public struct SpectrumSample
    {
        public readonly Complex value;
        public readonly double f;

        public double Abs => value.Magnitude;
        public double Arg => value.Phase;
        public double Frequency => f;

        public SpectrumSample(double f, Complex value)
        {
            this.f = f;
            this.value = value;
        }
    }
}
