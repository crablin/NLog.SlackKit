using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NLog.SlackKit.Tests
{
    [TestClass]
    public class SlackSendTests
    {
        private LogFactory _factory;

        [TestInitialize]
        public void InitSlackSendTests()
        {
            _factory = LogManager.LoadConfiguration("NLog.config");
        }

        [TestMethod]
        public void Send()
        {
            var logger = _factory.GetCurrentClassLogger();

            logger.Info("I test send INFO message");
            logger.Debug("I test send DEBUG message");
            logger.Warn("I test send WARN message");
            logger.Fatal("I test send FATAL message");

            if (SlackLogQueue.WaitAsyncCompleted())
            {
                Console.Write("Ok");
            }
            
        }

        [TestMethod]
        public void Send100()
        {
            var logger = _factory.GetCurrentClassLogger();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var count = new List<int>();

            for (var i = 1; i <= 10; i++)
            {
                count.Add(i);
                //logger.Info($"Priint: sync {i} times");
            }

            Parallel.ForEach(count, i =>
            {
                logger.Info($"Priint: sync {i} times");
            });

            stopwatch.Stop();

            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);

            if (SlackLogQueue.WaitAsyncCompleted())
            {
                Console.WriteLine("Ok");
            }
        }
    }
}