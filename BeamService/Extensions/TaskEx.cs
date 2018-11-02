using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    public static class TaskEx
    {
        public static YieldAsyncAwaitable YieldAsync() => new YieldAsyncAwaitable();

        public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);
        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) => Task.WhenAll(tasks);

    }

    public struct YieldAsyncAwaitable
    {
        public YieldAsyncAwaiter GetAwaiter() => new YieldAsyncAwaiter();

        public struct YieldAsyncAwaiter : ICriticalNotifyCompletion, INotifyCompletion
        {
            private static readonly WaitCallback sf_WhaitCallBack = RunAction;

            public bool IsCompleted => false;

            private static void RunAction(object action) => ((Action) action)();

            private static void QueueContinuation(Action action, bool FlowContext)
            {    
                if (FlowContext)
                    ThreadPool.QueueUserWorkItem(sf_WhaitCallBack, action);
                else
                    ThreadPool.UnsafeQueueUserWorkItem(sf_WhaitCallBack, action);
            }

            public void OnCompleted(Action continuation) => QueueContinuation(continuation, true);

            public void UnsafeOnCompleted(Action continuation) => QueueContinuation(continuation, false);

            public void GetResult() { }
        }
    }
}
