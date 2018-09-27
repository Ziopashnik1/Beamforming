using System;
using System.Windows.Input;

namespace BeamForming
{
    public class LamdaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private readonly Action<object> f_OnExecute;
        private readonly Func<object, bool> f_OnCanExecute;

        public LamdaCommand(Action<object> OnExecute, Func<object, bool> OnCanExecute = null)
        {
            f_OnExecute = OnExecute;
            f_OnCanExecute = OnCanExecute;
        }

        public bool CanExecute(object Parameter) => f_OnCanExecute?.Invoke(Parameter) ?? true;

        public void Execute(object Parameter) => f_OnExecute(Parameter);
    }
}
