using System;
using System.Linq;
using DSP.Lib;

namespace DSP.TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const double f0 = 20;
            const double DeltaF = 10;
            const double fd = 100;
            const double dt = 1 / fd;

            var s = Enumerable.Repeat(1d, 1000).ToArray();
            var d = new double[1000]; d[0] = 1;

            var s1 = new DigitalSignal(t => Math.Sin(2 * Math.PI * t * 20), dt, 1000);
            var s2 = new DigitalSignal(t => Math.Sin(2 * Math.PI * t * 30), dt, 1000);
            var s3 = new DigitalSignal(t => Math.Sin(2 * Math.PI * t * 40), dt, 1000);

            //var iir = new IIR(new [] { 1, - 0.7 }, new [] { 0.3, 0 } );

            //var impulse_response_iir = iir.GetImpulseResponse(3);

            var rlc = new BandPassRLC(f0, DeltaF, dt);

            var impulse_response = rlc.GetImpulseResponse(128);


            Console.ReadLine();
        }
    }
}
