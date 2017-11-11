using BeamService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string property = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(property);
            return true;
        }

        private Func<double, double> f_Signal;
        private DigitalAntennaArray f_Antenna;

        private int f_N = 8;
        private double f_d = 0.15;
        private double f_fd = 8e9;
        private int f_n = 8;
        private int f_Nd = 16;
        private double f_MaxValue = 2;

        private PatternValue[] f_Beam;
        private PatternValue[] f_Beam_Norm;
        private PatternValue[] f_Beam1 = new PatternValue[0];
        private PatternValue[] f_Beam1_Norm = new PatternValue[0];
        private double f_A0 = 1;
        private double f_f0 = 1e9;
        private const double toRad = Math.PI / 180;
        private double f_th1 = -90;
        private double f_th2 = 90;
        private double f_dth = 0.5;

        public ReadOnlyCollection<PatternValue> Beam => new ReadOnlyCollection<PatternValue>(f_Beam);
        public ReadOnlyCollection<PatternValue> BeamNorm => new ReadOnlyCollection<PatternValue>(f_Beam_Norm);

        public ReadOnlyCollection<PatternValue> Beam1 => new ReadOnlyCollection<PatternValue>(f_Beam1);
        public ReadOnlyCollection<PatternValue> BeamNorm1 => new ReadOnlyCollection<PatternValue>(f_Beam1_Norm);

        public SignalValue[] SignalIn { get; private set; }

        /// <summary>
        /// Угол фазирования антенноё решётки  
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
                ComputeOutSignal();
            }
        }

        private double f_th_signal = 5;
        public double ThSignal
        {
            get => f_th_signal;
            set
            {
                if (!Set(ref f_th_signal, value)) return;
                ComputeOutSignal();
            }
        }

        private void ComputeOutSignal()
        {
            var signals = f_Antenna.GetOutSignal(f_th_signal * toRad, f_Signal);
            OutSignalP = signals[0];
            OutSignalQ = signals[1];      
        }

        private DigitalSignal f_OutSignalP;
        private DigitalSignal f_OutSignalQ;
        public DigitalSignal OutSignalP
        {
            get => f_OutSignalP;
            set => Set(ref f_OutSignalP, value);
        }
        public DigitalSignal OutSignalQ
        {
            get => f_OutSignalQ;
            set => Set(ref f_OutSignalQ, value);
        }

        /// <summary>
        /// Коэффициент усиления решётки
        /// </summary>
        public double Max { get; private set; } = double.NaN;

        public double Max_db => 10 * Math.Log10(Max);

        public double MaxPos { get; private set; } = double.NaN;

        public double BeamWidth07 { get; private set; } = double.NaN;

        public double BeamWidth0 { get; private set; } = double.NaN;

        public double LeftBeamEdge_07 { get; private set; } = double.NaN;

        public double RightBeamEdge_07 { get; private set; } = double.NaN;

        public double UBL { get; private set; } = double.NaN;

        public double MeanUBL { get; private set; } = double.NaN;

        public DataPoint[] KND_th0 { get; private set; }


        private DigitalSignal[] f_InputSignals;
        public DigitalSignal[] InputSignals
        {
            get => f_InputSignals;
            set => Set(ref f_InputSignals, value);
        }

        public MainViewModel()
        {
            f_Signal = t => f_A0 * Math.Sin(2 * Math.PI * f_f0 * t);// + f_A0 * Math.Sin(2 * Math.PI * 2 * f_f0 * t);   // Определяем сигнал

            f_Antenna = new DigitalAntennaArray(f_N, f_d, f_fd, f_n, f_Nd, f_MaxValue, 0e-10);
            Func<double, double> f1 = th => Math.Cos(th);
            f_Antenna.ElementPattern = f1;
            CalculatePattern();
            CalculateKND_from_th0_Async();
            ComputeOutSignal();
        }

        private void CalculatePattern()
        {
            f_Beam = f_Antenna.ComputePattern(f_Signal, f_th1 * toRad, f_th2 * toRad, f_dth * toRad);
            OnPropertyChanged(nameof(Beam));
            f_Beam_Norm = GetBeamNorm(f_Beam, out var max);
            OnPropertyChanged(nameof(BeamNorm));
            f_Beam1 = ComputePattern(f_Antenna.ElementPattern, f_th1 * toRad, f_th2 * toRad, f_dth * toRad);
            OnPropertyChanged(nameof(Beam1));
            f_Beam1_Norm = GetBeamNorm(f_Beam1, out var _);
            OnPropertyChanged(nameof(BeamNorm1));
            Max = max;
            OnPropertyChanged(nameof(Max));
            OnPropertyChanged(nameof(Max_db));
            OnPropertyChanged(nameof(BeamNorm));

            Func<double, Complex> F = th => f_Antenna.ComputePatternValue(th, f_Signal);
            F.GetMaximum(out var max_pos);
            MaxPos = max_pos / toRad;
            OnPropertyChanged(nameof(MaxPos));

            F.GetPatternWidth(max_pos, out var left_07, out var right_07, out var left_0, out var right_0);
            LeftBeamEdge_07 = left_07 / toRad;
            RightBeamEdge_07 = right_07 / toRad;
            OnPropertyChanged(nameof(LeftBeamEdge_07));
            OnPropertyChanged(nameof(RightBeamEdge_07));

            BeamWidth07 = (right_07 - left_07) / toRad;
            OnPropertyChanged(nameof(BeamWidth07));
            BeamWidth0 = (right_0 - left_0) / toRad;
            OnPropertyChanged(nameof(BeamWidth0));

            UBL = f_Beam_Norm.Where(v => !(v.Angle > left_0 && v.Angle < right_0)).ToArray().Max(v => v.Value_db);
            OnPropertyChanged(nameof(UBL));

            MeanUBL = 10 * Math.Log10(f_Beam_Norm.Where(v => !(v.Angle > left_0 && v.Angle < right_0)).Sum(v => v.Value) * f_dth / (180 - BeamWidth0));
            OnPropertyChanged(nameof(MeanUBL));

            SignalIn = f_Antenna.ADC[0].GetDiscretSignal(new Source(f_Signal), f_Antenna.Nd);
            OnPropertyChanged(nameof(SignalIn));

            InputSignals = f_Antenna.GetInputDigitalSignals(Th0, f_Signal);
        }

        private PatternValue[] ComputePattern(Func<double, double> F, double th1, double th2, double dth)
        {
            var th = th1;
            var count = (int)((th2 - th1) / dth) + 1;
            var result = new List<PatternValue>(count);
            while(th < th2)
            {
                result.Add(new PatternValue { Angle = th, Value = F(th) });
                th += dth;
            }
            return result.ToArray();
        }

        private static PatternValue[] GetBeamNorm(PatternValue[] pattern, out double Max)
        {
            var max = pattern.Max(v => v.Value);
            Max = max;
            return pattern.Select(v => new PatternValue { Angle = v.Angle, Value = v.Value / max }).ToArray();
        }

        public async void CalculateKND_from_th0_Async()
        {
            await Task.Run((Action)CalculateKND_from_th0).ConfigureAwait(true);
            OnPropertyChanged(nameof(KND_th0));
        }

        private void CalculateKND_from_th0()
        {
            var array = new DigitalAntennaArray(f_N, f_d, f_fd, f_n, f_Nd, f_MaxValue);
            array.ElementPattern = Math.Cos;

            var result = new List<DataPoint>(90);
            for (var th0 = 0d; th0 < 45; th0 += 0.5)
            {
                array.th0 = th0 * toRad;
                var pattern = array.ComputePattern(f_Signal, -90 * toRad, 90 * toRad, 0.5 * toRad);
                var max = pattern.Max(v => v.Value);
                result.Add(new DataPoint { X = th0, Y = max });
            }
            KND_th0 = result.ToArray();
        }
    }

    public class DataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Ydb => 20 * Math.Log10(Y);
        public double YdbP => 10 * Math.Log10(Y);
    }
}
