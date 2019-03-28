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

            const int antennas_count_x = 16;
            const int antennas_count_y = 16;
           // const int antennas_count_z = 16;
            const double f0 = 1e9;  // Hz
            const double dx = 0.15; // m
            const double dy = 0.15; // m
            //const double dz = 0.15; // m
            const int SamplesCount = 64;

            var antenna_array = new DigitalAntennaArray2(SamplesCount);

            for (var ix = 0; ix < antennas_count_x; ix++)
                for (var iy = 0; iy < antennas_count_y; iy++)
                    //for (var iz = 0; iz < antennas_count_z; iz++)
                    {
                        var location = new Vector3D(ix * dx, iy * dy);
                        antenna_array.Add(antenna_item, location, adc);
                    }

            //var i = 0;
            //foreach (var element in Enumerable.Repeat(antenna_item, antennas_count_x))
            //    antenna_array.Add(element, new Vector3D(i++ * dx), adc);

            var beam_forming = new MatrixBeamForming(
                antenna_array.Select(e => e.Location).ToArray(),
                antenna_array.SamplesCount,
                fd);

            antenna_array.BeamForming = beam_forming;

            SignalFunction s = new RectSignalFunction(5/ f0, 10 / f0);// * new CosSignal(1, f0);

            var radio_scene = new RadioScene
            {
                { new SpaceAngle(0,0, AngleType.Deg), s }
            };

            beam_forming.PhasingАngle = new SpaceAngle(0, 0, AngleType.Deg);

            var signal = antenna_array.GetSignal(radio_scene);
        }
    }
}
