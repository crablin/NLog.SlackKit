using NLog.Layouts;

namespace NLog.SlackKit
{
    public static class NLogExtentions
    {
        /// <summary>
        /// Return a color string from the level of an NLog message
        /// </summary>
        /// <param name="level">The NLog message level</param>
        /// <returns>A string that can be one of [good, warning, danger], or grey #cccccc</returns>
        public static string ToSlackColor(this LogLevel level)
        {
            switch (level.Name.ToLowerInvariant())
            {
                case "warn":
                    return "warning";

                case "error":
                case "fatal":
                    return "danger";

                case "info":
                    return "good";

                default:
                    return "#cccccc";
            }
        }

        /// <summary>
        /// Get SimpleLayout the value (default is null).
        /// </summary>
        /// <param name="layout">The layout.</param>
        /// <param name="eventInfo">The event information.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string RenderValue(this SimpleLayout layout, LogEventInfo eventInfo, string defaultValue = null)
        {
            string value = layout.Render(eventInfo);

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value;
        }
    }
}
