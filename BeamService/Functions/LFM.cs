using System;

namespace BeamService.Functions
{
    /// <summary>ЛЧМ</summary>
    public class LFM : HarmonicSignalFunction
    {
        private double _Period;

        private double _MaxFrequency;

        public double Period
        {
            get => _Period;
            set => Set(ref _Period, value);
        }

        public double MaxFrequency
        {
            get => _MaxFrequency;
            set => Set(ref _MaxFrequency, value);
        }

        public LFM(double Frequency, double MaxFrequency, double Period, double Phase, double Amplitude = 1)
            : base(Amplitude, Frequency, Phase)
        {
            _MaxFrequency = MaxFrequency;
            _Period = Period;
        }

        #region Overrides of SignalFunction

        public override double Value(double t)
        {
            double f(double tau) => _Frequency + tau * (_MaxFrequency - _Frequency) / _Period;

            var t0 = t % _Period;
            if (t < 0) t0 += _Period;

            return _Amplitude * Math.Sin(2 * Math.PI * f(t0) * t0 + _Phase);
        }

        #endregion
    }
}
