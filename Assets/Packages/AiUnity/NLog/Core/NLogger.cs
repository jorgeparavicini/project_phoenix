// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 
#if AIUNITY_CODE

namespace AiUnity.NLog.Core
{
    using AiUnity.Common.Log;
    using Internal;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Provides logging interface and utility functions.
    /// </summary>
    /// <tags>NLogAPI</tags>
    public class NLogger : ILogger
    {
        #region Fields
        private readonly Type loggerType = typeof(NLogger);
        private volatile LoggerConfiguration configuration;
        private volatile bool isDebugEnabled = false;
        private volatile bool isErrorEnabled = false;
        private volatile bool isFatalEnabled = false;
        private volatile bool isInfoEnabled = false;
        private volatile bool isTraceEnabled = false;
        private volatile bool isWarnEnabled = false;
        #endregion

        #region Properties
        public UnityEngine.Object Context { get; set; }

        /// <summary>
        /// Gets the factory that created this logger.
        /// </summary>
        public LogFactory Factory { get; private set; }

        /// <summary>
        /// The FormatProvider if any associated with this loggers.
        /// </summary>
        public IFormatProvider FormatProvider { get; protected set; }

        /// <summary>
        /// Gets the name of the logger.
        /// </summary>
        public string Name { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when logger configuration changes.
        /// </summary>
        public event EventHandler<EventArgs> LoggerReconfigured;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NLogger"/> class.
        /// </summary>
        //protected internal Logger()
        //protected internal NLogger(UnityEngine.Object context = null)
        protected internal NLogger(UnityEngine.Object context = null, IFormatProvider formatProvider = null)
        {
            Context = context;
            FormatProvider = formatProvider;
        }
        #endregion

        #region Methods
        internal void Initialize(string name, LoggerConfiguration loggerConfiguration, LogFactory factory)
        {
            Name = name;
            Factory = factory;

            SetConfiguration(loggerConfiguration);
        }

        internal void SetConfiguration(LoggerConfiguration newConfiguration)
        {
            this.configuration = newConfiguration;

            // pre-calculate 'enabled' flags
            this.isTraceEnabled = newConfiguration.IsEnabled(LogLevels.Trace);
            this.isDebugEnabled = newConfiguration.IsEnabled(LogLevels.Debug);
            this.isInfoEnabled = newConfiguration.IsEnabled(LogLevels.Info);
            this.isWarnEnabled = newConfiguration.IsEnabled(LogLevels.Warn);
            this.isErrorEnabled = newConfiguration.IsEnabled(LogLevels.Error);
            this.isFatalEnabled = newConfiguration.IsEnabled(LogLevels.Fatal);

            var loggerReconfiguredDelegate = LoggerReconfigured;

            if (loggerReconfiguredDelegate != null)
            {
                loggerReconfiguredDelegate(this, new EventArgs());
            }
        }

        internal void WriteToTargets(LogLevels level, UnityEngine.Object context, IFormatProvider formatProvider, string message, object[] args, Exception exception = null)
        {
            LogEventInfo logEventInfo = LogEventInfo.Create(level, Name, context, formatProvider, message, args, exception);
            WriteToTargets(logEventInfo);
        }

        internal void WriteToTargets(LogEventInfo logEventInfo)
        {
            LoggerEngine.Write(this.loggerType, GetTargetsForLevel(logEventInfo.Level), logEventInfo, Factory);
        }

        private TargetWithFilterChain GetTargetsForLevel(LogLevels level)
        {
            return this.configuration.GetTargetsForLevel(level);
        }
        #endregion

        #region Level Checks

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Trace</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsTraceEnabled
        {
            get { return this.isTraceEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Debug</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Debug</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsDebugEnabled
        {
            get { return this.isDebugEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Info</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Info</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsInfoEnabled
        {
            get { return this.isInfoEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Warn</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Warn</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsWarnEnabled
        {
            get { return this.isWarnEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Error</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Error</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsErrorEnabled
        {
            get { return this.isErrorEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Fatal</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsFatalEnabled
        {
            get { return this.isFatalEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Assert</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsAssertEnabled
        {
            get { return IsEnabled(LogLevels.Assert); }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the specified level.
        /// </summary>
        /// <param name="level">Log level to be checked.</param>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the specified level, otherwise it returns <see langword="false" />.</returns>
        public bool IsEnabled(LogLevels level)
        {
            return GetTargetsForLevel(level) != null;
        }

        #endregion

        #region Log() overloads 
        /// <summary>
        /// Logs a diagnostic message specified by <see cref="LogEventInfo" /> .
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        public void Log(LogEventInfo logEvent)
        {
            if (IsEnabled(logEvent.Level))
            {
                WriteToTargets(logEvent);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Log</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        public void Log(LogLevels level, string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Log</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        public void Log(LogLevels level, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Log</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        public void Log(LogLevels level, Exception exception, string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Log</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        public void Log(LogLevels level, UnityEngine.Object context, Exception exception, string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Trace() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_TRACE")]
        [Conditional("UNITY_EDITOR")]
        public void Trace(string message, params object[] args)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevels.Trace, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_TRACE")]
        [Conditional("UNITY_EDITOR")]
        public void Trace(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevels.Trace, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_TRACE")]
        [Conditional("UNITY_EDITOR")]
        public void Trace(Exception exception, string message, params object[] args)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevels.Trace, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_TRACE")]
        [Conditional("UNITY_EDITOR")]
        public void Trace(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevels.Trace, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Debug() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_DEBUG")]
        [Conditional("UNITY_EDITOR")]
        public void Debug(string message, params object[] args)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevels.Debug, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_DEBUG")]
        [Conditional("UNITY_EDITOR")]
        public void Debug(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevels.Debug, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_DEBUG")]
        [Conditional("UNITY_EDITOR")]
        public void Debug(Exception exception, string message, params object[] args)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevels.Debug, Context, FormatProvider, message, args, exception);
            }
        }


        /// <summary>
        /// Logs a diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_DEBUG")]
        [Conditional("UNITY_EDITOR")]
        public void Debug(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevels.Debug, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Info() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_INFO")]
        [Conditional("UNITY_EDITOR")]
        public void Info(string message, params object[] args)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevels.Info, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_INFO")]
        [Conditional("UNITY_EDITOR")]
        public void Info(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevels.Info, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_INFO")]
        [Conditional("UNITY_EDITOR")]
        public void Info(Exception exception, string message, params object[] args)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevels.Info, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_INFO")]
        [Conditional("UNITY_EDITOR")]
        public void Info(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevels.Info, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Warn() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_WARN")]
        [Conditional("UNITY_EDITOR")]
        public void Warn(string message, params object[] args)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevels.Warn, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_WARN")]
        [Conditional("UNITY_EDITOR")]
        public void Warn(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevels.Warn, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_WARN")]
        [Conditional("UNITY_EDITOR")]
        public void Warn(Exception exception, string message, params object[] args)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevels.Warn, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_WARN")]
        [Conditional("UNITY_EDITOR")]
        public void Warn(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevels.Warn, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Error() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_ERROR")]
        [Conditional("UNITY_EDITOR")]
        public void Error(string message, params object[] args)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevels.Error, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_ERROR")]
        [Conditional("UNITY_EDITOR")]
        public void Error(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevels.Error, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_ERROR")]
        [Conditional("UNITY_EDITOR")]
        public void Error(Exception exception, string message, params object[] args)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevels.Error, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_ERROR")]
        [Conditional("UNITY_EDITOR")]
        public void Error(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevels.Error, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Fatal() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        //[Conditional("NLOG_ALL")]
        //[Conditional("NLOG_FATAL")]
        //[Conditional("UNITY_EDITOR")]
        public void Fatal(string message, params object[] args)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevels.Fatal, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_FATAL")]
        [Conditional("UNITY_EDITOR")]
        public void Fatal(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevels.Fatal, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_FATAL")]
        [Conditional("UNITY_EDITOR")]
        public void Fatal(Exception exception, string message, params object[] args)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevels.Fatal, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_FATAL")]
        [Conditional("UNITY_EDITOR")]
        public void Fatal(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevels.Fatal, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Assert() Overloads

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="test">Assert fires if test evaluates false.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(bool test, string message, params object[] args)
        {
            if (IsAssertEnabled && !test)
            {
                WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="test">Assert fires if test evaluates false.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(bool test, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsAssertEnabled && !test)
            {
                WriteToTargets(LogLevels.Assert, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(Exception exception, string message, params object[] args)
        {
            if (IsAssertEnabled)
            {
                WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsAssertEnabled)
            {
                WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="test">Assert fires if test evaluates false.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(Func<bool> test, string message, params object[] args)
        {
            if (IsAssertEnabled && !test())
            {
                WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="test">Assert fires if test evaluates false.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("NLOG_ALL")]
        [Conditional("NLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(Func<bool> test, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsAssertEnabled && !test())
            {
                WriteToTargets(LogLevels.Assert, context, FormatProvider, message, args);
            }
        }
        #endregion

    }
}
#endif