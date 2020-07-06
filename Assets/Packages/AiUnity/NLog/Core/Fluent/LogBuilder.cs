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
using System;
using AiUnity.NLog.Core.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;

#if AIUNITY_CODE
namespace AiUnity.NLog.Core.Fluent
{
    /// <summary>
    /// A fluent class to build log events for NLog.
    /// </summary>
    public class LogBuilder
    {
        private readonly LogEventInfo _logEvent;
        private readonly NLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogBuilder"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="NLogger"/> to send the log event.</param>
        public LogBuilder(NLogger logger)
            : this(logger, LogLevels.Debug)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogBuilder"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="NLogger"/> to send the log event.</param>
        /// <param name="logLevel">The <see cref="LogLevels"/> for the log event.</param>
        public LogBuilder(NLogger logger, LogLevels logLevel)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            //if (logLevel == null)
                //throw new ArgumentNullException("logLevel");

            _logger = logger;
            _logEvent = new LogEventInfo
            {
                Level = logLevel,
                LoggerName = logger.Name,
                TimeStamp = DateTime.Now
            };
        }

        /// <summary>
        /// Gets the <see cref="LogEventInfo"/> created by the builder.
        /// </summary>
        public LogEventInfo LogEventInfo
        {
            get { return _logEvent; }
        }

        /// <summary>
        /// Sets the <paramref name="exception"/> information of the logging event.
        /// </summary>
        /// <param name="exception">The exception information of the logging event.</param>
        /// <returns></returns>
        public LogBuilder Exception(Exception exception)
        {
            _logEvent.Exception = exception;
            return this;
        }

        /// <summary>
        /// Sets the level of the logging event.
        /// </summary>
        /// <param name="logLevel">The level of the logging event.</param>
        /// <returns></returns>
        public LogBuilder Level(LogLevels logLevel)
        {
            _logEvent.Level = logLevel;
            return this;
        }

        /// <summary>
        /// Sets the logger name of the logging event.
        /// </summary>
        /// <param name="loggerName">The logger name of the logging event.</param>
        /// <returns></returns>
        public LogBuilder LoggerName(string loggerName)
        {
            _logEvent.LoggerName = loggerName;
            return this;
        }

        /// <summary>
        /// Sets the log message on the logging event.
        /// </summary>
        /// <param name="message">The log message for the logging event.</param>
        /// <returns></returns>
        public LogBuilder Message(string message)
        {
            _logEvent.Message = message;

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formating on the logging event.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The object to format.</param>
        /// <returns></returns>
        public LogBuilder Message(string format, object arg0)
        {
            _logEvent.Message = format;
            _logEvent.Parameters = new[] { arg0 };

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formating on the logging event.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <returns></returns>
        public LogBuilder Message(string format, object arg0, object arg1)
        {
            _logEvent.Message = format;
            _logEvent.Parameters = new[] { arg0, arg1 };

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formating on the logging event.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <returns></returns>
        public LogBuilder Message(string format, object arg0, object arg1, object arg2)
        {
            _logEvent.Message = format;
            _logEvent.Parameters = new[] { arg0, arg1, arg2 };

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formating on the logging event.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <param name="arg3">The fourth object to format.</param>
        /// <returns></returns>
        public LogBuilder Message(string format, object arg0, object arg1, object arg2, object arg3)
        {
            _logEvent.Message = format;
            _logEvent.Parameters = new[] { arg0, arg1, arg2, arg3 };

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formating on the logging event.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public LogBuilder Message(string format, params object[] args)
        {
            _logEvent.Message = format;
            _logEvent.Parameters = args;

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formating on the logging event.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public LogBuilder Message(IFormatProvider provider, string format, params object[] args)
        {
            _logEvent.FormatProvider = provider;
            _logEvent.Message = format;
            _logEvent.Parameters = args;

            return this;
        }

        /// <summary>
        /// Sets a per-event context property on the logging event.
        /// </summary>
        /// <param name="name">The name of the context property.</param>
        /// <param name="value">The value of the context property.</param>
        /// <returns></returns>
        public LogBuilder Property(object name, object value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            _logEvent.Properties[name] = value;
            return this;
        }

        /// <summary>
        /// Sets the timestamp of the logging event.
        /// </summary>
        /// <param name="timeStamp">The timestamp of the logging event.</param>
        /// <returns></returns>
        public LogBuilder TimeStamp(DateTime timeStamp)
        {
            _logEvent.TimeStamp = timeStamp;
            return this;
        }

        /// <summary>
        /// Sets the stack trace for the event info.
        /// </summary>
        /// <param name="stackTrace">The stack trace.</param>
        /// <param name="userStackFrame">Index of the first user stack frame within the stack trace.</param>
        /// <returns></returns>
        public LogBuilder StackTrace(StackTrace stackTrace, int userStackFrame)
        {
            _logEvent.SetStackTrace(stackTrace, userStackFrame);
            return this;
        }

        /// <summary>
        /// Writes the log event to the underlying logger.
        /// </summary>
        public void Write()
        {
            _logger.Log(_logEvent);
        }

        /// <summary>
        /// Writes the log event to the underlying logger.
        /// </summary>
        /// <param name="condition">If condition is true, write log event; otherwise ignore event.</param>
        public void WriteIf(Func<bool> condition)
        {
            if (condition == null || !condition())
                return;

            _logger.Log(_logEvent);
        }

        /// <summary>
        /// Writes the log event to the underlying logger.
        /// </summary>
        /// <param name="condition">If condition is true, write log event; otherwise ignore event.</param>
        public void WriteIf(bool condition)
        {
            if (!condition)
                return;

            _logger.Log(_logEvent);
        }
    }
}
#endif
