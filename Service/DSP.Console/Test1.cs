using System;
using System.Linq;
using DSP.Lib;

namespace DSP.TestConsole
{
    class Test1
    {
        public static void Run()
        {
            var s = Enumerable.Repeat(1d, 1000).ToArray();
            var s1 = new DigitalSignal(0.5e-4, 1000, t => Math.Sin(2 * Math.PI * t * 0.5e3));
            var s2 = new DigitalSignal(0.5e-4, 1000, t => Math.Sin(2 * Math.PI * t * 1e3));


            var iir = new IIR(
                a: new[] { 1, -0.951229 },
                b: new[] { 0, 0.048771 });

            var rc = new ExponentialLowPassRC(1e-3, 0.5e-4);

            //var pulse_length = rc.GetImpulseResponse().Count();

            var pulse = rc.Filter(s).ToArray();
            rc.Reset();
            var y1 = rc.Filter(s1).ToArray();
            rc.Reset();
            var y2 = rc.Filter(s2).ToArray();

            var s1_power = s1.GetTotalPower();
            var s2_power = s2.GetTotalPower();

            var y1_power = y1.GetTotalPower();
            var y2_power = y2.GetTotalPower();

            var k1 = s1_power / y1_power;
            var k2 = s2_power / y2_power;
        }
    }
}
