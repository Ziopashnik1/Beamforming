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
using System.Windows.Threading;
using Antennas;
using BeamService.AmplitudeDestributions;
using BeamService.Digital;
using BeamService.Functions;
using DSP.Lib;
using MathCore;
using MathCore.Vectors;
using MathCore.ViewModels;
using Antenna = Antennas.Antenna;
using PatternValue = BeamService.PatternValue;
using Vibrator = Antennas.Vibrator;

namespace BeamForming
{
    internal class MainWindow3ViewModel : ViewModel
    {
        public IReadOnlyCollection<AmplitudeDestribution> KnownAmplitudeDestributions { get; } = new AmplitudeDestribution[]
        {
            new Uniform(), 
            new CosOnPedestal(), 
        };

        private AmplitudeDestribution _AmplitudeDestributionX = new Uniform();

        public AmplitudeDestribution AmplitudeDestributionX
        {
            get => _AmplitudeDestributionX;
            set
            {
                if(!Set(ref _AmplitudeDestributionX, value)) return;
                ComputeOutputSignalAsync();
            }
        }

        private AmplitudeDestribution _AmplitudeDestributionY = new Uniform();

        public AmplitudeDestribution AmplitudeDestributionY
        {
            get => _AmplitudeDestributionY;
            set
            {
                if (!Set(ref _AmplitudeDestributionY, value)) return;
                ComputeOutputSignalAsync();
            }
        }

        private ADC _ADC;

        public ADC ADC
        {
            get => _ADC;
            set
            {
                if(!Set(ref _ADC, value ?? throw new ArgumentNullException(nameof(value)))) return;
                UpdateBeamforming();
                ComputeOutputSignalAsync();
            }
        }

        private Antenna _AntennaItem;

        public Antenna AntennaItem
        {
            get => _AntennaItem;
            set
            {
                if (!Set(ref _AntennaItem, value ?? throw new ArgumentNullException(nameof(value)))) return;
                foreach (var antenna in Antenna) antenna.Element = value;
                ComputeOutputSignalAsync();
            }
        }

