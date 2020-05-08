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
            t %= period;
            if (t < 0) t += period;
            //return Amplitude * ((t.Equals(tau) || t.Equals(0d) ? 0.5 : 0 < t && t < tau ? 1 : 0) - 0.5);
            if (t.Equals(tau) || t.Equals(0d)) return 0;
            if (t < tau) return Amplitude * 0.5;
            return -Amplitude * 0.5;
        }
    }
}