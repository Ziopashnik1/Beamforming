using BeamService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Antennas;
using BeamService.Digital;
using BeamService.Functions;
using DSP.Lib;
using MathService;
using MathService.Vectors;
using MathService.ViewModels;
using Antenna = Antennas.Antenna;
using IAntenna = Antennas.IAntenna;
using PatternValue = BeamService.PatternValue;
using Vibrator = Antennas.Vibrator;

namespace BeamForming
{
    internal class MainWindow3ViewModel : ViewModel
    {
        private ADC _ADC;

        public ADC ADC
        {
            get => _ADC;
            set => Set(ref _ADC, value);
        }

        private Antenna _AntennaItem;

        public Antenna AntennaItem
        {
            get => _AntennaItem;
            set => Set(ref _AntennaItem, value);
        }

        private int _Nx = 8;

        public int Nx
        {
            get => _Nx;
            set
            {
                var old_nx = _Nx;
                if (!Set(ref _Nx, value)) return;

                if (old_nx < value)
                {
                    // Нужно добавить новые элементы
                    var new_count = value - old_nx;
                    for (var j = 0; j < _Ny; j++)
                        for (var i = 1; i <= new_count; i++)
                            Antenna.Add(_AntennaItem, new Vector3D((old_nx + i) * _dx, j * _dy), _ADC);
                }
                else
                {
                    // Нужно найти и удалить лишние
                    var max_x = value * _dx;
                    foreach (var item in Antenna.Where(item => item.Location.X >= max_x).ToArray())
                        Antenna.Remove(item);
                }

                UpdateBeamforming();
            }
        }

        private int _Ny = 8;
        public int Ny
        {
            get => _Ny;
            set
            {
                var old_ny = _Ny;
                if (!Set(ref _Ny, value)) return;

                if (old_ny < value)
                {
                    // Нужно добавить новые элементы
                    var new_count = value - old_ny;
                    for (var i = 0; i < _Nx; i++)
                        for (var j = 1; j <= new_count; j++)
                            Antenna.Add(_AntennaItem, new Vector3D(i * _dx, (old_ny + j) * _dy), _ADC);
                }
                else
                {
                    // Нужно найти и удалить лишние
                    var max_y = value * _dy;
                    foreach (var item in Antenna.Where(item => item.Location.Y >= max_y).ToArray())
                        Antenna.Remove(item);
                }

                UpdateBeamforming();
            }
        }

        private double _dx = 0.15;
        public double dx
        {
            get => _dx;
            set
            {
                var old_dx = _dx;
                if (!Set(ref _dx, value)) return;

                foreach (var item in Antenna)
                {
                    var x = item.LocationX;
                    var i = x / old_dx;
                    item.LocationX = i * value;
                }

                UpdateBeamforming();
            }
        }

        private double _dy = 0.15;
        public double dy
        {
            get => _dy;
            set
            {
                var old_dy = _dy;
                if (!Set(ref _dy, value)) return;

                foreach (var item in Antenna)
                {
                    var y = item.LocationY;
                    var j = y / old_dy;
                    item.LocationY = j * value;
                }

                UpdateBeamforming();
            }
        }

        [DependencyOn(nameof(Nx)), DependencyOn(nameof(dx))]
        public double AperturaLengthX => _dx * _Nx;
        //{
        //    get
        //    {
        //        Antenna.Select(item => item.LocationX).GetMinMax(v => v, out var min, out var max);
        //        return max - min;
        //    }
        //}

        [DependencyOn(nameof(Ny)), DependencyOn(nameof(dy))]
        public double AperturaLengthY => _dy * _Ny;
        //{
        //    get
        //    {
        //        Antenna.Select(item => item.LocationY).GetMinMax(v => v, out var min, out var max);
        //        return max - min;
        //    }
        //}

        private double _th0 = 0;
        public double th0
        {
            get => _th0;
            set
            {
                if (!Set(ref _th0, value)) return;
            }
        }

        private double _phi0 = 0;
        public double phi0
        {
            get => _phi0;
            set
            {
                if (!Set(ref _phi0, value)) return;
            }
        }


        private DigitalSignal _OutSignal;

        public DigitalSignal OutSignal
        {
            get => _OutSignal;
            private set
            {
                if (!Set(ref _OutSignal, value)) return;

                var values = OutSignalValues;
                values.Clear();
                if (value is null) return;

                var dt = value.dt * 1e9;
                for (var i = 0; i < value.SamplesCount; i++)
                    values.Add(new DataPoint
                    {
                        X = dt * i,
                        Y = value[i]
                    });
            }
        }

        public class DataPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        public ObservableCollection<DataPoint> OutSignalValues { get; } = new ObservableCollection<DataPoint>();

        public RadioScene Sources { get; } = new RadioScene();

        private int _SamplesCount;

        public int SamplesCount
        {
            get => _SamplesCount;
            //set
            //{
            //    if(!Set(ref _SamplesCount, value)) return;
            //    Antenna.SamplesCount = value;
            //}
        }

        public DigitalAntennaArray2 Antenna { get; }

        private PatternValue[] _Pattern;

        public PatternValue[] Pattern
        {
            get => _Pattern;
            set => Set(ref _Pattern, value);
        }

        private double _PatternCalculationProgress;

        public double PatternCalculationProgress
        {
            get => _PatternCalculationProgress;
            set => Set(ref _PatternCalculationProgress, value);
        }

        private double _PatternMaximum;

        public double PatternMaximum
        {
            get => _PatternMaximum;
            private set => Set(ref _PatternMaximum, value);
        }

        private bool _NormPattern;

        public bool NormPattern
        {
            get => _NormPattern;
            set
            {
                if (Set(ref _NormPattern, value))
                    ComputeOutputSignalAsync();
            }
        }

