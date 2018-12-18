using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace System.Threading.Tasks
{
    public struct DispatcherAwaiter : INotifyCompletion
    {
        private readonly Dispatcher f_Dispatcher;

        public bool IsCompleted => f_Dispatcher.CheckAccess();

        public DispatcherAwaiter(Dispatcher dispatcher) => f_Dispatcher = dispatcher;

        public void OnCompleted(Action continuation) => f_Dispatcher.Invoke(continuation);

        public void GetResult() { }
    }
}
