using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MathService;

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

        public override string ToString() => $"Отсчётов {Count}, мощность {Power}, амплитуда {Max - Min}";

        public DigitalSpectrum GetSpectrum()
        {
            var N = f_Samples.Length;
            var spectrum_samples = new Complex[N];
            var j2pi_N = 2 * Math.PI/N;
            for (var m = 0; m < N; m++)
            {
                Complex sample = default;
                for (var n = 0; n < N; n++)
                    sample += f_Samples[n].V / N * Complex.Exp(-j2pi_N * m * n);
                if (sample.Abs < 1e-10) sample = default;
                spectrum_samples[m] = sample;
            }
            return new DigitalSpectrum(spectrum_samples, 1 / (N * f_dt));
        }
    }
}