        private double _Phi = 0;

        public double Phi
        {
            get => _Phi;
            set => Set(ref _Phi, value);
        }

        public double SNR { get; private set; }

        //public IEnumerable<IAntenna> KnownAntennaElements => new IAntenna[]
        //{
        //    _AntennaItem,
        //    new Uniform(),
        //    new CosElement(),
        //    new Cos2Element(),
        //    new GuigensElement(),
        //    new Vibrator()
        //};

        public IEnumerable<SignalFunction> KnownFunctions =>
            new SignalFunction[]
            {
                new SinSignal(1, 1e9),
                new CosSignal(1, 1e9),
                new RandomSignal(),
                new LFM(1e9, 2e9, 60e-9, 0),
                new RectSignalFunction(60e-9, 120e-9),
            };

        public ICommand AddNewSourceCommand { get; }

        public ICommand RemoveSourceCommand { get; }

        public MainWindow3ViewModel()
        {
            _SamplesCount = 16;
            var antenna = new DigitalAntennaArray2(_SamplesCount);

            _AntennaItem = new UniformAntenna();
            const double fd = 8e9; // Hz
            const double max_amplidude = 5;
            _ADC = new ADC(16, fd, max_amplidude);
            for (var ix = 0; ix < _Nx; ix++)
                for (var iy = 0; iy < _Ny; iy++)
                {
                    var location = new Vector3D(ix * _dx, iy * _dy);
                    antenna.Add(_AntennaItem, location, _ADC);
                }

            Antenna = antenna;
            UpdateBeamforming();

            AddNewSourceCommand = new LamdaCommand(AddNewCommandExecuted);
            RemoveSourceCommand = new LamdaCommand(RemoveSourceCommandExecuted, p => p is SpaceSignal source && Sources.Contains(source));
            Sources.CollectionChanged += OnRadioSceneChanged;
            antenna.PropertyChanged += OnAntennaPaarmeterChanged;

            Sources.Add(new SpaceSignal { Signal = new SinSignal(1, 1e9) });
        }

        private void UpdateBeamforming()
        {
            Antenna.BeamForming = new MatrixBeamForming(Antenna.Select(item => item.Location).ToArray(), _SamplesCount, _ADC.Fd);
        }

        private void AddNewCommandExecuted(object Obj) => Sources.Add(new SpaceSignal { Signal = new SinSignal(1, 1e9) });

        private void RemoveSourceCommandExecuted(object Obj)
        {
            if (!(Obj is SpaceSignal source))
                return;

            Sources.Remove(source);
        }

        private void OnRadioSceneChanged(object Sender, NotifyCollectionChangedEventArgs E)
        {
            void OnSourceParameterChanged(object s, PropertyChangedEventArgs e) => ComputeOutputSignalAsync();

            switch (E.Action)
            {
                default: throw new ArgumentOutOfRangeException();
                case NotifyCollectionChangedAction.Add:
                    foreach (SpaceSignal source in E.NewItems)
                        source.PropertyChanged += OnSourceParameterChanged;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (SpaceSignal source in E.OldItems)
                        source.PropertyChanged -= OnSourceParameterChanged;
                    break;
            }
            ComputeOutputSignalAsync();
        }

        private void OnAntennaPaarmeterChanged(object Sender, PropertyChangedEventArgs E) => ComputeOutputSignalAsync();


        private CancellationTokenSource _ComputeOutputSignalCancellation;
        private async void ComputeOutputSignalAsync()
        {
            var cancellation = new CancellationTokenSource();
            Interlocked.Exchange(ref _ComputeOutputSignalCancellation, cancellation)?.Cancel();
            var cancel = cancellation.Token;
            try
            {
                await TaskEx.YieldAsync();

                var radio_scene = Sources;


                var signal = Antenna.GetSignal(radio_scene);

                //var s0 = radio_scene.FirstOrDefault(s => !(s.Signal is RandomSignal))?.Signal;

                //if (s0 is null)
                //    SNR = double.NaN;
                //else
                //{
                //    var sum = 0d;
                //    var N = Antenna.ElementsCount;
                //    for (var i = 0; i < signal.Count; i++)
                //    {
                //        var v = s0.Value(signal[i].t) - signal[i].Value / N;
                //        sum += v * v;
                //    }

                //    SNR = -10 * Math.Log10(sum);
                //}

                await Application.Current.Dispatcher;
                OnPropertyChanged(nameof(SNR));

                OutSignal = signal;

                const double thetta_min = -90;
                const double thetta_max = 90;
                const double d_thetta = 0.5;
                const int beam_samples_count = (int)((thetta_max - thetta_min) / d_thetta) + 1;
                var pattern = new List<PatternValue>(beam_samples_count);

                await TaskEx.YieldAsync();

                var phi = _Phi;
                for (var thetta = thetta_min; thetta <= thetta_max; thetta += d_thetta)
                {
                    cancel.ThrowIfCancellationRequested();
                    pattern.Add(new PatternValue
                    {
                        Angle = thetta,
                        Value = Antenna.GetSignal(radio_scene.Rotate(thetta, phi, AngleType.Deg)).GetTotalPower()//.GetOutSignal(radio_scene, thetta * toRad).P.Power
                    });

                    PatternCalculationProgress = (double)pattern.Count / pattern.Capacity;
                }

                var max = pattern.Max(v => v.Value);

                await Application.Current.Dispatcher;

                Pattern = (_NormPattern ? pattern.Select(v => v / max) : pattern).ToArray();
                PatternMaximum = 10 * Math.Log10(max);
            }
            catch (IndexOutOfRangeException) { } // Костыли
            catch (InvalidOperationException) { } // Костыли
            catch (ArgumentException) { } // Костыли
            catch (NullReferenceException) { } // Костыли
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
        }
    }
}
