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

using System.Globalization;
using System.Linq;

#if AIUNITY_CODE
namespace AiUnity.NLog.Core.Config
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    using AiUnity.NLog.Core.Common;
    using AiUnity.NLog.Core.Internal;
    using AiUnity.NLog.Core.Targets;
    using AiUnity.Common.InternalLog;

    /// <summary>
    /// Keeps logging configuration and provides simple API to modify it.
    /// </summary>
    public class LoggingConfiguration
    {
        private readonly IDictionary<string, Target> targets =
            new Dictionary<string, Target>(StringComparer.OrdinalIgnoreCase);

        private object[] configItems;

        // Internal logger singleton
        private static IInternalLogger Logger { get { return NLogInternalLogger.Instance; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingConfiguration" /> class.
        /// </summary>
        public LoggingConfiguration()
        {
            this.LoggingRules = new List<LoggingRule>();
        }

        /// <summary>
        /// Gets a collection of named targets specified in the configuration.
        /// </summary>
        /// <returns>
        /// A list of named targets.
        /// </returns>
        /// <remarks>
        /// Unnamed targets (such as those wrapped by other targets) are not returned.
        /// </remarks>
        public ReadOnlyCollection<Target> ConfiguredNamedTargets
        {
            get { return new List<Target>(this.targets.Values).AsReadOnly(); }
        }

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// </summary>
        public virtual IEnumerable<string> FileNamesToWatch
        {
            get { return new string[0]; }
        }

        /// <summary>
        /// Gets the collection of logging rules.
        /// </summary>
        public IList<LoggingRule> LoggingRules { get; private set; }

        /// <summary>
        /// Gets or sets the default culture info use.
        /// </summary>
        public CultureInfo DefaultCultureInfo
        {
            get { return NLogManager.Instance.DefaultCultureInfo(); }
            set { NLogManager.Instance.DefaultCultureInfo = () => value; }
        }

        /// <summary>
        /// Gets all targets.
        /// </summary>
        public ReadOnlyCollection<Target> AllTargets
        {
            get { return this.configItems.OfType<Target>().ToList().AsReadOnly(); }
        }

        /// <summary>
        /// Registers the specified target object under a given name.
        /// </summary>
        /// <param name="name">
        /// Name of the target.
        /// </param>
        /// <param name="target">
        /// The target object.
        /// </param>
        public void AddTarget(string name, Target target)
        {
            if (name == null)
            {
                throw new ArgumentException("Target name cannot be null", "name");
            }

            Logger.Debug("Registering target {0}: {1}", name, target.GetType().FullName);
            this.targets[name] = target;
        }

        /// <summary>
        /// Finds the target with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the target to be found.
        /// </param>
        /// <returns>
        /// Found target or <see langword="null"/> when the target is not found.
        /// </returns>
        public Target FindTargetByName(string name)
        {
            Target value;

            if (!this.targets.TryGetValue(name, out value))
            {
                return null;
            }

            return value;
        }

        /// <summary>
        /// Called by LogManager when one of the log configuration files changes.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="LoggingConfiguration"/> that represents the updated configuration.
        /// </returns>
        public virtual LoggingConfiguration Reload()
        {
            return this;
        }

        /// <summary>
        /// Removes the specified named target.
        /// </summary>
        /// <param name="name">
        /// Name of the target.
        /// </param>
        public void RemoveTarget(string name)
        {
            this.targets.Remove(name);
        }

        /// <summary>
        /// Installs target-specific objects on current system.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        /// <remarks>
        /// Installation typically runs with administrative permissions.
        /// </remarks>
        /*public void Install(InstallationContext installationContext)
        {
            if (installationContext == null)
            {
                throw new ArgumentNullException("installationContext");
            }

            this.InitializeAll();
            foreach (IInstallable installable in this.configItems.OfType<IInstallable>())
            {
                installationContext.Info("Installing '{0}'", installable);

                try
                {
                    installable.Install(installationContext);
                    installationContext.Info("Finished installing '{0}'.", installable);
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    installationContext.Error("'{0}' installation failed: {1}.", installable, exception);
                }
            }
        }*/

        /// <summary>
        /// Uninstalls target-specific objects from current system.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        /// <remarks>
        /// Uninstallation typically runs with administrative permissions.
        /// </remarks>
        /*public void Uninstall(InstallationContext installationContext)
        {
            if (installationContext == null)
            {
                throw new ArgumentNullException("installationContext");
            }

            this.InitializeAll();

            foreach (IInstallable installable in this.configItems.OfType<IInstallable>())
            {
                installationContext.Info("Uninstalling '{0}'", installable);

                try
                {
                    installable.Uninstall(installationContext);
                    installationContext.Info("Finished uninstalling '{0}'.", installable);
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    installationContext.Error("Uninstallation of '{0}' failed: {1}.", installable, exception);
                }
            }
        }*/

        /// <summary>
        /// Closes all targets and releases any unmanaged resources.
        /// </summary>
        internal void Close()
        {
            Logger.Debug("Closing logging configuration...");
            foreach (ISupportsInitialize initialize in this.configItems.OfType<ISupportsInitialize>())
            {
                Logger.Trace("Closing {0}", initialize);
                try
                {
                    initialize.Close();
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    Logger.Warn("Exception while closing {0}", exception);
                }
            }

            Logger.Debug("Finished closing logging configuration.");
        }

        internal void Dump()
        {
            List<string> dump = new List<string>();

            dump.Add("----- NLog configuration dump. -----");
            dump.Add("Targets:");
            foreach (Target target in this.targets.Values)
            {
                dump.Add(target.ToString());
            }

            dump.Add("Rules:");
            foreach (LoggingRule rule in this.LoggingRules)
            {
                dump.Add(rule.ToString());
            }
            dump.Add("--- End of NLog configuration dump ---");

            Logger.Debug(string.Join(Environment.NewLine, dump.ToArray()));
        }

        /// <summary>
        /// Flushes any pending log messages on all appenders.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        internal void FlushAllTargets(AsyncContinuation asyncContinuation)
        {
            var uniqueTargets = new List<Target>();
            foreach (var rule in this.LoggingRules)
            {
                foreach (var t in rule.Targets)
                {
                    if (!uniqueTargets.Contains(t))
                    {
                        uniqueTargets.Add(t);
                    }
                }
            }

            AsyncHelpers.ForEachItemInParallel(uniqueTargets, asyncContinuation, (target, cont) => target.Flush(cont));
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        internal void ValidateConfig()
        {
            var roots = new List<object>();
            foreach (LoggingRule r in this.LoggingRules)
            {
                roots.Add(r);
            }

            foreach (Target target in this.targets.Values)
            {
                roots.Add(target);
            }

            if (NLogManager.Instance.ShowUnityLog)
            {
                roots.Add(new UnityLogListener());
            }

            this.configItems = ObjectGraphScanner.FindReachableObjects<object>(roots.ToArray());

            // initialize all config items starting from most nested first
            // so that whenever the container is initialized its children have already been
            Logger.Trace("Found {0} configuration items", this.configItems.Length);

            foreach (object o in this.configItems)
            {
                PropertyHelper.CheckRequiredParameters(o);
            }
        }

        internal void InitializeAll()
        {
            this.ValidateConfig();

            foreach (ISupportsInitialize initialize in this.configItems.OfType<ISupportsInitialize>().Reverse())
            {
                Logger.Trace("Initializing {0}", initialize);

                try
                {
                    initialize.Initialize(this);
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    if (NLogManager.Instance.ThrowExceptions)
                    {
                        throw new NLogConfigurationException("Error during initialization of " + initialize, exception);
                    }
                }
            }
        }

        internal void EnsureInitialized()
        {
            //this.InitializeAll();
        }
    }
}
#endif
