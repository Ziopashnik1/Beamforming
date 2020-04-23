using System;
using System.ComponentModel;
using BeamService.Functions;
using MathCore.Vectors;
using MathCore.ViewModels;

namespace BeamService
{
    public class SpaceSignal : ViewModel
    {
        private double _Theta;
        private double _Phi;
        private SignalFunction _Signal;

        public double Theta
        {
            get => _Theta;
            set => SetValue(ref _Theta, value).Update(nameof(Angle));
        }

        public double Phi
        {
            get => _Phi;
            set => SetValue(ref _Phi, value).Update(nameof(Angle));
        }

        public SpaceAngle Angle
        {
            get => new SpaceAngle(_Theta, _Phi);
            set
            {
                Theta = value.ThetaRad;
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

        public void Deconstruct(out double theta, out Func<double, double> signal)
        {
            theta = Theta;
            signal = Signal.Value;
        }

        private void OnSignalPropertyChanged(object Sender, PropertyChangedEventArgs E) => OnPropertyChanged(nameof(Signal));
    }
}