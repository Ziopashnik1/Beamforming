using System.Linq;
using Antennas;
using BeamService;
using BeamService.Digital;
using BeamService.Functions;
using MathService;
using MathService.Vectors;

namespace Antenna.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const double fmax = 1e9; // Hz
            const int signal_count = 22;
            const double band = 200e6;
            const double df = band / (signal_count - 1);

            var radio_scene = Enumerable.Range(0, signal_count)
                .Select(i => new CosSignal(1, fmax - df * i))
                .Aggregate(new RadioScene(), (scene, signal) => { scene.Add(new SpaceAngle(30, 30, AngleType.Deg), signal); return scene; });


            const double fd = 3e9; // Hz
            var adc = new ADC(16, fd, 5);
            var antenna_item = new UniformAntenna();

            const int antennas_count_x = 16;
            const int antennas_count_y = 16;
            const double f0 = 1e9;  // Hz
            const double dx = 0.15; // m
            const double dy = 0.15; // m
            const int SamplesCount = 300;

            var antenna_array = new DigitalAntennaArray2(SamplesCount);

            for (var ix = 0; ix < antennas_count_x; ix++)
                for (var iy = 0; iy < antennas_count_y; iy++)
                    antenna_array.Add(antenna_item, new Vector3D(ix * dx, iy * dy), adc);

            var beam_forming = new MatrixBeamForming(
                antenna_array.Select(e => e.Location).ToArray(),
                antenna_array.SamplesCount,
                fd);

            antenna_array.BeamForming = beam_forming;

            beam_forming.PhasingАngle = new SpaceAngle(30, 30, AngleType.Deg);
            var signal = antenna_array.GetSignal(radio_scene);

            var spectrum = signal.GetSpectrum();
        }
    }
}
