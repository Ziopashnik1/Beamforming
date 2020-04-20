using System;

namespace BeamService
{
    /// <summary>Источник сигнала для АЦП</summary>
    public class AnalogSignalSource
    {
        protected readonly Func<double, double> f_F;

        public double this[double t] => f_F(t);

        public AnalogSignalSource(Func<double, double> f) => f_F = f;

        public static AnalogSignalSource operator +(AnalogSignalSource a, AnalogSignalSource b) => a is null ? b : (b is null ? a : new AnalogSignalSource(t => a.f_F(t) + b.f_F(t)));
    }
}
