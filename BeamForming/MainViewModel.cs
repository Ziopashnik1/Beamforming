using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeamForming
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private string f_WindowTitle = "Заголовок";

        public string WindowTitle
        {
            get { return f_WindowTitle; }
            set
            {
                f_WindowTitle = value;
                OnPropertyChanged("WindowTitle");
            }
        }

        private BeamPatternValue[] f_Pattern;

        public BeamPatternValue[] Pattern
        {
            get { return f_Pattern; }
            set
            {
                f_Pattern = value;
                OnPropertyChanged(nameof(Pattern));
            }
        }

        public ICommand RecalculatePattern { get; }

        public MainViewModel()
        {
            f_Pattern = new BeamPatternValue[181];
            for (int i = 0; i < f_Pattern.Length; i++)
            {
                f_Pattern[i] = new BeamPatternValue
                {
                    Angle = i - 90,
                    Value = Math.Cos((i - 90) * Math.PI / 180)
                };
            }

            var a = 0;
            RecalculatePattern = new LamdaCommand(p =>
            {
                a++;
                var pattern = new BeamPatternValue[181];
                for (int i = 0; i < f_Pattern.Length; i++)
                {
                    pattern[i] = new BeamPatternValue
                    {
                        Angle = i - 90 + a,
                        Value = Math.Cos((i - 90 + a) * Math.PI / 180)
                    };
                }
                Pattern = pattern;
            }, p => true);
        }

    }

    public struct BeamPatternValue
    {
        public double Angle { get; set; }
        public double Value { get; set; }

        public override string ToString()
        {
            return Angle.ToString() + " : " + Value.ToString();
        }
    }

    public class LamdaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action<object> f_OnExecute;
        private readonly Func<object, bool> f_OnCanExecute;

        public LamdaCommand(Action<object> OnExecute, Func<object, bool> OnCanExecute)
        {
            f_OnExecute = OnExecute;
            f_OnCanExecute = OnCanExecute;
        }

        public bool CanExecute(object parameter)
        {
            return f_OnCanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            f_OnExecute(parameter);
        }
    }
}