        public IReadOnlyCollection<Antenna> KnownAntennaItems { get; } = new Antenna[]
        {
            new UniformAntenna(), 
            new Dipole(),
            new Guigens(), 
            new Vibrator(),
        };

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
                        for (var i = 0; i < new_count; i++)
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
                ComputeOutputSignalAsync();
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
                        for (var j = 0; j < new_count; j++)
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
                ComputeOutputSignalAsync();
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
                ComputeOutputSignalAsync();
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
                ComputeOutputSignalAsync();
            }
        }

        [DependencyOn(nameof(Nx)), DependencyOn(nameof(dx))]
        public double AperturaLengthX => _dx * _Nx;

        [DependencyOn(nameof(Ny)), DependencyOn(nameof(dy))]
        public double AperturaLengthY => _dy * _Ny;

        #region th0 : double - Угол отклонения луча ДН по углу места

        ///<summary>Угол отклонения луча ДН по углу места</summary>
        private double _th0;

        ///<summary>Угол отклонения луча ДН по углу места</summary>
        public double th0
        {
            get => _th0;
            set
            {
                if(!Set(ref _th0, value)) return;
                UpdateBeamforming();
                ComputeOutputSignalAsync();
            }
        }

        #endregion

        #region phi0 : double - Угол отклонения луча ДН по азимуту

        ///<summary>Угол отклонения луча ДН по азимуту</summary>
        private double _phi0;

        ///<summary>Угол отклонения луча ДН по азимуту</summary>
        public double phi0
        {
            get => _phi0;
            set
            {
                if(!Set(ref _phi0, value)) return;
                UpdateBeamforming();
                ComputeOutputSignalAsync();
            }
        }

        #endregion

        #region fd : double - Частота дискретизации

        private double _fd;

        ///<summary>Частота дискретизации</summary>
        public double fd
        {
            get => _fd;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), value, "Частота должна быть больше 0");
                if (!Set(ref _fd, value)) return;

                _ADC.Fd = value;
                UpdateBeamforming();
                ComputeOutputSignalAsync();
            }
        }

        #endregion

        #region Nd : int - Размер выборки

        private int _Nd;

        ///<summary>Размер выборки</summary>
        public int Nd
        {
            get => _Nd;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), value, "Размер выборки должен быть больше 0");
                if (!Set(ref _Nd, value)) return;

                Antenna.SamplesCount = value;

                UpdateBeamforming();
                ComputeOutputSignalAsync();
            }
        }

        #endregion

        #region df : double - Частотная разрешающая способность

        ///<summary>Частотная разрешающая способность</summary>
        [DependencyOn(nameof(fd)), DependencyOn(nameof(Nd))]
        public double df => fd / Nd;

        #endregion

        #region n : int - Разрядность кода

        private int _n = 16;

        ///<summary>Разрядность кода</summary>
        public int n
        {
            get => _n;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), value, "Разрядность кода должна быть больше 0");
                if (!Set(ref _n, value)) return;

                _ADC.N = value;

                ComputeOutputSignalAsync();
            }
        }

        #endregion

        #region tj : double - Джиттер

        private double _tj;

        ///<summary>Джиттер</summary>
        public double tj
        {
            get => _tj;
            set => Set(ref _tj, value);
        }

        #endregion

        #region ADCmaxAmplitude : double - Максимальная амплитуда АЦП

        ///<summary>Максимальная амплитуда АЦП</summary>
        private double _AdCmaxAmplitude;

        ///<summary>Максимальная амплитуда АЦП</summary>
        public double ADCmaxAmplitude
        {
            get => _AdCmaxAmplitude;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), value, "Максимальная амплитуда АЦП должна быть больше 0");
                if (!Set(ref _AdCmaxAmplitude, value)) return;

                _ADC.MaxValue = value;

                ComputeOutputSignalAsync();
            }
        }

        #endregion

        private DigitalSignal _OutSignalI;
        private DigitalSignal _OutSignalQ;

        public DigitalSignal OutSignalI
        {
            get => _OutSignalI;
            private set
            {
                if (!Set(ref _OutSignalI, value)) return;

                var values = new ObservableCollection<DataPoint>();
                values.Clear();
                if (value is null) return;

                var dt = value.dt * 1e9;
                for (var i = 0; i < value.SamplesCount; i++)
                    values.Add(new DataPoint
                    {
                        X = dt * i,
                        Y = value[i]
                    });
                OutSignalValuesI = values;
            }
        }

        public DigitalSignal OutSignalQ
        {
            get => _OutSignalQ;
            private set
            {
                if (!Set(ref _OutSignalQ, value)) return;

                var values = new ObservableCollection<DataPoint>();
                if (value is null) return;

                var dt = value.dt * 1e9;
                for (var i = 0; i < value.SamplesCount; i++)
                    values.Add(new DataPoint
                    {
                        X = dt * i,
                        Y = value[i]
                    });
                OutSignalValuesQ = values;
            }
        }

        public class DataPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        private ObservableCollection<DataPoint> _OutSignalValuesI = new ObservableCollection<DataPoint>();

        public ObservableCollection<DataPoint> OutSignalValuesI
        {
            get => _OutSignalValuesI;
            set => Set(ref _OutSignalValuesI, value);
        }

        private ObservableCollection<DataPoint> _OutSignalValuesQ = new ObservableCollection<DataPoint>();

        public ObservableCollection<DataPoint> OutSignalValuesQ
        {
            get => _OutSignalValuesQ;
            set => Set(ref _OutSignalValuesQ, value);
        }

        public RadioScene Sources { get; } = new RadioScene();

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

        #region Phi : double - Азимут сечения ДН

        ///<summary>Азимут сечения ДН</summary>
        private double _Phi;

        ///<summary>Азимут сечения ДН</summary>
        public double Phi
        {
            get => _Phi;
            set
            {
                if (!Set(ref _Phi, value)) return;
                ComputeOutputSignalAsync();
            }
        }

        #endregion

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
                new RectSignalFunction(5e-10, 1e-9),
                new RadioSignalFunction(5e-10, 1e-9, 5e9), 
            };

        public ICommand AddNewSourceCommand { get; }

        public ICommand RemoveSourceCommand { get; }

        public MainWindow3ViewModel()
        {
            _Nd = 16;
            var antenna = new DigitalAntennaArray2(_Nd);

            _AntennaItem = new UniformAntenna();
            const double fd = 8e9; // Hz
            _fd = fd;
            const double max_amplidude = 5;
            _AdCmaxAmplitude = max_amplidude;
            _ADC = new ADC(_n, _fd, _AdCmaxAmplitude);
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
            Antenna.BeamForming = new MatrixBeamForming(Antenna.Select(item => item.Location).ToArray(), _Nd, _ADC.Fd)
            {
                PhasingАngle = new SpaceAngle(th0, phi0)
            };
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

                var destribution_x = _AmplitudeDestributionX.GetDestribution(
                    Antenna.Average(item => item.LocationX),
                    Antenna.Max(item => item.LocationX) - Antenna.Min(item => item.LocationX));
                var destribution_y = _AmplitudeDestributionY.GetDestribution(
                    Antenna.Average(item => item.LocationY),
                    Antenna.Max(item => item.LocationY) - Antenna.Min(item => item.LocationY));

                var signal = Antenna.GetSignal(radio_scene, destribution_x, destribution_y);

                await Application.Current.Dispatcher;
                OnPropertyChanged(nameof(SNR));

                OutSignalI = signal.I;
                OutSignalQ = signal.Q;

                const double theta_min = -90;
                const double theta_max = 90;
                const double d_theta = 0.5;
                const int beam_samples_count = (int)((theta_max - theta_min) / d_theta) + 1;
                var pattern = new List<PatternValue>(beam_samples_count);

                await TaskEx.YieldAsync();

                var phi = _Phi;
                for (var theta = theta_min; theta <= theta_max; theta += d_theta)
                {
                    cancel.ThrowIfCancellationRequested();
                    var (i, q) = Antenna.GetSignal(radio_scene.Rotate(-theta, -phi, AngleType.Deg), destribution_x, destribution_y);
                    pattern.Add(new PatternValue
                    {
                        Angle = theta,
                        Value = i.GetTotalPower() + q.GetTotalPower()
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
