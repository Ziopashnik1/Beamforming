using BeamService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MathService;

// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace BeamForming
{
    /// <summary>Класс логики представления интерфейса</summary>
    public class MainViewModel : ViewModel
    {
        /// <summary>Функция сигнала падающей волны</summary>
        private Func<double, double> f_Signal;

        /// <summary>Исследуемая антенная решётка</summary>
        private DigitalAntennaArray f_Antenna;

        /// <summary>Число элементов решётки</summary>
        private int f_N = 8;

        /// <summary>Шаг между элементами решётки</summary>
        private double f_d = 0.15;

        /// <summary>Частота дискретизации сигнала в Гц</summary>
        private double f_fd = 16e9;

        /// <summary>Число разрядов кода АЦП</summary>
        private int f_n = 8;

        /// <summary>Размер выборки цифрового сигнала</summary>
        private int f_Nd = 64;

        /// <summary>Динамический диапазон АЦП (максимальное значение амплитуды входного ограничителя)</summary>
        private double f_MaxValue = 2;

        /// <summary>Массив отсчётов ДН решётки (не нормированный)</summary>
        private PatternValue[] f_Beam;

        /// <summary>Массив отсчётов ДН решётки (нормированный)</summary>
        private PatternValue[] f_Beam_Norm;

        /// <summary>Массив отсчётов ДН единичного излучателя (не нормированный)</summary>
        private PatternValue[] f_Beam1 = new PatternValue[0];

        /// <summary>Массив отсчётов ДН единичного излучателя (нормированный)</summary>
        private PatternValue[] f_Beam1_Norm = new PatternValue[0];

        private PatternValue[] f_RadioSceneBeamPattern = new PatternValue[0];

        /// <summary>Амплитуда сигнала</summary>
        private double f_A0 = 1;

        /// <summary>Частота сигнала</summary>
        private double f_f0 = 1e9;

        /// <summary>Константа преобразования градусов в радианы</summary>
        private const double toRad = Math.PI / 180;

        /// <summary>Начало сектора углов расчёта ДН (градусы)</summary>
        private double f_th1 = -90;

        /// <summary>Конец сектора углов расчёта ДН (градусы)</summary>
        private double f_th2 = 90;

        /// <summary>Шаг расчёта ДН</summary>
        private double f_dth = 0.5;

        /// <summary>Массив отсчётов ДН решётки (не нормированный)</summary>
        public ReadOnlyCollection<PatternValue> Beam => new ReadOnlyCollection<PatternValue>(f_Beam);

        /// <summary>Массив отсчётов ДН решётки (нормированный)</summary>
        public ReadOnlyCollection<PatternValue> BeamNorm => new ReadOnlyCollection<PatternValue>(f_Beam_Norm);

        /// <summary>Массив отсчётов ДН единичного излучателя (не нормированный)</summary>
        public ReadOnlyCollection<PatternValue> Beam1 => new ReadOnlyCollection<PatternValue>(f_Beam1);

        /// <summary>Массив отсчётов ДН единичного излучателя (нормированный)</summary>
        public ReadOnlyCollection<PatternValue> BeamNorm1 => new ReadOnlyCollection<PatternValue>(f_Beam1_Norm);

        public ReadOnlyCollection<PatternValue> RadioSceneBeamPattern => f_RadioSceneBeamPattern.ToList().AsReadOnly();

        /// <summary>Сигнал на выходе АЦП</summary>
        public SignalValue[] SignalIn { get; private set; }

        /// <summary>Число элементов решётки</summary>
        public int N
        {
            get => f_Antenna.N;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(N), "Число элементов решётки должно быть больше 0");
                if (N == value) return;
                f_Antenna.N = value;
                OnPropertyChanged();
                ComputeOutSignal();
            }
        }

        /// <summary>Размер выборки цифрового сигнала </summary>  сам ДЕЛАЛ!!!!!!!
        public int Nd
        {
            get => f_Antenna.Nd;
            set
            {
                if (value < 2) throw new ArgumentOutOfRangeException(nameof(Nd), "Размер выборки должен быть больше числа элементов");
                if (Nd == value) return;
                f_Antenna.Nd = value;
                OnPropertyChanged();
                ComputeOutSignal();
            }
        }

        /// <summary>Шаг элементов</summary>
        public double d
        {
            get => f_Antenna.d;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(d), "Шаг между элементами решётки должен быть больше 0");
                if (d == value) return;
                f_Antenna.d = value;
                OnPropertyChanged();
                ComputeOutSignal();
            }
        }

        /// <summary>Разрядность кода АЦП</summary>
        public int n
        {
            get => f_Antenna.n;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(n), "Разрядность кода АЦП должна быть больше 0");
                if (n == value) return;
                f_Antenna.n = value;
                OnPropertyChanged();
                ComputeOutSignal();
            }
        }

        /// <summary>Частота дискретизации</summary>
        public double fd
        {
            get => f_Antenna.fd;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(fd), "Частота дискретизации должна быть больше 0");
                if (fd == value) return;
                f_Antenna.fd = value;
                OnPropertyChanged();
                ComputeOutSignal();
            }
        }

        /// <summary>Динамический диапазон входа АЦП</summary>
        public double MaxValue
        {
            get => f_Antenna.MaxValue;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(MaxValue), "Динамический диапазон входа АЦП должен быть больше 0");
                if (MaxValue == value) return;
                f_Antenna.MaxValue = value;
                OnPropertyChanged();
                ComputeOutSignal();
            }
        }

        /// <summary>Джиттер АЦП</summary>
        public double tj
        {
            get => f_Antenna.tj;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(tj), "Динамический диапазон входа АЦП должен быть больше 0");
                if (tj == value) return;
                f_Antenna.tj = value;
                OnPropertyChanged();
                ComputeOutSignal();
            }
        }

        /// <summary>Положение луча антенной решётки</summary>
        public double Th0
        {
            get => f_Antenna.th0 / toRad;
            set
            {
                value *= toRad;
                if (f_Antenna.th0 == value) return;
                f_Antenna.th0 = value;
                OnPropertyChanged(nameof(Th0));
                CalculatePattern();
                ComputeOutSignal();
            }
        }

        /// <summary>Угол мадения волны на решётку</summary>
        private double f_th_signal = 5;
        /// <summary>Угол мадения волны на решётку</summary>
        public double ThSignal
        {
            get => f_th_signal;
            set
            {
                if (!Set(ref f_th_signal, value)) return;
                ComputeOutSignal();
            }
        }

        /// <summary>Угол мадения волны 1 на решётку</summary>
        private double f_th_signal1 = 0;
        /// <summary>Угол мадения волны 1 на решётку</summary>
        public double ThSignal1
        {
            get => f_th_signal1;
            set
            {
                if (!Set(ref f_th_signal1, value)) return;
                ComputeOutSignal();
                CalculatePattern();
            }
        }

        /// <summary>Угол мадения волны 2 на решётку</summary>
        private double f_th_signal2 = 10;
        /// <summary>Угол мадения волны 2 на решётку</summary>
        public double ThSignal2
        {
            get => f_th_signal2;
            set
            {
                if (!Set(ref f_th_signal2, value)) return;
                ComputeOutSignal();
            }
        }

        /// <summary>Частота сигнала 1 падаюющей волны</summary>
        private double f_f01 = 1e9;
        /// <summary>Частота сигнала 1 падаюющей волны</summary>
        public double f01
        {
            get => f_f01;
            set
            {
                if (!Set(ref f_f01, value)) return;
                ComputeOutSignal();
            }
        }

        /// <summary>Частота сигнала 2 падаюющей волны</summary>
        private double f_f02 = 2e9;
        /// <summary>Частота сигнала 2 падаюющей волны</summary>
        public double f02
        {
            get => f_f02;
            set
            {
                if (!Set(ref f_f02, value)) return;
                ComputeOutSignal();
            }
        }

        /// <summary>Амплитуда сигнала 1 падаюющей волны</summary>
        private double f_A01 = 0.5;
        /// <summary>Амплитуда сигнала 1 падаюющей волны</summary>
        public double A01
        {
            get => f_A01;
            set
            {
                if (!Set(ref f_A01, value)) return;
                ComputeOutSignal();
            }
        }

        /// <summary>Амплитуда сигнала 2 падаюющей волны</summary>
        private double f_A02 = 0.00;
        /// <summary>Амплитуда сигнала 2 падаюющей волны</summary>
        public double A02
        {
            get => f_A02;
            set
            {
                if (!Set(ref f_A02, value)) return;
                ComputeOutSignal();
            }
        }


        /// <summary>Функция сигнала - периодического парямоугольного испульса (единичной амплитуды)</summary>
        /// <param name="t">Текущее время</param>
        /// <param name="tau">Длительность импульса</param>
        /// <param name="T">Период повторения</param>
        /// <returns>Амплитуда сигнала</returns>
        private static double Rect(double t, double tau, double T)
        {
            t = t % T + (t < 0 ? T : 0);
            if (t.Equals(tau) || t.Equals(0d)) return 0.5;
            if (0 < t && t < tau) return 1;
            return 0;
        }

        /// <summary>Вычислить сигнал на выходе антенной решётки</summary>
        private void ComputeOutSignal()
        {
            double SignalFunction1(double t) => f_A01 * Rect(t, 1e-9, 4e-9) * Math.Cos(Math.PI * 2 * t * f_f01);   //          * Rect(t, 1e-9, 2e-9)
            double SignalFunction2(double t) => f_A02 * Math.Cos(Math.PI * 2 * t * f_f02);

            var scene = new RadioScene
            {
                new SpaceSignal{ Thetta = f_th_signal1 * toRad, Signal = new LamdaSignalFunction(SignalFunction1) },
                new SpaceSignal{ Thetta = f_th_signal2 * toRad, Signal = new LamdaSignalFunction(SignalFunction2) }
            };

            (OutSignalI, OutSignalQ) = f_Antenna.GetOutSignal(scene);
            CalculatePatternAsync();
        }

        /// <summary>Синфазная составляющая выходного сигнала</summary>
        private DigitalSignal f_OutSignalI;
        /// <summary>Синфазная составляющая выходного сигнала</summary>
        public DigitalSignal OutSignalI { get => f_OutSignalI; set => Set(ref f_OutSignalI, value); }

        /// <summary>Квадратурная составляющая выходного сигнала</summary>
        private DigitalSignal f_OutSignalQ;
        /// <summary>Квадратурная составляющая выходного сигнала</summary>
        public DigitalSignal OutSignalQ { get => f_OutSignalQ; set => Set(ref f_OutSignalQ, value); }

        /// <summary>Коэффициент усиления решётки (максимум не нормированной ДН)</summary>
        public double Max { get; private set; } = double.NaN;

        /// <summary>Коэффициент усиления решётки в дБ (максимум не нормированной ДН)</summary>
        public double Max_db => 20 * Math.Log10(Max);

        /// <summary>Положение максимума ДН (градусы)</summary>
        public double MaxPos { get; private set; } = double.NaN;

        /// <summary>Ширина луча по уровню 0.7 ДН (градусы)</summary>
        public double BeamWidth07 { get; private set; } = double.NaN;

        /// <summary>Ширина луча по уровню 0 ДН (градусы)</summary>
        public double BeamWidth0 { get; private set; } = double.NaN;

        /// <summary>Левая граница луча по уровню 0.7 (градусы)</summary>
        public double LeftBeamEdge_07 { get; private set; } = double.NaN;

        /// <summary>Правая граница луча по уровню 0.7 (градусы)</summary>
        public double RightBeamEdge_07 { get; private set; } = double.NaN;

        /// <summary>УБЛ</summary>
        public double UBL { get; private set; } = double.NaN;

        /// <summary>Средний УБЛ</summary>
        public double MeanUBL { get; private set; } = double.NaN;

        /// <summary>КНД в угломестной плоскости</summary>
        public DataPoint[] KND_th0 { get; private set; }

        /// <summary>Массив входных сигналов</summary>
        private DigitalSignal[] f_InputSignals;
        /// <summary>Массив входных сигналов</summary>
        public DigitalSignal[] InputSignals
        {
            get => f_InputSignals;
            set => Set(ref f_InputSignals, value);
        }

        /// <summary>Рассчитать ДН решётки</summary>
        public ICommand CalculatePatternCommand { get; }

        /// <summary>Инициализация новой модели представления пользовательского интерфейса</summary>
        public MainViewModel()
        {
            CalculatePatternCommand = new LamdaCommand(p => CalculatePattern(), p => true);

            f_Signal = t => f_A0 * Math.Cos(2 * Math.PI * f_f0 * t);//+ f_A0* Math.Sin(2 * Math.PI * 2 * f_f0 * t);   // Определяем сигнал // ЭТО ВООБЩЕ НУЖНО?

            f_Antenna = new DigitalAntennaArray(f_N, f_d, f_fd, f_n, f_Nd, f_MaxValue, 0e-10)
            {
                Element = new CosElement()
            };
            CalculatePattern();
            CalculateKND_from_th0_Async();
            ComputeOutSignal();
        }

        /// <summary>Рассчитать ДН решётки</summary>
        private async void CalculatePattern()
        {
            f_Beam = f_Antenna.ComputePattern(f_Signal, f_th1 * toRad, f_th2 * toRad, f_dth * toRad);
            OnPropertyChanged(nameof(Beam));
            f_Beam_Norm = GetBeamNorm(f_Beam, out var max);
            OnPropertyChanged(nameof(BeamNorm));
            f_Beam1 = ComputePattern(th => f_Antenna.Element.Pattern(th).Abs, f_th1 * toRad, f_th2 * toRad, f_dth * toRad);
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

            SignalIn = f_Antenna.ADC[0].GetDiscretSignal(new AnalogSignalSource(f_Signal), f_Antenna.Nd);
            OnPropertyChanged(nameof(SignalIn));

            InputSignals = f_Antenna.GetInputDigitalSignals(Th0, f_Signal);
        }

        private CancellationTokenSource f_CalculatePatternAsync_CancelationTokenSource = new CancellationTokenSource();

        private async void CalculatePatternAsync()
        {
            f_CalculatePatternAsync_CancelationTokenSource.Cancel();
            f_CalculatePatternAsync_CancelationTokenSource = new CancellationTokenSource();
            var token = f_CalculatePatternAsync_CancelationTokenSource.Token;

            // ReSharper disable once MethodSupportsCancellation
            await Task.Delay(5);
            if (token.IsCancellationRequested) return;

            try
            {
                f_RadioSceneBeamPattern = await Task.Run(() => CalculatePattern2(token), token);
                OnPropertyChanged(nameof(RadioSceneBeamPattern));
            }
            catch (OperationCanceledException)
            {

            }
        }

        /// <summary>Расчёт ДН с учётом радиосцены</summary>
        private PatternValue[] CalculatePattern2(CancellationToken cancel)
        {
            double SignalFunction(double t) => f_A01 * Rect(t, 1e-9, 5e-9) * Math.Cos(Math.PI * 2 * t * f_f01);    //    * Rect(t, 1e-9, 2e-9)       
            double NoiseFunction(double t) => f_A02 * Math.Cos(Math.PI * 2 * t * f_f02);

            var pattern_values = new PatternValue[361];
            var d_th = 180d / (pattern_values.Length - 1);

            for (var i = 0; i < pattern_values.Length; i++)
            {
                cancel.ThrowIfCancellationRequested();
                var th = i * d_th - 90;
                var scene = new RadioScene
                {
                    new SpaceSignal{ Thetta = th * toRad, Signal = new LamdaSignalFunction(SignalFunction) },
                    new SpaceSignal{ Thetta = f_th_signal2 * toRad, Signal = new LamdaSignalFunction(NoiseFunction) }
                };
                var signals = f_Antenna.GetOutSignal(scene);

                pattern_values[i] = new PatternValue { Angle = th * toRad, Value = Math.Sqrt(signals.P.Power + signals.Q.Power) };
            }
            cancel.ThrowIfCancellationRequested();
            var max = pattern_values.Max(v => v.Value); foreach (var v in pattern_values) v.Value /= max;
            return pattern_values;
        }

        /// <summary>Рассчитать ДН решётки</summary>
        /// <param name="F">Сигнал падающей волны</param>
        /// <param name="th1">Начало сектора расчёта</param>
        /// <param name="th2">Конец сектора расчёта</param>
        /// <param name="dth">Шаг расчёта ДН</param>
        /// <returns>Массив значений ДН</returns>
        private PatternValue[] ComputePattern(Func<double, double> F, double th1, double th2, double dth)
        {
            var th = th1;
            var count = (int)((th2 - th1) / dth) + 1;
            var result = new List<PatternValue>(count);
            while (th < th2)
            {
                result.Add(new PatternValue { Angle = th, Value = F(th) });
                th += dth;
            }
            return result.ToArray();
        }

        /// <summary>Вычислить нгормированную ДН по отсчётам ненормированной ДН</summary>
        /// <param name="pattern">Массив отсчётов ненормированной ДН</param>
        /// <param name="Max">Найденное значение максимума ДН (усиление)</param>
        /// <returns>Массив отсчётов нормированной ДН</returns>
        private static PatternValue[] GetBeamNorm(PatternValue[] pattern, out double Max)
        {
            var max = pattern.Max(v => v.Value);
            Max = max;
            return pattern.Select(v => new PatternValue { Angle = v.Angle, Value = v.Value / max }).ToArray();
        }

        /// <summary>Асинхронно рассчитать КНД решётки в угломестной плоскости</summary>
        public async void CalculateKND_from_th0_Async()
        {
            await Task.Run((Action)CalculateKND_from_th0).ConfigureAwait(true);
            OnPropertyChanged(nameof(KND_th0));
        }

        /// <summary>Рассчитать КНД решётки в угломестной плоскости</summary>
        private void CalculateKND_from_th0()
        {
            var array = new DigitalAntennaArray(f_N, f_d, f_fd, f_n, f_Nd, f_MaxValue);
            array.Element = new CosElement();

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
}
