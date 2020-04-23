using System;

namespace BeamService.Functions
{
    public class RectSignalFunction : AmplitudeSignalFunction
    {
        private double _Period = 1;
        private double _Tau = 5e-10;
        private double _q = 0.5;

        public double Period
        {
            get => _Period;
            set => Set(ref _Period, value);
        }

        public double q
        {
            get => _q;
            set
            {
                if (Set(ref _q, value))
                    OnPropertyChanged(nameof(Tau));
            }
        }

        public double Tau => _Period * _q;

        public RectSignalFunction() { }

        public RectSignalFunction(double Period, double q)
        {
            _Period = Period;
            _q = q;
        }

        public override double Value(double t)
        {
            var period = _Period * 1e-9;
            var tau = Tau * 1e-9;
            t = t % period + (t < 0 ? period : 0);
            return Amplitude * ((t.Equals(tau) || t.Equals(0d) ? 0.5 : 0 < t && t < tau ? 1 : 0) - 0.5);
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

        public RadioSignalFunction(double Period, double q, double f0) : base(Period, q) => _f0 = f0;

        public override double Value(double t) => (base.Value(t) + 0.5) * Math.Sin(2 * Math.PI * t * _f0 * 1e9);
    }
}