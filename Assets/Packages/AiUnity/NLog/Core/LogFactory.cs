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
using System.Linq;

namespace AiUnity.NLog.Core
{
    using AiUnity.Common.Extensions;
    using AiUnity.Common.InternalLog;
    using AiUnity.Common.Log;
    using AiUnity.NLog.Core.Common;
    using AiUnity.NLog.Core.Config;
    using AiUnity.NLog.Core.Internal;
    using AiUnity.NLog.Core.Targets;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
    /// </summary>
    public class LogFactory
    {
        #region Fields
        private static TimeSpan defaultFlushTimeout = TimeSpan.FromSeconds(15);
        private readonly Dictionary<LoggerCacheKey, WeakReference> loggerCache = new Dictionary<LoggerCacheKey, WeakReference>();
        private LoggingConfiguration config;
        private bool configLoaded;
        private LogLevels globalThreshold = NLogManager.GlobalLogLevelsDefault;
        private int logsEnabled;

        private const string DefaultConfig = @"<?xml version=""1.0"" encoding=""utf-8""?>
  <nlog buildLevels="""">
	<targets>
		<target name=""UnityConsole"" type=""UnityConsole""/>
	</targets>
	<rules>
		<logger name=""*"" namespace=""*"" target=""UnityConsole"" levels=""Fatal, Error, Warn""/>
	</rules>
  </nlog>";

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current logging configuration.
        /// </summary>
        public LoggingConfiguration Configuration
        {
            get
            {
                lock (this)
                {
                    if (this.configLoaded && this.config != null)
                    {
                        return this.config;
                    }
                    this.configLoaded = true;

                    // Finds editor/build config file in Resources folder
                    if (this.config == null)
                    {
                        string configText = NLogConfigFile.Instance.GetConfigText();

                        if (!string.IsNullOrEmpty(configText))
                        {
                            this.config = new XmlLoggingConfiguration(configText, NLogConfigFile.Instance.FileInfo.FullName);
                        }
                        else
                        {
                            Logger.Info("Using default configuration because unable to locate {0}.", NLogConfigFile.Instance.RelativeName);
                            this.config = new XmlLoggingConfiguration(DefaultConfig, NLogConfigFile.Instance.FileInfo.FullName);
                        }
                    }

                    if (this.config != null)
                    {
                        this.config.InitializeAll();
                    }

                    return this.config;
                }
            }
            set
            {
                lock (this)
                {
                    LoggingConfiguration oldConfig = this.config;
                    if (oldConfig != null)
                    {
                        Logger.Debug("Closing old configuration.");
                        Flush();
                        oldConfig.Close();
                    }

                    this.config = value;

                    if (this.config != null)
                    {
                        Logger.Debug("Establish new logging configuration.");
                        this.configLoaded = true;
                        Dump(this.config);

                        this.config.InitializeAll();
                        ReconfigExistingLoggers(this.config);
                    }
                    else
                    {
                        Logger.Debug("Logging Configuration deleted.");
                        this.configLoaded = false;
                    }

                    var configurationChangedDelegate = ConfigurationChanged;

                    if (configurationChangedDelegate != null)
                    {
                        configurationChangedDelegate(this, new LoggingConfigurationChangedEventArgs(oldConfig, value));
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the global log threshold. Log events below this threshold are not logged.
        /// </summary>
        public LogLevels GlobalThreshold
        {
            get { return this.globalThreshold; }

            set
            {
                if (this.globalThreshold != value)
                {
                    lock (this)
                    {
                        this.globalThreshold = value;
                        ReconfigExistingLoggers();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether exceptions should be thrown.
        /// </summary>
        /// <remarks>
        /// By default exceptions are not thrown under any circumstances.
        /// </remarks>
        public bool ThrowExceptions { get; set; }

        // Internal logger singleton
        private static IInternalLogger Logger { get { return NLogInternalLogger.Instance; } }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> changes.
        /// </summary>
        public event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged;

        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> gets reloaded.
        /// </summary>
        public event EventHandler<LoggingConfigurationReloadedEventArgs> ConfigurationReloaded;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LogFactory" /> class.
        /// </summary>
        public LogFactory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFactory" /> class.
        /// </summary>
        /// <param name="config">The config.</param>
        public LogFactory(LoggingConfiguration config)
            : this()
        {
            Configuration = config;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a logger that discards all log messages.
        /// </summary>
        /// <returns>Null logger instance.</returns>
        public NLogger CreateNullLogger()
        {
            //TargetWithFilterChain[] targetsByLevel = new TargetWithFilterChain[LogLevels.MaxLevel.Ordinal + 1];
            Dictionary<LogLevels, TargetWithFilterChain> targetsByLevel = new Dictionary<LogLevels, TargetWithFilterChain>();
            NLogger newLogger = new NLogger();
            newLogger.Initialize(string.Empty, new LoggerConfiguration(targetsByLevel), this);
            return newLogger;
        }

        /// <summary>Increases the log enable counter and if it reaches 0 the logs are disabled.</summary>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        /// than or equal to <see cref="DisableLogging"/> calls.</remarks>
        public void EnableLogging()
        {
            lock (this)
            {
                this.logsEnabled++;
                if (this.logsEnabled == 0)
                {
                    ReconfigExistingLoggers();
                }
            }
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        public void Flush()
        {
            Flush(defaultFlushTimeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(TimeSpan timeout)
        {
            try
            {
                AsyncHelpers.RunSynchronously(cb => Flush(cb, timeout));
            }
            catch (Exception e)
            {
                if (ThrowExceptions)
                {
                    throw;
                }
                Logger.Error(e.ToString());
            }
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(int timeoutMilliseconds)
        {
            Flush(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public void Flush(AsyncContinuation asyncContinuation)
        {
            Flush(asyncContinuation, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(AsyncContinuation asyncContinuation, int timeoutMilliseconds)
        {
            Flush(asyncContinuation, TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(AsyncContinuation asyncContinuation, TimeSpan timeout)
        {
            try
            {
                Logger.Trace("LogFactory.Flush({0})", timeout);

                var loggingConfiguration = Configuration;
                if (loggingConfiguration != null)
                {
                    Logger.Trace("Flushing all targets...");
                    loggingConfiguration.FlushAllTargets(AsyncHelpers.WithTimeout(asyncContinuation, timeout));
                }
                else
                {
                    asyncContinuation(null);
                }
            }
            catch (Exception e)
            {
                if (ThrowExceptions)
                {
                    throw;
                }
                Logger.Error(e.ToString());
            }
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public NLogger GetLogger(string name, UnityEngine.Object context = null, IFormatProvider formatProvider = null)
        {
            return GetLogger(name, typeof(NLogger), context, formatProvider);
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <param name="loggerType">The type of the logger to create. The type must inherit from NLog.Logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the 
        /// same argument aren't guaranteed to return the same logger reference.</returns>
        public NLogger GetLogger(string name, Type loggerType, UnityEngine.Object context = null, IFormatProvider formatProvider = null)
        {
            return GetLogger(new LoggerCacheKey(loggerType, name), context, formatProvider);
        }

        /// <summary>
        /// Returns <see langword="true" /> if logging is currently enabled.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is currently enabled, 
        /// <see langword="false"/> otherwise.</returns>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        /// than or equal to <see cref="DisableLogging"/> calls.</remarks>
        public bool IsLoggingEnabled()
        {
            return this.logsEnabled >= 0;
        }

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger and recalculates their target and filter list.
        /// Useful after modifying the configuration programmatically to ensure that all loggers have been properly configured.
        /// </summary>
        public void ReconfigExistingLoggers()
        {
            ReconfigExistingLoggers(this.config);
        }

        internal LoggerConfiguration GetConfigurationForLogger(string name, LoggingConfiguration configuration)
        {
            Dictionary<LogLevels, TargetWithFilterChain> targetsByLevel = new Dictionary<LogLevels, TargetWithFilterChain>();
            Dictionary<LogLevels, TargetWithFilterChain> lastTargetsByLevel = new Dictionary<LogLevels, TargetWithFilterChain>();

            Logger.Debug("Getting targets for {0} by level.", name);

            if (configuration != null && IsLoggingEnabled())
            {
                GetTargetsByLevelForLogger(name, configuration.LoggingRules, targetsByLevel, lastTargetsByLevel);
            }

            foreach (TargetWithFilterChain tfc in targetsByLevel.Values)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} =>", tfc);
                for (TargetWithFilterChain afc = tfc; afc != null; afc = afc.NextInChain)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", afc.Target.Name);
                    if (afc.FilterChain.Count > 0)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, " ({0} filters)", afc.FilterChain.Count);
                    }
                }
                Logger.Trace ("GetConfigurationForLogger: Chain = {0}", sb.ToString ());
            }

            return new LoggerConfiguration(targetsByLevel);
        }

        internal void GetTargetsByLevelForLogger(string name, IList<LoggingRule> rules, Dictionary<LogLevels, TargetWithFilterChain> targetsByLevel, Dictionary<LogLevels, TargetWithFilterChain> lastTargetsByLevel)
        {
            foreach (LoggingRule rule in rules)
            {
                if (!rule.NameMatches(name) || !rule.isPlatformMatch())
                {
                    continue;
                }

                //  The LogLevel conditional attributes replaces the need for GlobalThreshold
                //foreach (LogLevels logLevel in rule.logLevels.GetFlags().Where(l => GlobalThreshold.Has(l)))
                foreach (LogLevels logLevel in rule.logLevels.GetFlags())
                {
                    foreach (Target target in rule.Targets)
                    {
                        var awf = new TargetWithFilterChain(target, rule.Filters);

                        TargetWithFilterChain targetWithFilterChain;
                        if (lastTargetsByLevel.TryGetValue(logLevel, out targetWithFilterChain))
                        {
                            targetWithFilterChain.NextInChain = awf;
                        }
                        else
                        {
                            targetsByLevel[logLevel] = awf;
                        }
                        lastTargetsByLevel[logLevel] = awf;
                    }
                }

                GetTargetsByLevelForLogger(name, rule.ChildRules, targetsByLevel, lastTargetsByLevel);

                if (rule.Final)
                {
                    break;
                }
            }

            foreach (TargetWithFilterChain tfc in targetsByLevel.Values)
            {
                if (tfc != null)
                {
                    tfc.PrecalculateStackTraceUsage();
                }
            }
        }

        internal void ReconfigExistingLoggers(LoggingConfiguration configuration)
        {
            if (configuration != null)
            {
                configuration.EnsureInitialized();
            }

            foreach (var loggerWrapper in this.loggerCache.Values.ToList())
            {
                NLogger logger = loggerWrapper.Target as NLogger;
                if (logger != null)
                {
                    logger.SetConfiguration(GetConfigurationForLogger(logger.Name, configuration));
                }
            }
        }

        internal void ReloadConfig()
        {
            if (Configuration != null)
            {
                //this.configLoaded = false;
                Configuration = Configuration.Reload();

                if (ConfigurationReloaded != null)
                {
                    ConfigurationReloaded(this, new LoggingConfigurationReloadedEventArgs(true, null));
                }
            }
            else
            {
                Logger.Error("Unable to reload configuration {0}.", NLogConfigFile.Instance.FileInfo.FullName);
            }
        }

        private static void Dump(LoggingConfiguration config)
        {
            if (Logger.IsDebugEnabled)
            {
                config.Dump();
            }
        }

        private NLogger GetLogger(LoggerCacheKey cacheKey, UnityEngine.Object context, IFormatProvider formatProvider)
        {
            lock (this)
            {
                WeakReference l;

                if (this.loggerCache.TryGetValue(cacheKey, out l))
                {
                    NLogger existingLogger = l.Target as NLogger;
                    if (existingLogger != null)
                    {
                        // logger in the cache and still referenced
                        return existingLogger;
                    }
                }

                NLogger newLogger;

                if (cacheKey.ConcreteType != null && cacheKey.ConcreteType != typeof(NLogger))
                {

                    try
                    {
                        newLogger = (NLogger)FactoryHelper.CreateInstance(cacheKey.ConcreteType);
                    }
                    catch (Exception exception)
                    {
                        if (exception.MustBeRethrown())
                        {
                            throw;
                        }

                        if (ThrowExceptions)
                        {
                            throw;
                        }

                        Logger.Error("Cannot create instance of specified type. Proceeding with default type instance. Exception : {0}", exception);

                        //Creating default instance of logger if instance of specified type cannot be created.
                        cacheKey = new LoggerCacheKey(typeof(NLogger), cacheKey.Name);

                        newLogger = new NLogger(context, formatProvider);
                    }

                }
                else
                {
                    newLogger = new NLogger(context, formatProvider);
                }

                if (cacheKey.ConcreteType != null)
                {
                    newLogger.Initialize(cacheKey.Name, GetConfigurationForLogger(cacheKey.Name, Configuration), this);
                }


                this.loggerCache[cacheKey] = new WeakReference(newLogger);
                return newLogger;
            }
        }
        #endregion

        /// <summary>
        /// Logger cache key.
        /// </summary>
        internal class LoggerCacheKey
        {
            #region Properties
            internal Type ConcreteType { get; private set; }

            internal string Name { get; private set; }
            #endregion

            #region Constructors
            internal LoggerCacheKey(Type loggerConcreteType, string name)
            {
                ConcreteType = loggerConcreteType;
                Name = name;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Determines if two objects are equal in value.
            /// </summary>
            /// <param name="o">Other object to compare to.</param>
            /// <returns>True if objects are equal, false otherwise.</returns>
            public override bool Equals(object o)
            {
                var key = o as LoggerCacheKey;
                if (ReferenceEquals(key, null))
                {
                    return false;
                }

                return (ConcreteType == key.ConcreteType) && (key.Name == Name);
            }

            /// <summary>
            /// Serves as a hash function for a particular type.
            /// </summary>
            /// <returns>
            /// A hash code for the current <see cref="T:System.Object"/>.
            /// </returns>
            public override int GetHashCode()
            {
                return ConcreteType.GetHashCode() ^ Name.GetHashCode();
            }
            #endregion
        }
    }
}
#endif
