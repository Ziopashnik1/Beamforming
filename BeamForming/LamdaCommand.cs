using System;
using System.Windows.Input;

namespace BeamForming
{
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
