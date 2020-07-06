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
namespace AiUnity.NLog.Core.Config
{
    using System;
    using AiUnity.NLog.Core.Common;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using AiUnity.NLog.Core.Filters;
    using AiUnity.NLog.Core.Internal;
    using AiUnity.NLog.Core.Layouts;
    using AiUnity.NLog.Core.Targets;
    using AiUnity.NLog.Core.Targets.Wrappers;
    using AiUnity.NLog.Core.Time;
    using AiUnity.Common.Extensions;
    using AiUnity.Common.InternalLog;
    using AiUnity.Common.Log;

    /// <summary>
    /// A class for configuring NLog through an XML configuration file 
    /// (App.config style or App.nlog style).
    /// </summary>
    public class XmlLoggingConfiguration : LoggingConfiguration
    {
        private readonly ConfigurationItemFactory configurationItemFactory = ConfigurationItemFactory.Default;
        private readonly Dictionary<string, bool> visitedFile = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Internal logger singleton
        private static IInternalLogger Logger { get { return NLogInternalLogger.Instance; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        public XmlLoggingConfiguration(string fileName)
        {
            using (XmlReader reader = XmlReader.Create(fileName)) {
                this.Initialize(reader, fileName, false);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        public XmlLoggingConfiguration(string fileName, bool ignoreErrors)
        {
            using (XmlReader reader = XmlReader.Create(fileName)) {
                this.Initialize(reader, fileName, ignoreErrors);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        public XmlLoggingConfiguration(XmlReader reader, string fileName)
        {
            this.Initialize(reader, fileName, false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        public XmlLoggingConfiguration(XmlReader reader, string fileName, bool ignoreErrors)
        {
            this.Initialize(reader, fileName, ignoreErrors);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        public XmlLoggingConfiguration(string text, string fileName)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(text))) {
                this.Initialize(reader, fileName, false);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        public XmlLoggingConfiguration(string text, string fileName, bool ignoreErrors)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(text))) {
                this.Initialize(reader, fileName, ignoreErrors);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <param name="fileName">Name of the XML file.</param>
        internal XmlLoggingConfiguration(XmlElement element, string fileName)
        {
            using (var stringReader = new StringReader(element.OuterXml)) {
                XmlReader reader = XmlReader.Create(stringReader);

                this.Initialize(reader, fileName, false);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <param name="fileName">Name of the XML file.</param>
        /// <param name="ignoreErrors">If set to <c>true</c> errors will be ignored during file processing.</param>
        internal XmlLoggingConfiguration(XmlElement element, string fileName, bool ignoreErrors)
        {
            using (var stringReader = new StringReader(element.OuterXml)) {
                XmlReader reader = XmlReader.Create(stringReader);

                this.Initialize(reader, fileName, ignoreErrors);
            }
        }

        /// <summary>
        /// Gets the variables defined in the configuration.
        /// </summary>
        public Dictionary<string, string> Variables
        {
            get
            {
                return variables;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the configuration files
        /// should be watched for changes and reloaded automatically when changed.
        /// </summary>
        public bool AutoReload { get; set; }

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// This is the list of configuration files processed.
        /// If the <c>autoReload</c> attribute is not set it returns empty collection.
        /// </summary>
        public override IEnumerable<string> FileNamesToWatch
        {
            get
            {
                if (this.AutoReload) {
                    return this.visitedFile.Keys;
                }

                return new string[0];
            }
        }

        /// <summary>
        /// Re-reads the original configuration file and returns the new <see cref="LoggingConfiguration" /> object.
        /// </summary>
        /// <returns>The new <see cref="XmlLoggingConfiguration" /> object.</returns>
        public override LoggingConfiguration Reload()
        {
            string configText = NLogConfigFile.Instance.GetConfigText();

            if (!string.IsNullOrEmpty(configText))
            {
                return new XmlLoggingConfiguration(configText, NLogConfigFile.Instance.FileInfo.FullName);
            }
            else
            {
                Logger.Info("Using default configuration because unable to locate {0}.", NLogConfigFile.Instance.RelativeName);
            }

            return null;
        }

        private static bool IsTargetElement(string name)
        {
            return name.Equals("target", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("wrapper", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("wrapper-target", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("compound-target", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTargetRefElement(string name)
        {
            return name.Equals("target-ref", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("wrapper-target-ref", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("compound-target-ref", StringComparison.OrdinalIgnoreCase);
        }

        private static string CleanWhitespace(string s)
        {
            s = s.Replace(" ", string.Empty); // get rid of the whitespace
            return s;
        }

        private static string StripOptionalNamespacePrefix(string attributeValue)
        {
            if (attributeValue == null) {
                return null;
            }

            int p = attributeValue.IndexOf(':');
            if (p < 0) {
                return attributeValue;
            }

            return attributeValue.Substring(p + 1);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Target is disposed elsewhere.")]
        private static Target WrapWithAsyncTargetWrapper(Target target)
        {
            var asyncTargetWrapper = new AsyncTargetWrapper();
            asyncTargetWrapper.WrappedTarget = target;
            asyncTargetWrapper.Name = target.Name;
            target.Name = target.Name + "_wrapped";
            Logger.Debug("Wrapping target '{0}' with AsyncTargetWrapper and renaming to '{1}", asyncTargetWrapper.Name, target.Name);
            target = asyncTargetWrapper;
            return target;
        }

        /// <summary>
        /// Initializes the configuration.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        private void Initialize(XmlReader reader, string fileName, bool ignoreErrors)
        {
            try {
                reader.MoveToContent();
                NLogXmlElement content = null;
                try {
                    content = new NLogXmlElement(reader);
                }
                catch (Exception e) {
                    Logger.Error("Fail to read configuration file {0} because {1}", fileName, e.Message);
                }

                if (fileName != null) {
                    Logger.Info("Initialize NLog based upon {0}", fileName);
                    string key = Path.GetFullPath(fileName);
                    this.visitedFile[key] = true;
                    this.ParseTopLevel(content, Path.GetDirectoryName(fileName));

                }
                else {
                    this.ParseTopLevel(content, null);
                }
            }
            catch (Exception exception) {
                if (exception.MustBeRethrown()) {
                    throw;
                }

                NLogConfigurationException ConfigException = new NLogConfigurationException("Exception occurred when loading configuration from " + fileName, exception);

                if (!ignoreErrors) {
                    if (NLogManager.Instance.ThrowExceptions) {
                        throw ConfigException;
                    }
                    else {
                        Logger.Error("Error in Parsing Configuration File. Exception : {0}", ConfigException);
                    }
                }
                else {
                    Logger.Assert(ConfigException, "Error in Parsing Configuration File. Exception : {0}");
                }
            }
        }

        private void ConfigureFromFile(string fileName)
        {
            string key = Path.GetFullPath(fileName);
            if (this.visitedFile.ContainsKey(key)) {
                return;
            }

            this.visitedFile[key] = true;

            this.ParseTopLevel(new NLogXmlElement(fileName), Path.GetDirectoryName(fileName));
        }

        private void ParseTopLevel(NLogXmlElement content, string baseDirectory)
        {
            content.AssertName("nlog", "configuration");

            switch (content.LocalName.ToUpper(CultureInfo.InvariantCulture)) {
                case "CONFIGURATION":
                    this.ParseConfigurationElement(content, baseDirectory);
                    break;
                case "NLOG":
                    this.ParseNLogElement(content, baseDirectory);
                    break;
            }
        }

        private void ParseConfigurationElement(NLogXmlElement configurationElement, string baseDirectory)
        {
            Logger.Trace("ParseConfigurationElement");
            configurationElement.AssertName("configuration");

            foreach (var el in configurationElement.Elements("nlog")) {
                this.ParseNLogElement(el, baseDirectory);
            }
        }

        private void ParseNLogElement(NLogXmlElement nlogElement, string baseDirectory)
        {
            Logger.Trace("ParseNLogElement");
            nlogElement.AssertName("nlog");

            if (nlogElement.GetOptionalBooleanAttribute("useInvariantCulture", false)) {
                this.DefaultCultureInfo = CultureInfo.InvariantCulture;
            }

            AutoReload = nlogElement.GetOptionalBooleanAttribute("autoReload", false);
            NLogManager.Instance.ThrowExceptions = nlogElement.GetOptionalBooleanAttribute("throwExceptions", NLogManager.Instance.ThrowExceptions);
            //Logger.InternalLogLevel = nlogElement.GetOptionalAttribute("internalLevels", Logger.InternalLogLevelsDefault.ToString()).ToEnum<LogLevels>();

            //  The LogLevel conditional attributes replaces the need for GlobalThreshold
            //NLogManager.Instance.GlobalLogLevel = nlogElement.GetOptionalAttribute("globalLevels", NLogManager.GlobalLogLevelsDefault.ToString()).ToEnum<LogLevels>();
            NLogManager.Instance.GlobalLogLevel = LogLevels.Everything;
            NLogManager.Instance.AssertException = bool.Parse(nlogElement.GetOptionalAttribute("assertException", false.ToString()));
            NLogManager.Instance.ShowUnityLog = bool.Parse(nlogElement.GetOptionalAttribute("enableUnityLogListener", false.ToString()));

            foreach (var el in nlogElement.Children) {
                switch (el.LocalName.ToUpper(CultureInfo.InvariantCulture)) {
                    case "EXTENSIONS":
                        this.ParseExtensionsElement(el, baseDirectory);
                        break;

                    case "INCLUDE":
                        this.ParseIncludeElement(el, baseDirectory);
                        break;

                    case "APPENDERS":
                    case "TARGETS":
                        this.ParseTargetsElement(el);
                        break;

                    case "VARIABLE":
                        this.ParseVariableElement(el);
                        break;

                    case "RULES":
                        this.ParseRulesElement(el, this.LoggingRules);
                        break;

                    case "TIME":
                        this.ParseTimeElement(el);
                        break;

                    default:
                        Logger.Warn("Skipping unknown node: {0}", el.LocalName);
                        break;
                }
            }
        }

        private void ParseRulesElement(NLogXmlElement rulesElement, IList<LoggingRule> rulesCollection)
        {
            Logger.Trace("ParseRulesElement");
            rulesElement.AssertName("rules");

            foreach (var loggerElement in rulesElement.Elements("logger")) {
                this.ParseLoggerElement(loggerElement, rulesCollection);
            }
        }

        private void ParseLoggerElement(NLogXmlElement loggerElement, IList<LoggingRule> rulesCollection)
        {
            loggerElement.AssertName("logger");

            var enabled = loggerElement.GetOptionalBooleanAttribute("enabled", true);
            if (!enabled) {
                Logger.Debug("The logger named '{0}' are disabled");
                return;
            }

            var rule = new LoggingRule();
            string appendTo = loggerElement.GetOptionalAttribute("appendTo", null);
            if (appendTo == null) {
                appendTo = loggerElement.GetOptionalAttribute("target", null);
            }

            // aidesigner
            //rule.LoggerNamePattern = loggerElement.GetOptionalAttribute("name", "*"); ;
            rule.namePatternMatch.LoggerNamePattern = loggerElement.GetOptionalAttribute("name", "*"); ;
            rule.namespacePatternMatch.LoggerNamePattern = loggerElement.GetOptionalAttribute("namespace", "*"); ;

            // aidesigner
            //string platforms = loggerElement.GetOptionalAttribute("platforms", "Everything");
            //rule.TargetPlatforms = platforms.ToEnum<RuntimePlatforms>();
            rule.TargetPlatforms = loggerElement.GetOptionalAttribute("platforms", "Everything");

            if (appendTo != null) {
                foreach (string t in appendTo.Split(',')) {
                    string targetName = t.Trim();
                    Target target = FindTargetByName(targetName);

                    if (target != null) {
                        rule.Targets.Add(target);
                    }
                    else {
                        throw new NLogConfigurationException(string.Format("The rule having pattern \"{0}\" has an unknown target \"{1}\".", rule.namePatternMatch.LoggerNamePattern, targetName));
                    }
                }
            }
            rule.Final = loggerElement.GetOptionalBooleanAttribute("final", false);

            string levelString;

            if (loggerElement.AttributeValues.TryGetValue("level", out levelString)) {
                //LogLevels level = LogLevels.FromString(levelString);
                LogLevels level = levelString.ToEnum<LogLevels>();
                rule.EnableLoggingForLevel(level);
            }
            else if (loggerElement.AttributeValues.TryGetValue("levels", out levelString)) {
                levelString = CleanWhitespace(levelString);

                string[] tokens = levelString.Split(',');
                foreach (string s in tokens) {
                    if (!string.IsNullOrEmpty(s)) {
                        //LogLevels level = LogLevels.FromString(s);
                        LogLevels level = levelString.ToEnum<LogLevels>();
                        rule.EnableLoggingForLevel(level);
                    }
                }
            }
            else {
                //int minLevel = 0;
                //LogLevels minLevel = LogLevels.Trace;
                LogLevels minLevel = LogLevels.Assert;
                //int maxLevel = LogLevels.MaxLevel.Ordinal;
                //LogLevels maxLevel = LogLevels.Assert;
                LogLevels maxLevel = LogLevels.Trace;
                string minLevelString;
                string maxLevelString;

                if (loggerElement.AttributeValues.TryGetValue("minLevel", out minLevelString)) {
                    //minLevel = LogLevels.FromString(minLevelString).Ordinal;
                    minLevel = minLevelString.ToEnum<LogLevels>(); //.ToEnAiUnity.Log.Common.Extensions.EnumExtensions.ToEnum(minLevelString);
                }

                if (loggerElement.AttributeValues.TryGetValue("maxLevel", out maxLevelString)) {
                    //maxLevel = LogLevels.FromString(maxLevelString).Ordinal;
                    maxLevel = maxLevelString.ToEnum<LogLevels>();
                }

                for (int i = (int)minLevel; i <= (int)maxLevel; i = i << 1) {
                    //rule.EnableLoggingForLevel(LogLevels.FromOrdinal(i));
                    rule.EnableLoggingForLevel((LogLevels)i);
                }
            }

            foreach (var child in loggerElement.Children) {
                switch (child.LocalName.ToUpper(CultureInfo.InvariantCulture)) {
                    case "FILTERS":
                        this.ParseFilters(rule, child);
                        break;

                    case "LOGGER":
                        this.ParseLoggerElement(child, rule.ChildRules);
                        break;
                }
            }

            rulesCollection.Add(rule);
        }

        private void ParseFilters(LoggingRule rule, NLogXmlElement filtersElement)
        {
            filtersElement.AssertName("filters");

            foreach (var filterElement in filtersElement.Children) {
                string name = filterElement.LocalName;

                Filter filter = this.configurationItemFactory.Filters.CreateInstance(name);
                this.ConfigureObjectFromAttributes(filter, filterElement, false);
                rule.Filters.Add(filter);
            }
        }

        private void ParseVariableElement(NLogXmlElement variableElement)
        {
            variableElement.AssertName("variable");

            string name = variableElement.GetRequiredAttribute("name");
            string value = this.ExpandVariables(variableElement.GetRequiredAttribute("value"));

            this.variables[name] = value;
        }

        private void ParseTargetsElement(NLogXmlElement targetsElement)
        {
            targetsElement.AssertName("targets", "appenders");

            bool asyncWrap = targetsElement.GetOptionalBooleanAttribute("async", false);
            NLogXmlElement defaultWrapperElement = null;
            var typeNameToDefaultTargetParameters = new Dictionary<string, NLogXmlElement>();

            foreach (var targetElement in targetsElement.Children) {
                string name = targetElement.LocalName;
                string type = StripOptionalNamespacePrefix(targetElement.GetOptionalAttribute("type", null));

                switch (name.ToUpper(CultureInfo.InvariantCulture)) {
                    case "DEFAULT-WRAPPER":
                        defaultWrapperElement = targetElement;
                        break;

                    case "DEFAULT-TARGET-PARAMETERS":
                        if (type == null) {
                            throw new NLogConfigurationException("Missing 'type' attribute on <" + name + "/>.");
                        }

                        typeNameToDefaultTargetParameters[type] = targetElement;
                        break;

                    case "TARGET":
                    case "APPENDER":
                    case "WRAPPER":
                    case "WRAPPER-TARGET":
                    case "COMPOUND-TARGET":
                        if (type == null) {
                            throw new NLogConfigurationException("Missing 'type' attribute on <" + name + "/>.");
                        }

                        Target newTarget = this.configurationItemFactory.Targets.CreateInstance(type);

                        NLogXmlElement defaults;
                        if (typeNameToDefaultTargetParameters.TryGetValue(type, out defaults)) {
                            this.ParseTargetElement(newTarget, defaults);
                        }

                        this.ParseTargetElement(newTarget, targetElement);

                        if (asyncWrap) {
                            newTarget = WrapWithAsyncTargetWrapper(newTarget);
                        }

                        if (defaultWrapperElement != null) {
                            newTarget = this.WrapWithDefaultWrapper(newTarget, defaultWrapperElement);
                        }

                        Logger.Debug("Adding target {0}", newTarget);
                        AddTarget(newTarget.Name, newTarget);
                        break;
                }
            }
        }

        private void ParseTargetElement(Target target, NLogXmlElement targetElement)
        {
            var compound = target as CompoundTargetBase;
            var wrapper = target as WrapperTargetBase;

            this.ConfigureObjectFromAttributes(target, targetElement, true);

            foreach (var childElement in targetElement.Children) {
                string name = childElement.LocalName;

                if (compound != null) {
                    if (IsTargetRefElement(name)) {
                        string targetName = childElement.GetRequiredAttribute("name");
                        Target newTarget = this.FindTargetByName(targetName);
                        if (newTarget == null) {
                            throw new NLogConfigurationException("Referenced target '" + targetName + "' not found.");
                        }

                        compound.Targets.Add(newTarget);
                        continue;
                    }

                    if (IsTargetElement(name)) {
                        string type = StripOptionalNamespacePrefix(childElement.GetRequiredAttribute("type"));

                        Target newTarget = this.configurationItemFactory.Targets.CreateInstance(type);
                        if (newTarget != null) {
                            this.ParseTargetElement(newTarget, childElement);
                            if (newTarget.Name != null) {
                                // If the new target has name, register it
                                AddTarget(newTarget.Name, newTarget);
                            }

                            compound.Targets.Add(newTarget);
                        }

                        continue;
                    }
                }

                if (wrapper != null) {
                    if (IsTargetRefElement(name)) {
                        string targetName = childElement.GetRequiredAttribute("name");
                        Target newTarget = this.FindTargetByName(targetName);
                        if (newTarget == null) {
                            throw new NLogConfigurationException("Referenced target '" + targetName + "' not found.");
                        }

                        wrapper.WrappedTarget = newTarget;
                        continue;
                    }

                    if (IsTargetElement(name)) {
                        string type = StripOptionalNamespacePrefix(childElement.GetRequiredAttribute("type"));

                        Target newTarget = this.configurationItemFactory.Targets.CreateInstance(type);
                        if (newTarget != null) {
                            this.ParseTargetElement(newTarget, childElement);
                            if (newTarget.Name != null) {
                                // if the new target has name, register it
                                AddTarget(newTarget.Name, newTarget);
                            }

                            if (wrapper.WrappedTarget != null) {
                                throw new NLogConfigurationException("Wrapper target can only have one child.");
                            }

                            wrapper.WrappedTarget = newTarget;
                        }

                        continue;
                    }
                }

                this.SetPropertyFromElement(target, childElement);
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom", Justification = "Need to load external assembly.")]
        private void ParseExtensionsElement(NLogXmlElement extensionsElement, string baseDirectory)
        {
            extensionsElement.AssertName("extensions");

            foreach (var addElement in extensionsElement.Elements("add")) {
                string prefix = addElement.GetOptionalAttribute("prefix", null);

                if (prefix != null) {
                    prefix = prefix + ".";
                }

                string type = StripOptionalNamespacePrefix(addElement.GetOptionalAttribute("type", null));
                if (type != null) {
                    this.configurationItemFactory.RegisterType(Type.GetType(type, true), prefix);
                }

                string assemblyFile = addElement.GetOptionalAttribute("assemblyFile", null);
                if (assemblyFile != null) {
                    try {
                        string fullFileName = Path.Combine(baseDirectory, assemblyFile);
                        Logger.Info("Loading assembly file: {0}", fullFileName);

                        Assembly asm = Assembly.LoadFrom(fullFileName);
                        this.configurationItemFactory.RegisterItemsFromAssembly(asm, prefix);
                    }
                    catch (Exception exception) {
                        if (exception.MustBeRethrown()) {
                            throw;
                        }

                        Logger.Error("Error loading extensions: {0}", exception);
                        if (NLogManager.Instance.ThrowExceptions) {
                            throw new NLogConfigurationException("Error loading extensions: " + assemblyFile, exception);
                        }
                    }

                    continue;
                }

                string assemblyName = addElement.GetOptionalAttribute("assembly", null);
                if (assemblyName != null) {
                    try {
                        Logger.Info("Loading assembly name: {0}", assemblyName);
                        Assembly asm = Assembly.Load(assemblyName);

                        this.configurationItemFactory.RegisterItemsFromAssembly(asm, prefix);
                    }
                    catch (Exception exception) {
                        if (exception.MustBeRethrown()) {
                            throw;
                        }

                        Logger.Error("Error loading extensions: {0}", exception);
                        if (NLogManager.Instance.ThrowExceptions) {
                            throw new NLogConfigurationException("Error loading extensions: " + assemblyName, exception);
                        }
                    }

                    continue;
                }
            }
        }

        private void ParseIncludeElement(NLogXmlElement includeElement, string baseDirectory)
        {
            includeElement.AssertName("include");

            string newFileName = includeElement.GetRequiredAttribute("file");

            try {
                newFileName = this.ExpandVariables(newFileName);
                newFileName = SimpleLayout.Evaluate(newFileName);
                if (baseDirectory != null) {
                    newFileName = Path.Combine(baseDirectory, newFileName);
                }

                if (File.Exists(newFileName)) {
                    Logger.Debug("Including file '{0}'", newFileName);
                    this.ConfigureFromFile(newFileName);
                }
                else {
                    throw new FileNotFoundException("Included file not found: " + newFileName);
                }
            }
            catch (Exception exception) {
                if (exception.MustBeRethrown()) {
                    throw;
                }

                Logger.Error("Error when including '{0}' {1}", newFileName, exception);

                if (includeElement.GetOptionalBooleanAttribute("ignoreErrors", false)) {
                    return;
                }

                throw new NLogConfigurationException("Error when including: " + newFileName, exception);
            }
        }

        private void ParseTimeElement(NLogXmlElement timeElement)
        {
            timeElement.AssertName("time");

            string type = timeElement.GetRequiredAttribute("type");

            TimeSource newTimeSource = this.configurationItemFactory.TimeSources.CreateInstance(type);

            this.ConfigureObjectFromAttributes(newTimeSource, timeElement, true);

            Logger.Debug("Selecting time source {0}", newTimeSource);
            TimeSource.Current = newTimeSource;
        }

        private void SetPropertyFromElement(object o, NLogXmlElement element)
        {
            if (this.AddArrayItemFromElement(o, element)) {
                return;
            }

            if (this.SetLayoutFromElement(o, element)) {
                return;
            }

            PropertyHelper.SetPropertyFromString(o, element.LocalName, this.ExpandVariables(element.Value), this.configurationItemFactory);
        }

        private bool AddArrayItemFromElement(object o, NLogXmlElement element)
        {
            string name = element.LocalName;

            PropertyInfo propInfo;
            if (!PropertyHelper.TryGetPropertyInfo(o, name, out propInfo)) {
                return false;
            }

            Type elementType = PropertyHelper.GetArrayItemType(propInfo);
            if (elementType != null) {
                IList propertyValue = (IList)propInfo.GetValue(o, null);
                object arrayItem = FactoryHelper.CreateInstance(elementType);
                this.ConfigureObjectFromAttributes(arrayItem, element, true);
                this.ConfigureObjectFromElement(arrayItem, element);
                propertyValue.Add(arrayItem);
                return true;
            }

            return false;
        }

        private void ConfigureObjectFromAttributes(object targetObject, NLogXmlElement element, bool ignoreType)
        {
            foreach (var kvp in element.AttributeValues) {
                string childName = kvp.Key;
                string childValue = kvp.Value;

                if (ignoreType && childName.Equals("type", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                PropertyHelper.SetPropertyFromString(targetObject, childName, this.ExpandVariables(childValue), this.configurationItemFactory);
            }
        }

        private bool SetLayoutFromElement(object o, NLogXmlElement layoutElement)
        {
            PropertyInfo targetPropertyInfo;
            string name = layoutElement.LocalName;

            // if property exists
            if (PropertyHelper.TryGetPropertyInfo(o, name, out targetPropertyInfo)) {
                // and is a Layout
                if (typeof(Layout).IsAssignableFrom(targetPropertyInfo.PropertyType)) {
                    string layoutTypeName = StripOptionalNamespacePrefix(layoutElement.GetOptionalAttribute("type", null));

                    // and 'type' attribute has been specified
                    if (layoutTypeName != null) {
                        // configure it from current element
                        Layout layout = this.configurationItemFactory.Layouts.CreateInstance(this.ExpandVariables(layoutTypeName));
                        this.ConfigureObjectFromAttributes(layout, layoutElement, true);
                        this.ConfigureObjectFromElement(layout, layoutElement);
                        targetPropertyInfo.SetValue(o, layout, null);
                        return true;
                    }
                }
            }

            return false;
        }

        private void ConfigureObjectFromElement(object targetObject, NLogXmlElement element)
        {
            foreach (var child in element.Children) {
                this.SetPropertyFromElement(targetObject, child);
            }
        }

        private Target WrapWithDefaultWrapper(Target t, NLogXmlElement defaultParameters)
        {
            string wrapperType = StripOptionalNamespacePrefix(defaultParameters.GetRequiredAttribute("type"));

            Target wrapperTargetInstance = this.configurationItemFactory.Targets.CreateInstance(wrapperType);
            WrapperTargetBase wtb = wrapperTargetInstance as WrapperTargetBase;
            if (wtb == null) {
                throw new NLogConfigurationException("Target type specified on <default-wrapper /> is not a wrapper.");
            }

            this.ParseTargetElement(wrapperTargetInstance, defaultParameters);
            while (wtb.WrappedTarget != null) {
                wtb = wtb.WrappedTarget as WrapperTargetBase;
                if (wtb == null) {
                    throw new NLogConfigurationException("Child target type specified on <default-wrapper /> is not a wrapper.");
                }
            }

            wtb.WrappedTarget = t;
            wrapperTargetInstance.Name = t.Name;
            t.Name = t.Name + "_wrapped";

            Logger.Debug("Wrapping target '{0}' with '{1}' and renaming to '{2}", wrapperTargetInstance.Name, wrapperTargetInstance.GetType().Name, t.Name);
            return wrapperTargetInstance;
        }

        private string ExpandVariables(string input)
        {
            string output = input;

            // TODO - make this case-insensitive, will probably require a different approach
            foreach (var kvp in this.variables) {
                output = output.Replace("${" + kvp.Key + "}", kvp.Value);
            }

            return output;
        }
    }
}
#endif
