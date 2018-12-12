using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        private readonly int _currentProcessId = Process.GetCurrentProcess().Id;

        public bool Async { get; set; }

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
                var payload = GenerateSlackPayload(info);
                var json = payload.ToJson();
                
                SendTo((client) =>
                {
                    if (Async)
                    {
                        if (!SlackLogQueue.Counter.ContainsKey(_currentProcessId))
                        {
                            SlackLogQueue.Counter.TryAdd(_currentProcessId, new StrongBox<int>(0));
                        }

                        Interlocked.Increment(ref SlackLogQueue.Counter[_currentProcessId].Value);
                        client.UploadStringCompleted += Client_UploadStringCompleted;
                        client.UploadStringTaskAsync(new Uri(WebHookUrl), "POST", json).ConfigureAwait(true);
                    }
                    else
                    {
                        client.UploadString(WebHookUrl, "POST", json);
                    }
                });
            }
            catch (Exception e)
            {
                info.Continuation(e);

                if (Async)
                {
                    Interlocked.Decrement(ref SlackLogQueue.Counter[_currentProcessId].Value);
                }
            }
        }

        private void Client_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            Interlocked.Decrement(ref SlackLogQueue.Counter[_currentProcessId].Value);
        }

        private Payload GenerateSlackPayload(AsyncLogEventInfo info)
        {
            var message = Layout.Render(info.LogEvent);
            var payload = new Payload()
            {
                Text = message
            };

            payload.Channel = Channel.RenderValue(info.LogEvent);
            payload.Username = Username.RenderValue(info.LogEvent);

            if (!string.IsNullOrWhiteSpace(Icon))
            {
                payload.SetIcon(Icon);
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

            return payload;
        }

        /// <summary>
        /// Send this payload via a POST request to the given slack Webhook
        /// </summary>
        /// <param name="webHookUrl">The WebhookUrl where Payload will be POST</param>
        private void SendTo(Action<WebClient> action)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Encoding = Encoding.UTF8;
                client.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                action(client);
            }
        }
    }

}