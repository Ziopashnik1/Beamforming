using System.Linq;
using Antennas;
using BeamService;
using BeamService.Digital;
using MathService;
using MathService.Vectors;

namespace Antenna.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const double fd = 8e9; // Hz
            var adc = new ADC(16, fd, 5);

            var antenna_item = new UniformAntenna();

            const int antennas_count = 16;
            const double f0 = 1e9;  // Hz
            const double dx = 0.15; // m
            const int SamplesCount = 16;

            var antenna_array = new DigitalAntennaArray2(SamplesCount);
            

            var i = 0;
            foreach (var element in Enumerable.Repeat(antenna_item, antennas_count))
                antenna_array.Add(element, new Vector3D(i++ * dx), adc);

            var beam_forming = new MatrixBeamForming(
                antenna_array.Select(e => e.Location).ToArray(),
                antenna_array.SamplesCount,
                fd);

            antenna_array.BeamForming = beam_forming;

            var radio_scene = new RadioScene
            {
                { 30 * Consts.ToRad, 0, new CosSignal(1, f0) }
            };

            beam_forming.PhasingАngle = new SpaceAngle(30, 0, AngleType.Deg);

            var signal = antenna_array.GetSignal(radio_scene);
        }
    }
}
