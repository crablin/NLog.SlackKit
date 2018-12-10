using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NLog.SlackKit
{
    public class SlackLogQueue
    {
        internal static ConcurrentDictionary<int, StrongBox<int>> Counter = new ConcurrentDictionary<int, StrongBox<int>>();

        public static bool WaitAsyncCompleted(int timeoutOfSecond = 30)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (Counter[threadId].Value > 0)
            {
                if (stopwatch.Elapsed > TimeSpan.FromSeconds(timeoutOfSecond))
                {
                    Counter.TryRemove(threadId, out StrongBox<int> _);
                    return false;
                }

                Thread.Sleep(100);
            }

            stopwatch.Stop();

            return true;
        }
    }
}
