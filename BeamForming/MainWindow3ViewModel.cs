using BeamService;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Antennas;
using BeamService.Functions;
using DSP.Lib;
using MathService;
using MathService.Vectors;
using MathService.ViewModels;
using PatternValue = BeamService.PatternValue;

namespace BeamForming
{
    internal class MainWindow3ViewModel : ViewModel
    {
        private DigitalSignal _OutSignal;

        public DigitalSignal OutSignal
        {
            get => _OutSignal;
            set => Set(ref _OutSignal, value);
        }

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

        //public IEnumerable<IAntenna> KnownAntennaElements => new[]
        //{
        //    Antenna.Element,
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
            _SamplesCount = 128;
            var antenna = new DigitalAntennaArray2(_SamplesCount);
            var antenna_item = new UniformAntenna();
            const double fd = 8e9; // Hz
            const double max_amplidude = 5;
            var adc = new ADC(16, fd, max_amplidude);
            const int antennas_count_x = 16;
            const int antennas_count_y = 16;
            const double dx = 0.15; // m
            const double dy = 0.15; // m
            for (var ix = 0; ix < antennas_count_x; ix++)
            for (var iy = 0; iy < antennas_count_y; iy++)
            {
                var location = new Vector3D(ix * dx, iy * dy);
                antenna.Add(antenna_item, location, adc);
            }

            Antenna = antenna;

            AddNewSourceCommand = new LamdaCommand(AddNewCommandExecuted);
            RemoveSourceCommand = new LamdaCommand(RemoveSourceCommandExecuted, p => p is SpaceSignal source && Sources.Contains(source));
            Sources.CollectionChanged += OnRadioSceneChanged;
            antenna.PropertyChanged += OnAntennaPaarmeterChanged;

            Sources.Add(new SpaceSignal { Signal = new SinSignal(1, 1e9) });
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
                        Value =  Antenna.GetSignal(radio_scene.Rotate(thetta, phi, AngleType.Deg)).GetTotalPower()//.GetOutSignal(radio_scene, thetta * toRad).P.Power
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
