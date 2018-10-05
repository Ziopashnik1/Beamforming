using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BeamService;
using Xceed.Wpf.DataGrid;

namespace BeamForming
{
    class MainWindow3ViewModel : ViewModel
    {
        private DigitalSignal f_OutSignal;

        public DigitalSignal OutSignal
        {
            get => f_OutSignal;
            set => Set(ref f_OutSignal, value);
        }

        public RadioScene Sources { get; } = new RadioScene();

        public DigitalAntennaArray Antenna { get; } = new DigitalAntennaArray(16, 0.15, 8e9, 8, 16, 5);

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
        }

        private void AddNewCommandExecuted(object Obj)
        {
            Sources.Add(new SpaceSignal { Signal = new SinSignal(1, 1e9) });
        }

        private void RemoveSourceCommandExecuted(object Obj)
        {
            if(!(Obj is SpaceSignal source)) return;
            Sources.Remove(source);
        }

        private void OnRadioSceneChanged(object Sender, NotifyCollectionChangedEventArgs E)
        {
            switch (E.Action)
            {
                default: throw new ArgumentOutOfRangeException();
                case NotifyCollectionChangedAction.Add:
                    foreach (SpaceSignal source in E.NewItems) source.PropertyChanged += OnSourceParameterChanged;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (SpaceSignal source in E.OldItems) source.PropertyChanged -= OnSourceParameterChanged;
                    break;
                //case NotifyCollectionChangedAction.Replace: break;
                //case NotifyCollectionChangedAction.Move: break;
                //case NotifyCollectionChangedAction.Reset: break;
            }

            ComputeAutputSignalAsync();
        }

        private void OnSourceParameterChanged(object Sender, PropertyChangedEventArgs E) => ComputeAutputSignalAsync();


        private async void ComputeAutputSignalAsync()
        {
            await Task.Yield();
            var signal = Antenna.GetOutSignal(Sources);
            OutSignal = signal.P;
        }
    }
}
