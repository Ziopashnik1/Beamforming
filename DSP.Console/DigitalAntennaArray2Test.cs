using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antennas;
using BeamService;
using BeamService.Functions;
using MathService;
using MathService.Vectors;

namespace DSP.TestConsole
{
    static class DigitalAntennaArray2Test
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
                    new DigitalAntennaArray2(16),
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
