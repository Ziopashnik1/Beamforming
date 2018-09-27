using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public RadioScene Sources { get; } = new RadioScene();

        public ICommand AddNewSourceCommand { get; }

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
        }

        private void AddNewCommandExecuted(object Obj)
        {
            Sources.Add(new SpaceSignal { Signal = new SinSignal() });
        }
    }
}
