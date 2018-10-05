using System;
using System.ComponentModel;

namespace BeamService
{
    public class SpaceSignal : ViewModel
    {
        private double f_Thetta;
        private SignalFunction f_Signal;

        public double Thetta
        {
            get => f_Thetta;
            set => Set(ref f_Thetta, value);
        }

        public SignalFunction Signal
        {
            get => f_Signal;
            set
            {
                var old_signal = f_Signal;
                if(!Set(ref f_Signal, value)) return;
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