using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace NLog.SlackKit
{
    public static class SlackLogQueue
    {
        internal static int QueueCount = 0;

        public static bool WaitAsyncCompleted(int timeoutOfSecond = 30)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (QueueCount > 0)
            {
                if (stopwatch.Elapsed > TimeSpan.FromSeconds(timeoutOfSecond))
                {
                    QueueCount = 0;
                    return false;
                }

                Thread.Sleep(100);
            }

            stopwatch.Stop();

            return true;
        }
    }
}
