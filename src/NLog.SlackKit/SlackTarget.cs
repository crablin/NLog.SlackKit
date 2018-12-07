﻿using System;
using System.Diagnostics;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.SlackKit.Models;
using NLog.Targets;

namespace NLog.SlackKit
{
    [Target("Slack")]
    public class SlackTarget : TargetWithLayout
    {
        private readonly Process _currentProcess = Process.GetCurrentProcess();

        [RequiredParameter]
        public string WebHookUrl { get; set; }

        public SimpleLayout Channel { get; set; }

        public SimpleLayout Username { get; set; }

        public string Icon { get; set; }

        public bool ExcludeLevel { get; set; }

        protected override void InitializeTarget()
        {
            if (string.IsNullOrWhiteSpace(WebHookUrl))
            {
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL cannot be empty.");
            }

            if (!Uri.TryCreate(WebHookUrl, UriKind.Absolute, out Uri uriResult))
            {
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL is an invalid URL.");
            }

            if (!string.IsNullOrWhiteSpace(Channel.Text)
                && (!this.Channel.Text.StartsWith("#") && !Channel.Text.StartsWith("@") && !Channel.Text.StartsWith("${")))
            {
                throw new ArgumentOutOfRangeException("Channel", "The Channel name is invalid. It must start with either a # or a @ symbol or use a variable.");
            }

            base.InitializeTarget();
        }

        protected override void Write(AsyncLogEventInfo info)
        {
            try
            {
                this.SendToSlack(info);
            }
            catch (Exception e)
            {
                info.Continuation(e);
            }
        }

        private void SendToSlack(AsyncLogEventInfo info)
        {
            var message = Layout.Render(info.LogEvent);
            var payload = new Payload()
            {
                Text = message
            };

            var channel = Channel.Render(info.LogEvent);
            if (!string.IsNullOrWhiteSpace(channel))
            {
                payload.Channel = channel;
            }

            if (!string.IsNullOrWhiteSpace(Icon))
            {
                payload.SetIcon(Icon);
            }

            string username = this.Username.Render(info.LogEvent);
            if (!string.IsNullOrWhiteSpace(username))
            {
                payload.Username = username;
            }

            if (!ExcludeLevel)
            {
                var mainAttachment = new Attachment
                {
                    Title = info.LogEvent.Level.ToString(),
                    Color = info.LogEvent.Level.ToSlackColor()
                };
                payload.Attachments.Add(mainAttachment);
            }
            if (info.LogEvent.Parameters != null)
            {
                foreach (var param in info.LogEvent.Parameters)
                {
                    if (param is ISlackLogger slackLogger)
                    {
                        var requestAttachment = slackLogger.ToAttachment(info.LogEvent);
                        payload.Attachments.Add(requestAttachment);
                    }
                }
            }

            var exception = info.LogEvent.Exception;
            if (exception != null)
            {
                var attachment = new Attachment
                {
                    Title = exception.Message,
                    Color = LogLevel.Error.ToSlackColor()
                };

                attachment.Fields.Add(new Field
                {
                    Title = "Type",
                    Value = exception.GetType().FullName,
                    Short = true
                });

                if (!string.IsNullOrWhiteSpace(exception.StackTrace))
                {
                    attachment.Text = exception.StackTrace;
                }
                payload.Attachments.Add(attachment);
            }

            payload.SendTo(WebHookUrl);
        }
    }
}
