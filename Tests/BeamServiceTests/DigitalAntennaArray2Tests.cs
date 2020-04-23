using System;
using System.Linq;
using Antennas;
using BeamService;
using BeamService.Digital;
using BeamService.Functions;
using MathCore;
using MathCore.Vectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeamServiceTests
{
    [TestClass]
    public class DigitalAntennaArray2Tests
    {
        [TestMethod]
        public void Matrix_Beamforming_With_LFM_Signal()
        {
            const double fd = 8e9; // Hz
            const double max_amplidude = 5;
            var adc = new ADC(16, fd, max_amplidude);

            var antenna_item = new UniformAntenna();

            const int antennas_count_x = 16;
            const int antennas_count_y = 16;
            const double f0 = 1e9;  // Hz
            const double f_max = 2e9;  // Hz
            const double period = 60e-9;  // Hz
            const double dx = 0.15; // m
            const double dy = 0.15; // m
            const int samples_count = 960;

            var antenna_array = new DigitalAntennaArray2(samples_count);

            for (var ix = 0; ix < antennas_count_x; ix++)
                for (var iy = 0; iy < antennas_count_y; iy++)
                {
                    var location = new Vector3D(ix * dx, iy * dy);
                    antenna_array.Add(antenna_item, location, adc);
                }


            var beam_forming = new MatrixBeamForming(
                antenna_array.Select(e => e.Location).ToArray(),
                antenna_array.SamplesCount,
                fd);

            antenna_array.BeamForming = beam_forming;

            SignalFunction s = new LFM(f0, f_max, period, 0);

            const double source_angle_theta = 0;
            const double source_angle_phi = 0;
            var radio_scene = new RadioScene
            {
                { new SpaceAngle(source_angle_theta, source_angle_phi, AngleType.Deg), s }
            };

            const double beam_angle_theta = 0;
            const double beam_angle_phi = 0;
            beam_forming.PhasingАngle = new SpaceAngle(beam_angle_theta, beam_angle_phi, AngleType.Deg);

            var (signal, _) = antenna_array.GetSignal(radio_scene);

            var s0 = adc.GetDigitalSignal(new AnalogSignalSource(t => s.Value(t)), samples_count);
            var p0 = s0.GetTotalPower();

            var power = signal.GetTotalPower();

            var gain = Math.Sqrt(power / p0);

            Assert.AreEqual(antennas_count_x * antennas_count_y, gain, 1e-7);
        }

        [TestMethod]
        public void Matrix_Beamforming_With_Two_Sources()
        {
            const double fd = 8e9; // Hz
            const double max_amplidude = 5;
            var adc = new ADC(16, fd, max_amplidude);

            var antenna_item = new UniformAntenna();

            const int antennas_count_x = 16;
            const int antennas_count_y = 16;
            const double f0 = 1e9;  // Hz
            const double dx = 0.15; // m
            const double dy = 0.15; // m
            const int samples_count = 128;

            var antenna_array = new DigitalAntennaArray2(samples_count);

            for (var ix = 0; ix < antennas_count_x; ix++)
                for (var iy = 0; iy < antennas_count_y; iy++)
                {
                    var location = new Vector3D(ix * dx, iy * dy);
                    antenna_array.Add(antenna_item, location, adc);
                }


            var beam_forming = new MatrixBeamForming(
                antenna_array.Select(e => e.Location).ToArray(),
                antenna_array.SamplesCount,
                fd);

            antenna_array.BeamForming = beam_forming;

            SignalFunction source1 = new CosSignal(1, f0);
            SignalFunction source2 = new SinSignal(1, 2 * f0);

            const double source1_angle_theta = +3.5, source1_angle_phi = 0;
            const double source2_angle_theta = -3.5, source2_angle_phi = 0;
            var radio_scene = new RadioScene
            {
                { new SpaceAngle(source1_angle_theta, source1_angle_phi, AngleType.Deg), source2 },
                { new SpaceAngle(source2_angle_theta, source2_angle_phi, AngleType.Deg), source1 },
            };

            const double beam1_angle_theta = 3.5, beam1_angle_phi = 0;
            beam_forming.PhasingАngle = new SpaceAngle(beam1_angle_theta, beam1_angle_phi, AngleType.Deg);

            var signal1 = antenna_array.GetSignal(radio_scene);

            const double beam2_angle_theta = -3.5, beam2_angle_phi = 0;
            beam_forming.PhasingАngle = new SpaceAngle(beam2_angle_theta, beam2_angle_phi, AngleType.Deg);

            var signal2 = antenna_array.GetSignal(radio_scene);

            //const double beam3_angle_theta = 60, beam3_angle_phi = 60;
            //beam_forming.PhasingАngle = new SpaceAngle(beam3_angle_theta, beam3_angle_phi, AngleType.Deg);

            //var signal3 = antenna_array.GetSignal(radio_scene);

        }
    }
}
