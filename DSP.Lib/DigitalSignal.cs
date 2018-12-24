using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;

namespace DSP.Lib
{
    public class DigitalSignal : IEnumerable<double>, IEquatable<DigitalSignal>
    {
        private double _dt;

        private double[] _Samples;

        public double dt => _dt;

        public double fd => 1 / _dt;

        public double TimeLength => _dt * _Samples.Length;

        public int SamplesCount => _Samples.Length;

        public double[] Samples => _Samples;

        public double this[int index]
        {
            get => _Samples[index];
            set => _Samples[index] = value;
        }

        public DigitalSignal(double dt, double[] Samples)
        {
            _dt = dt;
            _Samples = Samples;
        }

        public DigitalSignal(double dt, IEnumerable<double> samples) : this(dt, samples.ToArray()) { }

        public DigitalSignal(double dt, int SamplesCount, [NotNull] Func<double, double> f)
            : this(dt, f.Sample(dt, SamplesCount))
        {

        }

        public double GetTotalPower() => _Samples.GetTotalPower();

        public Complex[] GetSpectrum()
        {
            var N = SamplesCount;
            var spectrum_samples = new Complex[N];
            var w = -2 * Math.PI / N;

            var j2pi_N = Complex.ImaginaryOne * 2 * Math.PI / N;
            for (var m = 0; m < N; m++)
            {
                Complex sample = default;
                for (var n = 0; n < N; n++)
                    sample += _Samples[n] / N * Complex.Exp(-j2pi_N * m * n);
                if (sample.Magnitude < 1e-10) sample = default;
                spectrum_samples[m] = sample;
            }

            return spectrum_samples;
        }

        public Complex[] FFT()
        {
            //TODO:Реализовать бабочку!
            throw new NotImplementedException();
        }

        #region Overrides of Object

        public override string ToString() => $"signal(samples:{_Samples.Length}; dt:{_dt}); power:{GetTotalPower()}";

        public override bool Equals(object obj) => base.Equals(obj);


        public bool Equals(DigitalSignal other) => _dt.Equals(other._dt) && Equals(_Samples, other._Samples);

        public override int GetHashCode()
        {
            unchecked
            {
                return (_dt.GetHashCode() * 397) ^ (_Samples != null ? _Samples.GetHashCode() : 0);
            }
        }

        #endregion

        #region IEnumerator<double>

        IEnumerator<double> IEnumerable<double>.GetEnumerator() => ((IEnumerable<double>)_Samples).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _Samples.GetEnumerator();

        #endregion

        public static implicit operator double[] (DigitalSignal siagnal) => (double[])siagnal._Samples.Clone();

        public static DigitalSignal operator +([NotNull] DigitalSignal s1, [NotNull] DigitalSignal s2)
        {
            if (s1 is null) throw new ArgumentNullException(nameof(s1));
            if (s2 is null) throw new ArgumentNullException(nameof(s2));
            if (s1.dt != s2.dt) throw new InvalidOperationException();
            if (s1.SamplesCount != s2.SamplesCount) throw new InvalidOperationException();

            var samples = new double[s1.SamplesCount];
            for (var i = 0; i < s2.Samples.Length; i++)
                samples[i] = s1.Samples[i] + s2.Samples[i];

            return new DigitalSignal(s1.dt, samples);
        }

        public static DigitalSignal operator -([NotNull] DigitalSignal s1, [NotNull] DigitalSignal s2)
        {
            if (s1 is null) throw new ArgumentNullException(nameof(s1));
            if (s2 is null) throw new ArgumentNullException(nameof(s2));
            if (s1.dt != s2.dt) throw new InvalidOperationException();
            if (s1.SamplesCount != s2.SamplesCount) throw new InvalidOperationException();

            var samples = new double[s1.SamplesCount];
            for (var i = 0; i < s2.Samples.Length; i++)
                samples[i] = s1.Samples[i] - s2.Samples[i];

            return new DigitalSignal(s1.dt, samples);
        }

        public static DigitalSignal operator *([NotNull] DigitalSignal s1, double k)
        {
            if (s1 is null) throw new ArgumentNullException(nameof(s1));

            var samples = new double[s1.SamplesCount];
            for (var i = 0; i < s1.Samples.Length; i++)
                samples[i] = s1.Samples[i] * k;

            return new DigitalSignal(s1.dt, samples);
        }

        public static DigitalSignal operator *(double k, [NotNull] DigitalSignal s1)
        {
            if (s1 is null) throw new ArgumentNullException(nameof(s1));

            var samples = new double[s1.SamplesCount];
            for (var i = 0; i < s1.Samples.Length; i++)
                samples[i] = s1.Samples[i] * k;

            return new DigitalSignal(s1.dt, samples);
        }

        public static DigitalSignal operator +([NotNull] DigitalSignal s1, double k)
        {
            if (s1 is null) throw new ArgumentNullException(nameof(s1));

            var samples = new double[s1.SamplesCount];
            for (var i = 0; i < s1.Samples.Length; i++)
                samples[i] = s1.Samples[i] + k;

            return new DigitalSignal(s1.dt, samples);
        }

        public static DigitalSignal operator +(double k, [NotNull] DigitalSignal s1)
        {
            if (s1 is null) throw new ArgumentNullException(nameof(s1));

            var samples = new double[s1.SamplesCount];
            for (var i = 0; i < s1.Samples.Length; i++)
                samples[i] = s1.Samples[i] + k;

            return new DigitalSignal(s1.dt, samples);
        }

        public static DigitalSignal operator -([NotNull] DigitalSignal s1, double k)
        {
            if (s1 is null) throw new ArgumentNullException(nameof(s1));

            var samples = new double[s1.SamplesCount];
            for (var i = 0; i < s1.Samples.Length; i++)
                samples[i] = s1.Samples[i] - k;

            return new DigitalSignal(s1.dt, samples);
        }

        public static DigitalSignal operator -(double k, [NotNull] DigitalSignal s1)
        {
            if (s1 is null) throw new ArgumentNullException(nameof(s1));

            var samples = new double[s1.SamplesCount];
            for (var i = 0; i < s1.Samples.Length; i++)
                samples[i] = k - s1.Samples[i];

            return new DigitalSignal(s1.dt, samples);
        }

        public static DigitalSignal operator /([NotNull] DigitalSignal s1, double k)
        {
            if (s1 is null) throw new ArgumentNullException(nameof(s1));

            var samples = new double[s1.SamplesCount];
            for (var i = 0; i < s1.Samples.Length; i++)
                samples[i] = s1.Samples[i] / k;

            return new DigitalSignal(s1.dt, samples);
        }

        public static DigitalSignal operator /(double k, [NotNull] DigitalSignal s1)
        {
            if (s1 is null) throw new ArgumentNullException(nameof(s1));

            var samples = new double[s1.SamplesCount];
            for (var i = 0; i < s1.Samples.Length; i++)
                samples[i] = k / s1.Samples[i];

            return new DigitalSignal(s1.dt, samples);
        }
    }
}
