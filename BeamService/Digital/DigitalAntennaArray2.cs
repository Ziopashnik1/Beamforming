using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Antennas;
using DSP.Lib;
using MathService;
using MathService.Vectors;

namespace BeamService
{
    public class DigitalAntennaArray2 : Antennas.Antenna, IEnumerable<DigitalAntennaArray2.DigitalAntennaItem>
    {
        public class DigitalAntennaItem : AntennaItem
        {
            public ADC ADC { get; }

            public DigitalFilter Filter { get; set; }

            public DigitalAntennaItem(
                Antennas.Antenna antenna, 
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

            public DigitalSignal GetSignal(RadioScene Scene, int SamplesCount)
            {
                var antenna_location = Location;

                DigitalSignal result = null;
                foreach (var signal in Scene)
                {
                    var signal_angle = signal.Angle;
                    var delta_t = antenna_location.GetProjectionTo(signal_angle) / Consts.SpeedOfLigth;

                    var analog_signal_source = new AnalogSignalSource(t => signal.Signal.Value(t - delta_t));
                    result += ADC.GetDigitalSignal(analog_signal_source, SamplesCount);

                    //todo: дописать диаграммообразующую схему
                }

                return result;
            }
        }

        private readonly List<DigitalAntennaItem> _Items = new List<DigitalAntennaItem>();

        public DigitalAntennaArray2() { }

        public DigitalSignal GetSignal(RadioScene Scene, int SamplesCount)
        {
            DigitalSignal result = null;

            foreach (var antenna_item in _Items)
            {
                var antenna_signal = antenna_item.GetSignal(Scene, SamplesCount);
                result += antenna_signal;
            }

            return result;


            //return _Items
            //    .AsParallel()
            //    .Aggregate(
            //        default(DigitalSignal), 
            //        (S, item) => S + item.GetSignal(Scene, SamplesCount));
        }


        public DigitalAntennaItem Add(
            Antennas.Antenna antenna, 
            Vector3D location,
            SpaceAngle angle,
            Complex K, 
            ADC ADC,
            DigitalFilter Filter)
        {
            var item = new DigitalAntennaItem(antenna, location, angle, K, ADC, Filter);
            _Items.Add(item);
            return item;
        }

        public DigitalAntennaItem Add(
            Antennas.Antenna antenna,
            Vector3D location, 
            ADC ADC)
        {
            var item = new DigitalAntennaItem(antenna, location, default, 1, ADC, null);
            _Items.Add(item);
            return item;
        }

        #region Overrides of Antenna

        /// <summary>Диаграмма направленности антенной решётки</summary>
        /// <param name="Direction">Пространственный угол</param>
        /// <param name="f">Частота</param>
        /// <returns>Комплексное значение ДН</returns>
        public override Complex Pattern(SpaceAngle Direction, double f) =>
            _Items.Count == 0
                ? Complex.NaN
                : _Items.AsParallel().Sum(i => i.Pattern(Direction, f));


        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<DigitalAntennaItem> GetEnumerator() => _Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _Items).GetEnumerator();

        #endregion
    }
}
