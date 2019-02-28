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
using MathService.ViewModels;

namespace BeamForming
{
    internal class MainWindow3ViewModel : ViewModel
    {
        private SamplesSignal f_OutSignal;

        public SamplesSignal OutSignal
        {
            get => f_OutSignal;
            set => Set(ref f_OutSignal, value);
        }

        public RadioScene Sources { get; } = new RadioScene();

        public DigitalAntennaArray Antenna { get; } = new DigitalAntennaArray(16, 0.15, 8e9, 8, 16, 5);

        private PatternValue[] f_Pattern;

        public PatternValue[] Pattern
        {
            get => f_Pattern;
            set => Set(ref f_Pattern, value);
        }

        private double f_PatternCalculationProgress;

        public double PatternCalculationProgress
        {
            get => f_PatternCalculationProgress;
            set => Set(ref f_PatternCalculationProgress, value);
        }

        private double f_PatternMaximum;

        public double PatternMaximum
        {
            get => f_PatternMaximum;
            private set => Set(ref f_PatternMaximum, value);
        }

        private bool f_NormPattern;

        public bool NormPattern
        {
            get => f_NormPattern;
            set
            {
                if (Set(ref f_NormPattern, value))
                    ComputeOutputSignalAsync();
            }
        }

        public double SNR { get; private set; }

        public IEnumerable<IAntenna> KnownAntennaElements => new[]
        {
            Antenna.Element,
            new Uniform(),
            new CosElement(),
            new Cos2Element(),
            new GuigensElement(),
            new Vibrator()
        };

        public IEnumerable<SignalFunction> KnownFunctions => new SignalFunction[]
        {
            new SinSignal(1, 1e9), 
            new CosSignal(1, 1e9),
            new RandomSignal(), 
        };

        public ICommand AddNewSourceCommand { get; }

        public ICommand RemoveSourceCommand { get; }

        public MainWindow3ViewModel()
        {
            //for (var i = 0; i < 18; i++)
            //{
            //    switch (i % 3)
            //    {
            //        case 0:
            //            Sources.Add(new SpaceSignal { Signal = new SinSignal(), Thetta = i * 10 });
            //            break;
            //        case 1:
            //            Sources.Add(new SpaceSignal { Signal = new CosSignal(), Thetta = i * 10 });
            //            break;
            //        case 2:
            //            Sources.Add(new SpaceSignal { Signal = new RectSignalFunction(), Thetta = i * 10 });
            //            break;
            //    }
            //}

            AddNewSourceCommand = new LamdaCommand(AddNewCommandExecuted);
            RemoveSourceCommand = new LamdaCommand(RemoveSourceCommandExecuted, p => p is SpaceSignal source && Sources.Contains(source));
            Sources.CollectionChanged += OnRadioSceneChanged;
            Antenna.PropertyChanged += OnAntennaPaarmeterChanged;

            Sources.Add(new SpaceSignal { Signal = new SinSignal(1, 1e9) });
            //ComputeOutputSignalAsync();
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


        private CancellationTokenSource f_ComputeOutputSignalCancellation;
        private async void ComputeOutputSignalAsync()
        {
            var cancellation = new CancellationTokenSource();
            Interlocked.Exchange(ref f_ComputeOutputSignalCancellation, cancellation)?.Cancel();
            var cancel = cancellation.Token;
            try
            {
                await TaskEx.YieldAsync();

                var radio_scene = Sources;


                var signal = (await Antenna.GetOutSignalAsync(radio_scene, cancel: cancel)).P;

                var s0 = radio_scene.FirstOrDefault(s => !(s.Signal is RandomSignal))?.Signal;

                if (s0 is null)
                    SNR = double.NaN;
                else
                {
                    var sum = 0d;
                    var N = Antenna.N;
                    for (var i = 0; i < signal.Count; i++)
                    {
                        var v = s0.Value(signal[i].t) - signal[i].Value / N;
                        sum += v * v;
                    }

                    SNR = -10 * Math.Log10(sum);
                }

                await Application.Current.Dispatcher;
                OnPropertyChanged(nameof(SNR));

                OutSignal = signal;

                const double thetta_min = -90;
                const double thetta_max = 90;
                const double d_thetta = 0.5;
                const int beam_samples_count = (int)((thetta_max - thetta_min) / d_thetta) + 1;
                const double toRad = Math.PI / 180;
                var pattern = new List<PatternValue>(beam_samples_count);

                await TaskEx.YieldAsync();

                for (var thetta = thetta_min; thetta <= thetta_max; thetta += d_thetta)
                {
                    cancel.ThrowIfCancellationRequested();
                    pattern.Add(new PatternValue
                    {
                        Angle = thetta,
                        Value =  Antenna.GetOutSignal(radio_scene, thetta * toRad).P.Power
                    });

                    PatternCalculationProgress = (double)pattern.Count / pattern.Capacity;
                }

                var max = pattern.Max(v => v.Value);

                await Application.Current.Dispatcher;

                Pattern = (f_NormPattern ? pattern.Select(v => v / max) : pattern).ToArray();
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
