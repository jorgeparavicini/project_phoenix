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

using System.Globalization;
using System.Reflection;

namespace AiUnity.NLog.Core
{
    using AiUnity.Common.Log;
    using AiUnity.Common.Patterns;
    using AiUnity.NLog.Core.Common;
    using AiUnity.NLog.Core.Config;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
    /// </summary>
    /// <tags>NLogAPI</tags>
    public sealed class NLogManager : Singleton<NLogManager>, ILogManager
    {
        #region Constants
        public const LogLevels GlobalLogLevelsDefault = LogLevels.Everything;
        #endregion

        #region Fields
        private readonly LogFactory globalFactory = new LogFactory();
        #endregion

        #region Properties
        /// <summary>
        /// Determines if an assert message should produce an exception.
        /// </summary>
        public bool AssertException { get; set; }

        public GetCultureInfo DefaultCultureInfo { get; set; }

        /// <summary>
        /// Gets or sets the global log threshold. Log events below this threshold are not logged.
        /// </summary>
        public LogLevels GlobalLogLevel
        {
            get { return this.globalFactory.GlobalThreshold; }
            set { this.globalFactory.GlobalThreshold = value; }
        }

        public HashSet<Assembly> HiddenAssemblies { get; set; }

        public HashSet<string> HiddenNameSpaces { get; set; }

        /// <summary>
        /// Determines NLog listens and shows Unity log messages.
        /// </summary>
        /// <autogeneratedoc />
        public bool ShowUnityLog { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether NLog should throw exceptions. 
        /// By default exceptions are not thrown under any circumstances.
        /// </summary>
        public bool ThrowExceptions
        {
            get { return this.globalFactory.ThrowExceptions; }
            set { this.globalFactory.ThrowExceptions = value; }
        }

        /// <summary>
        /// Gets or sets the current logging configuration.
        /// </summary>
        internal LoggingConfiguration Configuration
        {
            get { return this.globalFactory.Configuration; }
            set { this.globalFactory.Configuration = value; }
        }
        #endregion

        #region Delegates
        /// <summary>
        /// Delegate used to the the culture to use.
        /// </summary>
        /// <returns></returns>
        public delegate CultureInfo GetCultureInfo();
        #endregion

        #region Events
        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> changes.
        /// </summary>
        public event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged
        {
            add { this.globalFactory.ConfigurationChanged += value; }
            remove { this.globalFactory.ConfigurationChanged -= value; }
        }

        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> gets reloaded.
        /// </summary>
        public event EventHandler<LoggingConfigurationReloadedEventArgs> ConfigurationReloaded
        {
            add { this.globalFactory.ConfigurationReloaded += value; }
            remove { this.globalFactory.ConfigurationReloaded -= value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Prevents a default instance of the LogManager class from being created.
        /// </summary>
        //private NLogManager()
        public NLogManager()
        {
            HiddenAssemblies = new HashSet<Assembly>();
            HiddenNameSpaces = new HashSet<string>();
            DefaultCultureInfo = () => CultureInfo.CurrentCulture;
            AssertException = false;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public NLogger GetLogger(object context, IFormatProvider formatProvider = null)
        {
            //NLogInternalLogger.Instance.Warn("GetLogger(internal) Level=" + NLogInternalLogger.Instance.InternalLogLevel);
            UnityEngine.Object UnityContext = context as UnityEngine.Object;
            return this.globalFactory.GetLogger(context.GetType().FullName, UnityContext, formatProvider);
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public NLogger GetLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider = null)
        {
            return this.globalFactory.GetLogger(name, context, formatProvider);
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <param name="loggerType">The logger class. The class must inherit from <see cref="Logger" />.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public NLogger GetLogger(string name, Type loggerType, UnityEngine.Object context = null, IFormatProvider formatProvider = null)
        {
            return this.globalFactory.GetLogger(name, loggerType, context, formatProvider);
        }

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger.
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        public void ReconfigExistingLoggers()
        {
            this.globalFactory.ReconfigExistingLoggers();
        }

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger.
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        public void ReloadConfig()
        {
            this.globalFactory.ReloadConfig();
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        ILogger ILogManager.GetLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider)
        {
            return GetLogger(name, context, formatProvider);
        }
        #endregion

    }
}
#endif
