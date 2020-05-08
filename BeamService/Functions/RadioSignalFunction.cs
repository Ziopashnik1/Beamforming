using System;

namespace BeamService.Functions
{
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

        public override double Value(double t) => (base.Value(t) + 0.5 * Amplitude) * Math.Sin(2 * Math.PI * t * _f0 * 1e9);
    }
}