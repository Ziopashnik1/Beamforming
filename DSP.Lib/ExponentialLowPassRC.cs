using System;

namespace DSP.Lib
{
    public class ExponentialLowPassRC : IIR
    {
        public ExponentialLowPassRC(double tau, double dt)
            : base(
                a: new[] { 1, -Math.Exp(2 * Math.PI * -dt / tau) },
                b: new[] { 0, 1 - Math.Exp(2 * Math.PI * -dt / tau) })
        {

        }
    }
}
