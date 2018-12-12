using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NLog.SlackKit.DemoSite.Controllers
{
    [Route("api/[controller]")]
    public class SlackKitController : Controller
    {
        private ILogger<SlackKitController> _logger;

        public SlackKitController(ILogger<SlackKitController> logger)
        {
            _logger = logger;
        }

        // POST api/values
        [HttpPost("async")]
        public async Task Post([FromBody]string value)
        {
            _logger.LogInformation("I test send INFO message");
            _logger.LogDebug("I test send DEBUG message");
            _logger.LogWarning("I test send WARN message");
            _logger.LogCritical("I test send FATAL message");

            await SlackLogQueue.WaitAsyncCompleted();
        }

        [HttpPost("sync/100")]
        public void Post100([FromBody]string value)
        {
            var count = new List<int>();

            for (var i = 1; i <= 100; i++)
            {
                count.Add(i);
            }

            Parallel.ForEach(count, i =>
            {
                _logger.LogInformation($"Priint: sync {i} times");
            });
        }

        [HttpPost("sync")]
        public void Post2([FromBody]string value)
        {
            _logger.LogInformation("I test send INFO message");
            _logger.LogDebug("I test send DEBUG message");
            _logger.LogWarning("I test send WARN message");
            _logger.LogCritical("I test send FATAL message");

            //await SlackLogQueue.WaitAsyncCompleted();
        }
    }
}