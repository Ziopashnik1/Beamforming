using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Antennas;
using BeamService.AmplitudeDestributions;
using BeamService.Digital;
using DSP.Lib;
using MathCore;
using MathCore.Vectors;

namespace BeamService
{
    public class DigitalAntennaArray2 : Antenna, IEnumerable<DigitalAntennaItem>, INotifyCollectionChanged
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

        private double _AnalogAmpl = 1;

        public double AnalogAmpl
        {
            get => _AnalogAmpl;
            set => Set(ref _AnalogAmpl, value);
        }

    public BeamForming BeamForming { get; set; }

        public DigitalAntennaArray2(int SamplesCount) => _SamplesCount = SamplesCount;

        public (DigitalSignal I, DigitalSignal Q) GetSignal(RadioScene Scene, Func<double, double> Ax = null, Func<double, double> Ay = null)
        {
            var signals = _Items.Select(AntennaItem => AntennaItem.GetSignal(Scene, _SamplesCount, _AnalogAmpl, Ax, Ay));
            if(BeamForming is null) throw new InvalidOperationException("Отсутствует диаграммообразующая схема");
            return BeamForming.GetSignal(signals.ToArray());
        }

        public DigitalAntennaItem Add(
            Antenna antenna, 
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
            Antenna antenna,
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
