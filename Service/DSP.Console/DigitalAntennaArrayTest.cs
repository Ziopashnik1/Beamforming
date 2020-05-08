using System.Linq;
using Antennas;
using BeamService;
using BeamService.Functions;
using MathCore;
using MathCore.Vectors;

namespace DSP.TestConsole
{
    static class DigitalAntennaArrayTest
    {
        public static void Test()
        {
            const double fd = 8e9; // Hz
            var adc = new ADC(8, fd, 5);

            var antenna_item = new UniformAntenna();

            const int antennas_count = 256;
            const double f0 = 1e9;  // Hz
            const double dx = 0.137; // m
            var antenna_array = Enumerable.Repeat(antenna_item, antennas_count)
                .Aggregate(
                    new DigitalAntennaArray(16),
                    (array, item, i) =>
                    {
                        array.Add(item, new Vector3D(i * dx), adc);
                        return array;
                    });

            var radio_scene = new RadioScene
            {
                { 30 * Consts.ToRad, 0, new CosSignal(1, f0) }
            };

            var signal = antenna_array.GetSignal(radio_scene);
        }
    }
}
