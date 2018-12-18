using System;

namespace DSP.Lib
{
    public class LowPassRC : IIR
    {
        public LowPassRC(double tau, double dt)
            : this(1 / Math.Tan(Math.PI * dt / tau))
        {

        }

        private LowPassRC(double w0)
            : base(
                b: new[] { 1 / (1 + w0), 1 / (1 + w0) },
                a: new[] { 1, (1 - w0) / (1 + w0) }
            )
        {

        }
    }
}