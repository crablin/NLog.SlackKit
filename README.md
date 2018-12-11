NLog.Slack
=========

[![NuGet](https://img.shields.io/nuget/v/NLog.SlackKit.svg)](https://www.nuget.org/packages/NLog.SlackKit/)

## Overview

Inspired by [NLogToSlack](https://github.com/cyrilgandon/NLogToSlack).

Support .NET Core 2.0 and .NET Framework 4.6

## Features
- Post message to Slack
- Async to send without missing any log.

## Usage
1. Add **Incoming WebHooks** to slack team.
2. Generate a Webhook URL.
3. Copy URL to NLog target setting.

### nlog.config
``` xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <extensions>
    <add assembly="NLog.SlackKit" />
  </extensions>

  <targets>
    <target xsi:type="Slack"
            async="true"
            name="SlackNotify"
            layout="${message}"
            webHookUrl="https://hooks.slack.com/services/XXXX"
            channel="#log"
            username="Slack Kit"
            icon=":ghost:"/>
  </targets>
  

  <rules>
    <logger name="*" minlevel="Debug" writeTo="SlackNotify" />
  </rules>
</nlog>
```

- **async** : use async post NLog message to slack.
- **webHookUrl** : grad your Webhook Url
- **channel** : The channel name (e.g. #log) or user (e.g. @crab) to send NLog message.
- **username** : Name of the Slackbot.
- **icon** : Image of the Slackbot. you can use a URL or [Emoji](https://www.webpagefx.com/tools/emoji-cheat-sheet/)

### Extension Assembly

- Don't forget config extension 

``` xml
<extensions>
    <add assembly="NLog.SlackKit" />
</extensions>
```

## Async with safe thread 

Avoid post slack api is closed when thread is end. you must be code that.

``` C#
var logger = _factory.GetCurrentClassLogger();

logger.Info("message");

if (SlackLogQueue.WaitAsyncCompleted())
{
    //add flow on async post completed
}
```

## License

Copyright (c) 2018 Crab Lin