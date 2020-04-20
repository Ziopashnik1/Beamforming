using System;

namespace BeamService.Functions
{
    public class RectSignalFunction : AmplitudeSignalFunction
    {
        private double _Period = 1e-9;
        private double _Tau = 5e-10;

        public double Period
        {
            get => _Period;
            set => Set(ref _Period, value);
        }

        public double Tau
        {
            get => _Tau;
            set => Set(ref _Tau, value);
        }

        public RectSignalFunction() { }

        public RectSignalFunction(double Tau, double Period)
        {
            _Period = Period;
            _Tau = Tau;
        }

        public override double Value(double t)
        {
            t = t % Period + (t < 0 ? Period : 0);
            return Amplitude * ((t.Equals(Tau) || t.Equals(0d) ? 0.5 : 0 < t && t < Tau ? 1 : 0) - 0.5);
        }
    }

    public class RadioSignalFunction : RectSignalFunction
    {
        private double _f0 = 5e9;

        public double f0
        {
            get => _f0;
            set => Set(ref _f0, value);
        }

        public RadioSignalFunction() { }

        public RadioSignalFunction(double Tau, double Period, double f0) : base(Tau, Period) => _f0 = f0;

        #region Overrides of RectSignalFunction

        public override double Value(double t) => base.Value(t) * Math.Sin(2 * Math.PI * t * _f0);

        #endregion
    }
}