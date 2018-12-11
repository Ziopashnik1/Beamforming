using System;
using System.Linq;
using System.Numerics;
using DSP.Lib.Service;

namespace DSP.TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Complex[] P =
            {
                  new Complex(-0.5, + 0.866),
                  new Complex(-1, 0),
                  new Complex(-0.5, - 0.866),
            };

            const double fd = 2;

            Complex z(Complex p) => (2 * fd + p) / (2 * fd - p);

            Complex[] Z = P.Select(p => z(p)).ToArray();

            var a = Polynom.GetCoefficients(Z).Select(s => s.Real).ToArray();

            var b = Polynom.GetCoefficients(Enumerable.Repeat(-1d, 3).ToArray());

            var K = a.Sum() / b.Sum();

            Console.ReadLine();
        }
    }
}
