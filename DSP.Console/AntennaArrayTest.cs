using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antennas;

namespace DSP.TestConsole
{
    internal static class AntennaArrayTest
    {
        public static void Test()
        {
            var array = new LinearAntennaArray(Enumerable.Range(0, 16).Select(i => new Vibrator(0.15)), 0.15);

            
        }
    }
}
