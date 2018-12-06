using System;
using System.Linq;
using DSP.Lib;

namespace DSP.TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const double f0 = 1e3;

            var s = Enumerable.Repeat(1d, 1000).ToArray();
            var d = new double[1000]; d[0] = 1;

            var s1 = new DigitalSignal(t => Math.Sin(2 * Math.PI * t * 0.5e3), 0.5e-4, 1000);
            var s2 = new DigitalSignal(t => Math.Sin(2 * Math.PI * t * 1e3), 0.5e-4, 1000);


           

            Console.ReadLine();
        }
    }
}
