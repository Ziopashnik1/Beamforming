using System;
using Antennas;
using BeamService.AmplitudeDestributions;
using DSP.Lib;
using MathService;
using MathService.Vectors;

namespace BeamService
{
    public class DigitalAntennaItem : AntennaItem
    {
        public ADC ADC { get; }

        public DigitalFilter Filter { get; set; }

        public DigitalAntennaItem(
            Antenna antenna,
            Vector3D location,
            SpaceAngle angle,
            Complex k,
            ADC ADC,
            DigitalFilter filter)
            : base(antenna, location, angle, k)
        {
            this.ADC = ADC;
            Filter = filter;
        }

        public DigitalSignal GetSignal(RadioScene Scene, int SamplesCount, Func<double, double> Ax, Func<double, double> Ay)
        {
            var antenna_location = Location;

            AnalogSignalSource analog_result = null;
            foreach (var signal in Scene)
            {
                var signal_angle = signal.Angle;
                var delta_t = antenna_location.GetProjectionTo(signal_angle) / Consts.SpeedOfLigth;

                var A = (Ax?.Invoke(antenna_location.X) ?? 1) * (Ay?.Invoke(antenna_location.Y) ?? 1);
                var F = Element.Pattern(signal_angle, 1).Abs;

                analog_result += new AnalogSignalSource(t => A * F * signal.Signal.Value(t - delta_t));
            }

            var result = ADC.GetDigitalSignal(analog_result, SamplesCount);

            return Filter?.Filter(result) ?? result;
        }
    }
}