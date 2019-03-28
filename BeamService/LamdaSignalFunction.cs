using System;

namespace BeamService
{
    public class LamdaSignalFunction : SignalFunction
    {
        private readonly Func<double, double> _Function;
        public LamdaSignalFunction(Func<double, double> Function) => _Function = Function;

        public override double Value(double t) => _Function(t);
    }
}