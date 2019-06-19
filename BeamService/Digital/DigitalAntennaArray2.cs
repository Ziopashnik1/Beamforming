using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Antennas;
using BeamService.Digital;
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
            }

            return Filter?.Filter(result) ?? result;
        }
    }

    public class DigitalAntennaArray2 : Antennas.Antenna, IEnumerable<DigitalAntennaItem>, INotifyCollectionChanged
    {
        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        private readonly List<DigitalAntennaItem> _Items = new List<DigitalAntennaItem>();
        private int _SamplesCount;

        public int ElementsCount => _Items.Count;

        public int SamplesCount
        {
            get => _SamplesCount;
            set => Set(ref _SamplesCount, value);
        }

        public BeamForming BeamForming { get; set; }

        public DigitalAntennaArray2(int SamplesCount) => _SamplesCount = SamplesCount;

        public (DigitalSignal I, DigitalSignal Q) GetSignal(RadioScene Scene)
        {
            var signals = _Items.Select(AntennaItem => AntennaItem.GetSignal(Scene, _SamplesCount));
            if(BeamForming is null) throw new InvalidOperationException("Отсутствует диаграммообразующая схема");
            return BeamForming.GetSignal(signals.ToArray());
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
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            return item;
        }

        public DigitalAntennaItem Add(
            Antennas.Antenna antenna,
            Vector3D location, 
            ADC ADC)
        {
            var item = new DigitalAntennaItem(antenna, location, default, 1, ADC, null);
            _Items.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            return item;
        }

        public bool Remove(DigitalAntennaItem item)
        {
            var index = _Items.IndexOf(item);
            if (index < 0) return false;
            var removed = _Items.Remove(item);
            if (!removed) return false;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return true;
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
