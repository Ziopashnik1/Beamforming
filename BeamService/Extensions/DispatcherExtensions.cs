using System.Windows.Threading;

namespace System.Threading.Tasks
{
    public static class DispatcherExtensions
    {
        public static DispatcherAwaiter GetAwaiter(this Dispatcher dispatcher) => new DispatcherAwaiter(dispatcher);
    }
}