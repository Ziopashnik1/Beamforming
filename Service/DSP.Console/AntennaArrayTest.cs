using System.Linq;
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
