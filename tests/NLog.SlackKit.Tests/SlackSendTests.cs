using System;
using Xunit;

namespace NLog.SlackKit.Tests
{
    public class SlackSendTests
    {
        private LogFactory _factory;

        public SlackSendTests()
        {
            _factory = LogManager.LoadConfiguration("NLog.config");
        }
        [Fact]
        public void Send()
        {
            var logger = _factory.GetCurrentClassLogger();

            logger.Info("Crab");

            //logger.Info("I test send INFO message");
            //logger.Debug("I test send DEBUG message");
            //logger.Warn("I test send WARN message");
            //logger.Fatal("I test send FATAL message");
            
        }
        
        
    }
}
