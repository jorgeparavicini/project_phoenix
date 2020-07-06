#if AIUNITY_CODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Config;
using System;
using AiUnity.NLog.Core.Common;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using AiUnity.NLog.Core.Time;
using AiUnity.Common.Extensions;
using AiUnity.Common.InternalLog;
using System.Linq;
using AiUnity.Common.Patterns;
using AiUnity.Common.Log;

namespace AiUnity.NLog.Core.Config
{
    [NLogConfigurationItem]
    public class UnityLogListener : ISupportsInitialize
    {
        private static IInternalLogger Logger { get { return NLogInternalLogger.Instance; } }

        void ISupportsInitialize.Close()
        {
            Logger.Info("Removing UnityLogListener");
            Application.logMessageReceived -= HandleLog;
        }

        void ISupportsInitialize.Initialize(LoggingConfiguration configuration)
        {
            Logger.Info("Adding UnityLogListener");
            Application.logMessageReceived += HandleLog;
        }

        void HandleLog(String message, String stackTrace, LogType type)
        {
            LogLevels logLevels = LogLevels.Debug;
            switch (type)
            {
                case LogType.Assert:
                    logLevels = LogLevels.Assert;
                    break;
                case LogType.Exception:
                    logLevels = LogLevels.Assert;
                    break;
                case LogType.Log:
                    logLevels = LogLevels.Debug;
                    break;
                case LogType.Error:
                    logLevels = LogLevels.Error;
                    break;
                case LogType.Warning:
                    logLevels = LogLevels.Warn;
                    break;
            }

            LogEventInfo logEventInfo = this.CreateLogEventInfo(logLevels, message, null);
            NLogger logger = NLogManager.Instance.GetLogger(logEventInfo.LoggerName, logEventInfo.Context, logEventInfo.FormatProvider);
            logger.Log(logEventInfo);
        }

        public List<StackFrame> stackFrames;

        /// <summary>
        /// Create the log event
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">The name of the logger.</param>
        /// <param name="message">The log message.</param>
        /// <param name="arguments">The log parameters.</param>
        /// </summary>
        protected virtual LogEventInfo CreateLogEventInfo(LogLevels logLevel, string message, object[] arguments)
        {
            string loggerName = "Unity";

            try
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame stackFrame = stackTrace.GetFrames()
                    .SkipWhile(s => s.GetMethod().DeclaringType.Namespace.StartsWith("UnityEngine") || s.GetMethod().DeclaringType.Name.Equals("UnityLogListener"))
                    .FirstOrDefault();

                if (stackFrame != null)
                {
                    Type logType = stackFrame.GetMethod().DeclaringType;

                    if (logType.Namespace.StartsWith("AiUnity.NLog.Core") || logType.Namespace.StartsWith("AiUnity.CLog.Core") || logType.Namespace.StartsWith("AiUnity.Common"))
                    {
                        return null;
                    }
                    loggerName = logType.FullName;
                }
            }
            catch
            {
                loggerName = "Unity";
            }

            var logEventInfo = new LogEventInfo(logLevel, loggerName, null, null, message, arguments);

            logEventInfo.TimeStamp = TimeSource.Current.Time;
            logEventInfo.UnityLogListener = this;
            logEventInfo.FromUnityLogListener = true;

            return logEventInfo;
        }

    }
}

#endif