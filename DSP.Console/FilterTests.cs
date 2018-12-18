using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSP.Lib;

namespace DSP.TestConsole
{
    static class FilterTests
    {
        public static void Run()
        {
            const double f0 = 20;
            const double DeltaF = 10;
            const double fd = 100;
            const double dt = 1 / fd;

            var s = Enumerable.Repeat(1d, 1000).ToArray();
            var d = new double[1000]; d[0] = 1;

            var A0 = Math.Sqrt(2);
            var s0 = new DigitalSignal(t => 1 * Math.Cos(2 * Math.PI * t * 0), dt, 1000);
            var s1 = new DigitalSignal(t => A0 * Math.Cos(2 * Math.PI * t * f0), dt, 1000);
            var s2 = new DigitalSignal(t => A0 * Math.Cos(2 * Math.PI * t * 30), dt, 1000);
            var s3 = new DigitalSignal(t => A0 * Math.Cos(2 * Math.PI * t * 40), dt, 1000);

            //var iir = new IIR(new [] { 1, - 0.7 }, new [] { 0.3, 0 } );

            //var impulse_response_iir = iir.GetImpulseResponse(3);

            //var rlc = new BandPassRLC(f0, DeltaF, dt);

            //var impulse_response = rlc.GetImpulseResponse(128);

            var rc = new HighPassRC(1 / f0, dt);

            var c_0 = rc.GetTransmissionCoefficient(0, dt);
            var c_f0 = rc.GetTransmissionCoefficient(f0, dt);
            var c_fd_2 = rc.GetTransmissionCoefficient(fd / 2, dt);

            var abs_0 = c_0.Magnitude;
            var abs_f0 = c_f0.Magnitude;
            var abs_fd_2 = c_fd_2.Magnitude;
            var phase_0 = c_0.Phase * 180 / Math.PI;
            var phase_f0 = c_f0.Phase * 180 / Math.PI;
            var phase_fd_2 = c_fd_2.Phase * 180 / Math.PI;

            var y0 = rc.Filter(s0);
            rc.Reset();
            var y1 = rc.Filter(s1);
            rc.Reset();
            var y3 = rc.Filter(s3);
        }
    }
}
