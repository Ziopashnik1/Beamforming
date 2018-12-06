using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace DSP.Lib
{
    public static class DoubleArrayExtensions
    {
        public static double GetTotalPower([NotNull] this double[] samples)
        {
            if (samples is null) throw new ArgumentNullException(nameof(samples));
            if (samples.Length == 0) return double.NaN;
            return samples.Sum(S => S * S) / samples.Length;
        }

        public static Complex[] GetSpectrum([NotNull] this double[] signal)
        {
            if (signal is null) throw new ArgumentNullException(nameof(signal));
            var N = signal.Length;
            if(N == 0) return new Complex[0];

            var spectrum = new Complex[N];
            var w = -2 * Math.PI / N;
            for (var m = 0; m < N; m++)
            {
                var re = 0d;
                var im = 0d;
                var wm = w * m;
                for (var n = 0; n < N; n++)
                {
                    re += signal[n] * Math.Cos(wm * n);
                    im += signal[n] * Math.Sin(wm * n);
                }
                spectrum[m] = new Complex(re / N, im / N);
            }
            return spectrum;
        }

        public static double[] GetRealSignal([NotNull] this Complex[] spectrum)
        {
            if (spectrum is null) throw new ArgumentNullException(nameof(spectrum));
            var N = spectrum.Length;
            if (N == 0) return new double[0];

            var signal = new double[N];

            var w = 2 * Math.PI / N;
            for (var n = 0; n < spectrum.Length; n++)
            {
                var re = 0d;
                var wn = w * n;
                for (var m = 0; m < N; m++)
                    re += spectrum[m].Real * Math.Cos(wn * m) - spectrum[m].Imaginary * Math.Sin(wn * m);
                signal[n] = re;
            }

            return signal;
        }

        public static Complex[] GetComplexSignal([NotNull] this Complex[] spectrum)
        {
            if (spectrum is null) throw new ArgumentNullException(nameof(spectrum));
            var N = spectrum.Length;
            if (N == 0) return new Complex[0];

            var signal = new Complex[N];

            var w = 2 * Math.PI / N;
            for (var n = 0; n < spectrum.Length; n++)
            {
                var re = 0d;
                var im = 0d;
                var wn = w * n;
                for (var m = 0; m < N; m++)
                {
                    var cos = Math.Cos(wn * m);
                    var sin = Math.Sin(wn * m);
                    re += spectrum[m].Real * cos - spectrum[m].Imaginary * sin;
                    im += spectrum[m].Real * sin + spectrum[m].Imaginary * cos;
                }
                signal[n] = new Complex(re, im);
            }

            return signal;
        }

        public static double[] Multiply(this double[] array, double k)
        {
            var result = new double[array.Length];
            for (var i = 0; i < array.Length; i++)
                result[i] = array[i] * k;
            return result;
        }
    }
}
