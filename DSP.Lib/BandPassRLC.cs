using System;

namespace DSP.Lib
{
    public class BandPassRLC : IIR
    {
        public BandPassRLC(double f0, double DeltaF, double dt)
            : this(
                w0: Math.Tan(Math.PI * f0 * dt),
                dw: 2 * Math.Sin(Math.PI * DeltaF * dt) / (Math.Cos(2 * Math.PI * f0 * dt) + Math.Cos(Math.PI * DeltaF * dt)))
        {

        }

        private BandPassRLC(double w0, double dw)
            : base(
                a: new[] { 1, 2 * (w0 * w0 - 1) / (w0 * w0 + dw + 1), (1 - dw + w0 * w0) / (w0 * w0 + dw + 1) },
                b: new[] { dw / (w0 * w0 + dw + 1), 0, -dw / (w0 * w0 + dw + 1) }
            )
        {

        }
    }
}