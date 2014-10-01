﻿using NLog;
using NLog.Config;
using NLog.Targets;
using NServiceBus;

public class NLogConfig
{
    public void InCode()
    {
        #region NLogInCode

        var config = new LoggingConfiguration();

        var consoleTarget = new ColoredConsoleTarget
        {
            Layout = "${level:uppercase=true}|${logger}|${message}${onexception:${newline}${exception:format=tostring}}"
        };
        config.AddTarget("console", consoleTarget);
        config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));

        LogManager.Configuration = config;

        NServiceBus.Logging.LogManager.Use<NLogFactory>();

        #endregion
    }
    public void Filtering()
    {
        #region NLogFiltering

        var config = new LoggingConfiguration();

        var consoleTarget = new ColoredConsoleTarget();
        config.AddTarget("console", consoleTarget);
        config.LoggingRules.Add(new LoggingRule("MyNamespace.*", LogLevel.Debug, consoleTarget));

        LogManager.Configuration = config;

        NServiceBus.Logging.LogManager.Use<NLogFactory>();

        #endregion
    }
}