using System;
using System.Numerics;
using DSP.Lib.Service;

namespace DSP.TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Complex[] z = 
            {
                new Complex(-15.70796326794, -60.8366801296),
                new Complex(-15.70796326794, +60.8366801296),
            };

            var a = Polynom.GetCoefficients(z);

            //Array.Reverse(a);

            //var p = new Polynom(a);

            //var y = p.GetValue(1/2d);

            Console.ReadLine();
        }
    }
}
