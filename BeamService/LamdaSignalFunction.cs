using System;

namespace BeamService
{
    public class LamdaSignalFunction : SignalFunction
    {
        private readonly Func<double, double> f_Function;
        public LamdaSignalFunction(Func<double, double> Function) => f_Function = Function;

        public override double Value(double t) => f_Function(t);
    }
}