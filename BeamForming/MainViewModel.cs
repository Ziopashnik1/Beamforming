using BeamService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BeamForming
{
    /// <summary>
    /// Класс логики представления интерфейса
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DigitalAntennaArray f_Antenna;

        private int f_N = 8;
        private double f_d = 0.15;
        private double f_fd = 8e9;
        private int f_n = 8;
        private int f_Nd = 8;
        private double f_MaxValue = 2;

        private PatternValue[] f_Beam;
        private PatternValue[] f_Beam_Norm;
        private double f_f0 = 1e9;
        private const double toRad = Math.PI / 180;
        private double f_th1 = -90;
        private double f_th2 = 90;
        private double f_dth = 0.5;

        public ReadOnlyCollection<PatternValue> Beam => new ReadOnlyCollection<PatternValue>(f_Beam);
        public ReadOnlyCollection<PatternValue> BeamNorm => new ReadOnlyCollection<PatternValue>(f_Beam_Norm);

        /// <summary>
        /// Угол фазирования антенноё решёётки  
        /// </summary>
        public double Th0
        {
            get => f_Antenna.th0 / toRad;
            set
            {
                if (f_Antenna.th0 == value) return;
                f_Antenna.th0 = value * toRad;
                OnPropertyChanged(nameof(Th0));
                CalculatePattern();
            }
        }

        /// <summary>
        /// Коэффициент усиления решётки
        /// </summary>
        public double Max { get; private set; }

        public MainViewModel()
        {
            f_Antenna = new DigitalAntennaArray(f_N, f_d, f_fd, f_n, f_Nd, f_MaxValue);
            CalculatePattern();
        }

        private void CalculatePattern()
        {
            f_Beam = f_Antenna.ComputePattern(f_f0, f_th1 * toRad, f_th2 * toRad, f_dth * toRad);
            OnPropertyChanged(nameof(Beam));
            Max = f_Beam.Max(v => v.Value);
            OnPropertyChanged(nameof(Max));
            f_Beam_Norm = f_Beam.Select(v => new PatternValue { Angle = v.Angle, Value = v.Value / Max }).ToArray();
            OnPropertyChanged(nameof(BeamNorm));
        }
    }
}
