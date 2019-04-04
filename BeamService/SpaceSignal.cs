using System;
using System.ComponentModel;
using BeamService.Functions;
using MathService.Vectors;
using MathService.ViewModels;

namespace BeamService
{
    public class SpaceSignal : ViewModel
    {
        private double _Thetta;
        private double _Phi;
        private SignalFunction _Signal;

        public double Thetta
        {
            get => _Thetta;
            set => SetValue(ref _Thetta, value).Update(nameof(Angle));
        }

        public double Phi
        {
            get => _Phi;
            set => SetValue(ref _Phi, value).Update(nameof(Angle));
        }

        public SpaceAngle Angle
        {
            get => new SpaceAngle(_Thetta, _Phi);
            set
            {
                Thetta = value.ThettaRad;
                Phi = value.PhiRad;
            }
        }

        public SignalFunction Signal
        {
            get => _Signal;
            set
            {
                var old_signal = _Signal;
                if (!Set(ref _Signal, value)) return;
                if (old_signal != null) old_signal.PropertyChanged -= OnSignalPropertyChanged;
                if (value != null) value.PropertyChanged += OnSignalPropertyChanged;
            }
        }

        public void Deconstruct(out double thetta, out Func<double, double> signal)
        {
            thetta = Thetta;
            signal = Signal.Value;
        }

        private void OnSignalPropertyChanged(object Sender, PropertyChangedEventArgs E) => OnPropertyChanged(nameof(Signal));
    }
}