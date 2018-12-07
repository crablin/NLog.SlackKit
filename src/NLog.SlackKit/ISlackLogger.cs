using NLog.SlackKit.Models;

namespace NLog.SlackKit
{
    public interface ISlackLogger
    {
        Attachment ToAttachment(LogEventInfo info);
    }
}
