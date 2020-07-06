// ***********************************************************************
// Assembly         : Assembly-CSharp-Editor
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 05-21-2018
// ***********************************************************************
using System.Xml;
using UnityEditorInternal;

namespace AiUnity.NLog.Editor
{
    using AiUnity.Common.Attributes;
    using AiUnity.Common.Editor.Styles;
    using AiUnity.Common.Extensions;
    using AiUnity.Common.InternalLog;
    using AiUnity.Common.Log;
    using AiUnity.Common.Types;
    using AiUnity.NLog.Core;
    using AiUnity.NLog.Core.Common;
    using AiUnity.NLog.Core.LayoutRenderers;
    using AiUnity.NLog.Core.Layouts;
    using AiUnity.NLog.Core.Targets;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEngine;


    /// <summary>
    /// NLog editor window to create, edit, test nlog XML configuration.
    /// </summary>
    /// <seealso cref="UnityEditor.EditorWindow" />
    [Serializable]
    public class NLogEditor : EditorWindow
    {
        #region const fields
        /// <summary>
        /// The default configuration
        /// </summary>
        /// Default configuration file created upon user request
        /// The NLog standard namespace is xmlns=""http://www.nlog-project.org/schemas/NLog.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
        private const string DefaultConfig = @"<?xml version=""1.0"" encoding=""utf-8""?>
  <nlog>
	<targets>
		<target name=""UnityConsole"" type=""UnityConsole""/>
	</targets>
	<rules>
		<logger name=""*"" namespace=""*"" target=""UnityConsole""/>
	</rules>
  </nlog>";

        // Tooltip for config file selection GUI
        private const string ConfigTooltip = "Fixed location of NLog XML configuration file.  The GUI controls below provide the means to modify the XML indirectly (recommended) and directly (XML Viewer).  The NLog framework is solely configured by reading the configuration XML resource file at runtime, leaving it completely independent from the Unity Editor.";
        private GUIContent configFileLabel = new GUIContent("Config File", ConfigTooltip);
        // Tooltip for Source/DLL selection GUI
        private const string SourceTooltip = @"Specifies if NLog DLLs (recommended) or Source Code is used during compilation.  DLLs compile faster and enable double-click of log messages to bring up corresponding IDE editor line.  Source code allows you to investigate, modify, and extend the inner workings of NLog.";
        private GUIContent nlogSourceContent = new GUIContent("Source", SourceTooltip);
        // Tooltip for platform selection GUI
        private const string PlatformsTooltip = @"Specifies which Unity platforms builds include NLog logging.  Logging statements on unselected platforms will be compiled out of existence.  To select additional platforms at least one Build level must be selected.  Note the standalone editor platform is required.";
        private GUIContent buildPlatformsContent = new GUIContent("Platforms", PlatformsTooltip);
        // Tooltip for editor level selection GUI
        private const string EditorLevelsTooltip = @"Specifies which logging levels are included in Unity Editor.";
        // Tooltip for global level selection GUI
        private const string BuildLevelsTooltip = @"Specifies which logging levels are included in Unity builds.  Logging statements using unselected levels will be compiled out of existence.";
        private GUIContent buildLevelsContent = new GUIContent("Build levels", BuildLevelsTooltip);
        // Tooltip for Internal Level selection GUI
        private const string InternalLevelsTooltip = @"Specifies which logging levels are enabled for NLog internal messages (Debug feature).";
        private GUIContent internalLevelsContent = new GUIContent("Internal levels", InternalLevelsTooltip);
        // Tooltip for unity log selection GUI
        private const string enableUnityLogListenerTooltip = @"Enables NLog to display messages originating from the built in UnityEngine logger.  This is accomplished by hooking into the UnityEngine Debug callback.  The logger name is set to the calling class name, which was obtained by analyzing the stack trace.  Unity Console target(s) will drop these messages to prevent duplicate messages.";
        /// Tooltip for assert raise selection GUI
        private const string assertExceptionTooltip = @"Determine if failing NLog assertions should raise an exception.  In addition the Game is paused.";
        private GUIContent assertExceptionContent = new GUIContent("Assert raise", assertExceptionTooltip);
        #endregion

        #region private fields
        private XNamespace DefaultNamespace;
        private Dictionary<XElement, AnimBool> foldoutStates = new Dictionary<XElement, AnimBool>();
        private Dictionary<XElement, AnimBool> showAdvancedTarget;
        private bool IsConfigLoaded;
        private bool IsConfigValid;
        private string layoutInsertText;
        private XElement layoutTarget;
        private string layoutPropertyName;
        private GenericMenu layoutMenu;
        private IEnumerable<XElement> loggerXElements;
        private IEnumerable<XElement> nlogNodes;
        private LibSource nlogSource;
        private IEnumerable<XElement> rulesXElements;
        private bool testFoldoutSaveState;
        private bool targetsFoldoutSaveState;
        private bool rulesFoldoutSaveState;
        private bool xmlFoldoutSaveState;
        private AnimBool testFoldoutState;
        private AnimBool targetsFoldoutState;
        private AnimBool rulesFoldoutState;
        private AnimBool xmlFoldoutState;
        private string storedConfig;
        private Rect targetMenuRect = new Rect();
        private Rect layoutMenuRect = new Rect();
        private LogLevels testLogLevels = LogLevels.Everything;
        private IEnumerable<XElement> targetsXElements;
        private Dictionary<TargetAttribute, Type> targetTypeByAttribute;
        private Dictionary<LayoutRendererAttribute, Type> layoutRendererTypeByAttribute;
        private IEnumerable<XElement> targetXElements;
        private IEnumerable<XElement> rootTargetXElements;
        private GameObject TestContext;
        private string testLoggerName = "MyNamespace.MyLoggerName";
        private string testMessage = "NLog test message";
        private bool testHasException;
        private XDocument xDocument;
        private string xmlEditorText;
        private PluginImporter nlogImporter;
        private IEnumerable<PluginImporter> aiUnityImporters;
        private Vector2 scrollPos = Vector2.zero;
        private List<bool> targetFoldoutSaveStates;
        private List<bool> loggerFoldoutSaveStates;
        private PlatformEnumFlagWrapper<BuildTargetGroup> buildTargetGroupFlagWrapper;
        //private PlatformEnumFlagWrapper<RuntimePlatform> runtimePlatformFlagWrapper;
        #endregion

        #region private properties
        /// <summary>
        /// Internal logger singleton
        /// </summary>
        private static IInternalLogger Logger { get { return NLogInternalLogger.Instance; } }

        /// <summary>
        /// Gets a value indicating whether this instance is n log DLL.
        /// </summary>
        private bool IsNLogDll {
            get { return this.nlogSource == LibSource.Dll; }
        }

        /// <summary>
        /// Gets the n log node.
        /// </summary>
        private XElement NLogNode {
            get { return this.nlogNodes.FirstOrDefault(); }
        }

        /// <summary>
        /// Gets the rules xelement.
        /// </summary>
        private XElement RulesXElement {
            get { return this.rulesXElements.FirstOrDefault(); }
        }

        /// <summary>
        /// Gets the targets xelement.
        /// </summary>
        private XElement TargetsXElement {
            get { return this.targetsXElements.FirstOrDefault(); }
        }

        #endregion

        #region private methods
        /// <summary>
        /// Unity Menu entry to launch nlog editor control window.
        /// </summary>
        [MenuItem("Tools/AiUnity/NLog/Control Panel")]
        private static void ControlPanelMenu()
        {
            EditorWindow.GetWindow<NLogEditor>("NLog");
        }

        /// <summary>
        /// Unity Menu entry to launch AiUnity forums website.
        /// </summary>
        [MenuItem("Tools/AiUnity/NLog/Forums")]
        private static void ForumsMenu()
        {
            Application.OpenURL("https://forum.aiunity.com/categories");
        }

        /// <summary>
        /// Unity Menu entry to launch nlog help website.
        /// </summary>
        [MenuItem("Tools/AiUnity/NLog/Help")]
        private static void HelpMenu()
        {
            Application.OpenURL("http://aiunity.com/products/nlog");
        }

        /// <summary>
        /// Implicitly called by Unity for post serialization initialization
        /// </summary>
        private void OnEnable()
        {
            name = "NLogConfigWindow";
            PlayerPrefs.SetInt("AiUnityIsProSkin", Convert.ToInt32(EditorGUIUtility.isProSkin));

            // Used to hold GUI foldout state
            this.testFoldoutState = new AnimBool(this.testFoldoutSaveState);
            this.targetsFoldoutState = new AnimBool(this.targetsFoldoutSaveState);
            this.rulesFoldoutState = new AnimBool(this.rulesFoldoutSaveState);
            this.xmlFoldoutState = new AnimBool(this.xmlFoldoutSaveState);

            // Repaint GUI as foldout animated boolean changes to produce smooth visual effect.
            this.testFoldoutState.valueChanged.AddListener(Repaint);
            this.targetsFoldoutState.valueChanged.AddListener(Repaint);
            this.rulesFoldoutState.valueChanged.AddListener(Repaint);
            this.xmlFoldoutState.valueChanged.AddListener(Repaint);

            this.nlogNodes = Enumerable.Empty<XElement>();
            this.showAdvancedTarget = new Dictionary<XElement, AnimBool>();
            this.targetTypeByAttribute = new Dictionary<TargetAttribute, Type>();
            this.layoutRendererTypeByAttribute = new Dictionary<LayoutRendererAttribute, Type>();
            this.buildTargetGroupFlagWrapper = new PlatformEnumFlagWrapper<BuildTargetGroup>();
            //this.runtimePlatformFlagWrapper = new EnumFlagWrapper<RuntimePlatform>(RuntimePlatform.WindowsEditor | RuntimePlatform.OSXEditor | RuntimePlatform.LinuxEditor, true);
            //this.runtimePlatformFlagWrapper = new PlatformEnumFlagWrapper<RuntimePlatform>();
            this.layoutInsertText = string.Empty;
            this.storedConfig = string.Empty;
            this.IsConfigLoaded = false;
            this.IsConfigValid = false;
            this.xDocument = null;

            SetupPlatforms();
            SetupImporters();
            ReflectAssembly();
            CreateLayoutMenu();
            //FindConfig();
            CheckConfig();
        }

#if AIUNITY_ALPHA
		void FindConfig()
		{
			string[] guids = AssetDatabase.FindAssets(NLogConfigFile.Instance.ConfigFileNameWithoutExtension, new string[] { "Assets" });
			string configName = guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).FirstOrDefault(p => p.Contains("Resources") && p.EndsWith(NLogConfigFile.Instance.ConfigFileName));

			if (string.IsNullOrEmpty(configName)) {
				string nlogPath = Directory.GetDirectories("Assets", "*", SearchOption.AllDirectories).FirstOrDefault(d => d.Contains("NLog"));
				if (string.IsNullOrEmpty(nlogPath)) {
					Logger.Error("Unable to locate NLog directory");
					nlogPath = "Assets";
				}
				configName = Path.Combine(Path.Combine(nlogPath, "Resources"), NLogConfigFile.Instance.ConfigFileName);
			}
			//NLogConfigFile.Instance.Initialize(configName);
			//ConfigFileInfo = new UnityFileInfo(configName);
		}
#endif

        /// <summary>
        /// Check for the existence of a single NLog configuration file
        /// </summary>
        void CheckConfig()
        {
            string[] guids = AssetDatabase.FindAssets(NLogConfigFile.Instance.NameWithoutExtension + " t:TextAsset", null);
            IEnumerable<string> ConfigNames = guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).Where(p => p.Contains("Resources") && p.EndsWith("xml"));

            if (ConfigNames.Count() > 1)
            {
                Logger.Error("Multiple NLog config files found under \"Resources\" directories:" + Environment.NewLine + string.Join(Environment.NewLine, ConfigNames.ToArray()));
            }
        }

        /// <summary>
        /// Implicitly called by Unity to draw Window GUI
        /// </summary>
        private void OnGUI()
        {
            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.Space();
            EditorGUIUtility.labelWidth = 100f;

            DrawConfigGUI();
            DrawEditorSeperator();
            DrawTesterGUI();
            DrawEditorSeperator();
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, false, false);

            if (this.xDocument != null)
            {
                EditorGUI.BeginDisabledGroup(!this.IsConfigValid);

                DrawTargetsGUI();
                DrawEditorSeperator();

                DrawRulesGUI();
                DrawEditorSeperator();

                EditorGUI.EndDisabledGroup();
            }
            if (this.IsConfigLoaded)
            {
                DrawXmlViewerGUI();
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draw the Configuration section of the editor GUI
        /// </summary>
        private void DrawConfigGUI()
        {
            // Refresh Config FileInfo in case it has been altered externally
            NLogConfigFile.Instance.FileInfo.Refresh();

            // Create config GUI
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            int configWidth = Math.Max(350, NLogConfigFile.Instance.RelativeNameWithoutExtension.Length * 10);
            EditorGUILayout.TextField(this.configFileLabel, NLogConfigFile.Instance.RelativeNameWithoutExtension, GUILayout.MinWidth(configWidth));
            EditorGUI.EndDisabledGroup();

            // Mark config as not loaded if file is missing
            if (!NLogConfigFile.Instance.FileInfo.Exists)
            {
                this.IsConfigLoaded = false;
                this.IsConfigValid = false;
                this.xDocument = null;
            }
            else if (!this.IsConfigLoaded)
            {
                LoadParseXML();
            }

            // Create option to create config file if not loaded
            EditorGUI.BeginDisabledGroup(this.IsConfigLoaded);
            if (GUILayout.Button(string.Empty, CustomEditorStyles.PlusIconStyle))
            {
                LoadParseXML(DefaultConfig);
                SaveXML();
            }
            EditorGUI.EndDisabledGroup();

            // Create option to delete config file if loaded
            EditorGUI.BeginDisabledGroup(!this.IsConfigLoaded);
            if (GUILayout.Button(string.Empty, CustomEditorStyles.MinusIconStyle))
            {
                DeleteXML();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
            {
                Application.OpenURL("http://aiunity.com/products/nlog/manual#gui-config-file");
            }
            EditorGUILayout.EndHorizontal();

            if (!this.IsConfigLoaded)
            {
                EditorGUILayout.HelpBox("To configure NLog create a NLog.xml configuration file by clicking the + sign above.  The GUI controls below will then allow you to fully customize NLog.  In the absence of a configuration file the NLog framework will log Fatal, Error, and Warning messages with a default layout to UnityConsole.  Note the NLog framework is solely configured by reading the configuration XML resource file at runtime, leaving it completely independent from the Unity Editor.", MessageType.Info);
            }

            EditorGUI.BeginChangeCheck();

            // Create config source GUI
            this.nlogSource = (LibSource)EditorGUILayout.EnumPopup(this.nlogSourceContent, this.nlogSource);

            // Create config platforms GUI
            this.buildTargetGroupFlagWrapper.EnumFlags = EditorGUILayout.MaskField(this.buildPlatformsContent, this.buildTargetGroupFlagWrapper.EnumFlags, this.buildTargetGroupFlagWrapper.EnumNames.ToArray());
            this.buildTargetGroupFlagWrapper.Add(BuildTargetGroup.Standalone);

            // Create config build levels GUI EditorCommon
            IEnumerable<string> buildLevelDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';')
                .Select(d => d.Trim()).Where(d => d.StartsWith("NLOG_")).Select(d => d.Substring(5).Replace("ALL", "EVERYTHING").ToLower().UppercaseLetter())
                .Where(d => Enum.GetNames(typeof(LogLevels)).Contains(d));
            LogLevels buildLevelsImport = string.Join(", ", buildLevelDefines.ToArray()).ToEnum<LogLevels>();
            LogLevels buildLevels = (LogLevels)EditorGUILayout.EnumFlagsField(this.buildLevelsContent, buildLevelsImport);

            // Source switch requires updates to dll plugIn importer and preprocessor defines
            if (EditorGUI.EndChangeCheck() || AutoSourceDetect())
            {
                Logger.Info("NLog initiating recompile due to changes in DLL importers and Preprocessor defines.");

                // Save XML changes which would be lost due to recompile
                SaveXML();

                // Enable/Disable CLog Plugin Importers based upon settings
                foreach (PluginImporter pluginImporter in this.aiUnityImporters)
                {
                    if (pluginImporter.GetCompatibleWithAnyPlatform() != IsNLogDll || pluginImporter.GetCompatibleWithEditor() != IsNLogDll)
                    {
                        Logger.Info("Setting {0} plugin enable={1}", pluginImporter.assetPath, IsNLogDll);
                        try
                        {
                            pluginImporter.SetCompatibleWithAnyPlatform(IsNLogDll);
                            pluginImporter.SetCompatibleWithEditor(IsNLogDll);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to configure DLL importer={0}", pluginImporter.assetPath);
                        }
                    }
                }

                // Configure defines for selected platforms
                foreach (BuildTargetGroup buildTargetGroup in this.buildTargetGroupFlagWrapper.EnumValues.Where(e => e != BuildTargetGroup.Unknown))
                {
                    List<string> initialDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';').Select(d => d.Trim()).Where(d => d != string.Empty).ToList();
                    List<string> revisedDefines = new List<string>(initialDefines);

                    // Purge existing NLog preprocessor defines
                    foreach (string define in initialDefines)
                    {
                        if (define.Equals("AIUNITY_CODE") || define.StartsWith("NLOG_"))
                        {
                            revisedDefines.Remove(define);
                        }
                    }

                    // Establish NLOG_<Level> preprocessor define to indicate log levels enabled.
                    if (this.buildTargetGroupFlagWrapper.Has(buildTargetGroup))
                    {
                        // Establish AIUNITY_CODE preprocessor define to indicate if Source Code active
                        if (!IsNLogDll)
                        {
                            revisedDefines.Add("AIUNITY_CODE");
                        }

                        if (buildLevels == LogLevels.Everything)
                        {
                            revisedDefines.Add("NLOG_ALL");
                        }
                        else
                        {
                            foreach (LogLevels globalLevel in buildLevels.GetFlags())
                            {
                                revisedDefines.Add("NLOG_" + globalLevel.ToString().ToUpper());
                            }
                        }
                    }

                    if (!Enumerable.SequenceEqual(initialDefines.OrderBy(t => t), revisedDefines.OrderBy(t => t)))
                    {
                        try
                        {
                            // Persist preprocessor defines to Unity
                            Logger.Debug("Updated {0} defines New={1}{3}Old={2}", buildTargetGroup.ToString(), string.Join(";", revisedDefines.ToArray()), string.Join(";", initialDefines.ToArray()), Environment.NewLine);
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", revisedDefines.ToArray()));
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to set preprocessor defines for platform={0}.", buildTargetGroup);
                        }
                    }
                }

                foreach (PluginImporter pluginImporter in this.aiUnityImporters)
                {
                    pluginImporter.SaveAndReimport();
                }
            }

#if AIUNITY_ALPHA
			// Create config editor levels GUI
			GUIContent editorLevelsContent = new GUIContent("Editor levels", EditorLevelsTooltip);
            XAttribute editorLevelsAttribute = NLogNode != null ? NLogNode.Attribute("editorLevels") : null;
			LogLevels defaultEditorLevels = NLogManager.GlobalLogLevelsDefault;
			LogLevels editorLevelsImport = editorLevelsAttribute != null ? editorLevelsAttribute.Value.ToEnum<LogLevels>() : defaultEditorLevels;
			LogLevels editorLevels = NLogManager.Instance.GlobalLogLevel = (LogLevels)EditorGUILayout.EnumFlagsField(editorLevelsContent, editorLevelsImport);
			string editorLevelsUpdate = editorLevels.Equals(LogLevels.Everything) ? "Everything" :
				string.Join(", ", editorLevels.GetFlags().Select(p => p.ToString()).ToArray());
			UpdateAttribute(NLogNode, "editorLevels", editorLevelsUpdate, defaultEditorLevels.ToString());
#endif

            // Create config internal level GUI
            Logger.InternalLogLevel = (LogLevels)EditorGUILayout.EnumFlagsField(this.internalLevelsContent, Logger.InternalLogLevel);

#if AIUNITY_BETA
            // Create Unity log listener GUI
            GUIContent enableUnityLogListenerContent = new GUIContent("Unity Listener", enableUnityLogListenerTooltip);
            bool enableUnityLogListenerParse = false;
            XAttribute enableUnityLogListenerAttribute = NLogNode != null ? NLogNode.Attribute("enableUnityLogListener") : null;
            bool enableUnityLogListenerImport = enableUnityLogListenerAttribute != null && bool.TryParse(enableUnityLogListenerAttribute.Value, out enableUnityLogListenerParse) && enableUnityLogListenerParse;
            bool enableUnityLogListener = EditorGUILayout.Toggle(enableUnityLogListenerContent, enableUnityLogListenerImport);
            UpdateAttribute(NLogNode, "enableUnityLogListener", enableUnityLogListener.ToString(), "False");
#endif

            // Create assert raise exception GUI
            bool assertExceptionParse = false;
            XAttribute assertExceptionAttribute = NLogNode != null ? NLogNode.Attribute("assertException") : null;
            bool assertExceptionImport = assertExceptionAttribute != null && bool.TryParse(assertExceptionAttribute.Value, out assertExceptionParse) && assertExceptionParse;
            bool assertException = EditorGUILayout.Toggle(this.assertExceptionContent, assertExceptionImport);
            UpdateAttribute(NLogNode, "assertException", assertException.ToString(), "False");

            if (this.buildTargetGroupFlagWrapper.EnumFlags == buildTargetGroupFlagWrapper.EnumValueToFlag[BuildTargetGroup.Standalone])
            {
                EditorGUILayout.HelpBox("With \"Platforms\" set to Standalone, log messages on other platforms will be compile out of design.", MessageType.Info);
            }
            if (buildLevels == 0)
            {
                EditorGUILayout.HelpBox("With \"Build levels\" set to Nothing, all log messages will be compiled out of design.", MessageType.Info);
            }
        }

        /// <summary>
        /// Draw the Tester section of the editor GUI
        /// </summary>
        /// <exception cref="Exception">Inner Exception test message.
        /// or
        /// Outer Exception test message.</exception>
        /// <exception cref="System.Exception">Inner Exception test message.
        /// or
        /// Outer Exception test message.</exception>
        private void DrawTesterGUI()
        {
            // Create test logger foldout GUI
            EditorGUILayout.BeginHorizontal();
            GUIContent testFoldoutContent = new GUIContent(string.Format("Test Logger ({0})", this.testLoggerName), "Use the test logger to validate your configuration");
            GUIStyle testStyle = new GUIStyle(EditorStyles.foldout);
            testStyle.margin.right = (int)testStyle.CalcSize(testFoldoutContent).x - 45;
            testStyle.stretchWidth = false;

            this.testFoldoutState.target = EditorGUILayout.Foldout(this.testFoldoutState.target, testFoldoutContent, testStyle);

            // Create test logger play GUI
            Texture2D playImage = EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_PlayButton" : "PlayButton") as Texture2D;
            GUIStyle playStyle = new GUIStyle(EditorStyles.miniButton) { padding = new RectOffset(1, 1, 1, 1) };

            // Create and run test log messages upon user request
            if (GUILayout.Button(playImage, playStyle, GUILayout.Width(20), GUILayout.Height(15)))
            {
                SaveXML();
                NLogger testLogger = NLogManager.Instance.GetLogger(this.testLoggerName, this.TestContext);
                Exception testException = null;
                if (this.testHasException)
                {
                    try { throw new Exception("Inner Exception test message."); }
                    catch (Exception innerException)
                    {
                        try { throw new Exception("Outer Exception test message.", innerException); }
                        catch (Exception outerException) { testException = outerException; }
                    }
                }

                if (this.testLogLevels.Has(LogLevels.Assert))
                {
                    testLogger.Assert(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Fatal))
                {
                    testLogger.Fatal(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Error))
                {
                    testLogger.Error(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Warn))
                {
                    testLogger.Warn(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Info))
                {
                    testLogger.Info(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Debug))
                {
                    testLogger.Debug(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Trace))
                {
                    testLogger.Trace(testException, this.testMessage);
                }
            }


            GUILayout.FlexibleSpace();
            if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
            {
                Application.OpenURL("http://aiunity.com/products/nlog/manual#gui-test-logger");
            }
            EditorGUILayout.EndHorizontal();

            // Fade section of Test Logger GUI section
            if (EditorGUILayout.BeginFadeGroup(this.testFoldoutState.faded))
            {
                EditorGUI.indentLevel++;

                // Create test logger preview GUI
                List<string> logCommands = new List<string>();
                foreach (LogLevels testLogLevelFlag in this.testLogLevels.GetFlags())
                {
                    string assertArgument = testLogLevelFlag == LogLevels.Assert ? "false, " : string.Empty;
                    logCommands.Add(string.Format("logger.{0}({1}\"{2}\")", testLogLevelFlag, assertArgument, this.testMessage));
                }
                if (logCommands.Any())
                {
                    GUIStyle PreviewStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
                    GUIContent previewContent = new GUIContent("Preview", "Logging statements being tested.");
                    EditorGUILayout.LabelField(previewContent, new GUIContent(string.Join(Environment.NewLine, logCommands.ToArray())), PreviewStyle);
                    EditorGUILayout.Space();
                }

                // Create test logger configuration parameter GUIs
                GUIContent testLoggerNameContent = new GUIContent("Name", "Name of logger to be tested.  In code the logger name can be set at logger instantiation or defaults to class name.");
                this.testLoggerName = EditorGUILayout.TextField(testLoggerNameContent, this.testLoggerName);
                GUIContent testContextContent = new GUIContent("Context", "GameObject associated with log message (Optional).  The gameObject gains focus when console log message is double clicked.");
                this.TestContext = EditorGUILayout.ObjectField(testContextContent, this.TestContext, typeof(GameObject), true) as GameObject;
                GUIContent testLevelsContent = new GUIContent("Levels", "A test log statement is generated for each selected level.");
                this.testLogLevels = (LogLevels)EditorGUILayout.EnumFlagsField(testLevelsContent, this.testLogLevels);
                GUIContent testMessageContent = new GUIContent("Message", "The message body of the test log statement.");
                this.testMessage = EditorGUILayout.TextField(testMessageContent, this.testMessage);
                GUIContent testHasExceptionContent = new GUIContent("Exception", "Add a test exception to the logging statements.");
                this.testHasException = EditorGUILayout.Toggle(testHasExceptionContent, this.testHasException);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            // Transform foldout State to a simple bool list that can be serialized
            this.testFoldoutSaveState = this.testFoldoutState.value;
        }

        /// <summary>
        /// Draw the Targets section of the editor GUI
        /// </summary>
        private void DrawTargetsGUI()
        {
            // Create targets foldout GUI
            EditorGUILayout.BeginHorizontal();
            GUIContent targetsFoldoutContent = new GUIContent("Targets", "Targets are the destination of log messages");
            GUIStyle targetsStyle = new GUIStyle(EditorStyles.foldout);
            targetsStyle.margin.right = (int)targetsStyle.CalcSize(targetsFoldoutContent).x - 45;
            targetsStyle.stretchWidth = false;

            this.targetsFoldoutState.target = EditorGUILayout.Foldout(this.targetsFoldoutState.target, targetsFoldoutContent, targetsStyle);
            bool addTarget = GUILayout.Button(string.Empty, CustomEditorStyles.PlusIconStyle);

            // During GUI repaint phase find and use location of last control to place generic menu
            if (Event.current.type == EventType.Repaint)
            {
                this.targetMenuRect = GUILayoutUtility.GetLastRect();
            }
            if (addTarget)
            {
                GenericMenu targetMenu = CreateTargetMenu();
                targetMenu.DropDown(this.targetMenuRect);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
            {
                Application.OpenURL("http://aiunity.com/products/nlog/manual#gui-targets");
            }
            EditorGUILayout.EndHorizontal();

            // Fade section of Targets GUI section
            if (EditorGUILayout.BeginFadeGroup(this.targetsFoldoutState.faded))
            {
                DrawTargetGUI(this.rootTargetXElements.ToList());
            }
            EditorGUILayout.EndFadeGroup();

            // Transform foldout States to a simple bool list that can be serialized
            this.targetsFoldoutSaveState = this.targetsFoldoutState.value;
            this.targetFoldoutSaveStates = this.foldoutStates.Where(x => x.Key.Name.LocalName.Equals("target")).Select(x => x.Value.value).ToList();
        }

        /// <summary>
        /// Draws the target GUI.
        /// </summary>
        /// <param name="targetXElements">The target x elements.</param>
        private void DrawTargetGUI(List<XElement> targetXElements)
        {
            foreach (XElement targetElement in targetXElements)
            {

                int targetBaseIndexLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = targetElement.AncestorsAndSelf(this.DefaultNamespace + "target").Count();

                XAttribute typeAttribute = targetElement.GetOrSetAttribute("type", "UnityConsole");
                XAttribute nameAttribute = targetElement.GetOrSetAttribute("name", typeAttribute.Value);

                // Create target foldout GUI
                EditorGUILayout.BeginHorizontal();
                GUIStyle targetStyle = new GUIStyle(EditorStyles.foldout);
                GUIContent targetFoldoutContent = new GUIContent(string.Format("<target name={0} type={1}>", nameAttribute.Value, typeAttribute.Value));

                // Dynamically targetFoldoutContent if window size too small
                if (position.width - ((int)targetStyle.CalcSize(targetFoldoutContent).x + (12 * EditorGUI.indentLevel)) < 150)
                {
                    targetFoldoutContent = new GUIContent(string.Format("<target name={0}>", nameAttribute.Value));
                }
                targetStyle.margin.right = (int)targetStyle.CalcSize(targetFoldoutContent).x + (12 * EditorGUI.indentLevel) - 40;
                targetStyle.stretchWidth = false;

                this.foldoutStates[targetElement].target = EditorGUILayout.Foldout(this.foldoutStates.GetOrAdd(targetElement).target, targetFoldoutContent, targetStyle);

                bool addSubTarget = GUILayout.Button(string.Empty, CustomEditorStyles.PlusIconStyle);

                // During GUI repaint phase find and use location of last control to place generic menu
                if (Event.current.type == EventType.Repaint)
                {
                    this.targetMenuRect = GUILayoutUtility.GetLastRect();
                }
                if (addSubTarget)
                {
                    GenericMenu targetMenu = CreateTargetMenu(targetElement);
                    targetMenu.DropDown(this.targetMenuRect);
                }

                if (GUILayout.Button(string.Empty, CustomEditorStyles.MinusIconStyle))
                {
                    RemoveFoldoutXElement(targetElement);
                    continue;
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(string.Empty, CustomEditorStyles.CSScriptIconStyle))
                {
                    string guid = AssetDatabase.FindAssets(typeAttribute.Value).FirstOrDefault();
                    if (!string.IsNullOrEmpty(guid))
                    {
                        string assetPath = Application.dataPath.Remove(Application.dataPath.Length - 6) + AssetDatabase.GUIDToAssetPath(guid);
                        InternalEditorUtility.OpenFileAtLineExternal(assetPath, 0);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Missing Asset", string.Format("Unable to find \"{0}\" script.", typeAttribute.Value), "Ok");
                    }
                }

                // Create GUI control to move target up
                EditorGUI.BeginDisabledGroup(!(targetElement.PreviousNode is XElement));
                if (GUILayout.Button("Up", EditorStyles.miniButton))
                {
                    targetElement.MoveElementUp();
                }
                EditorGUI.EndDisabledGroup();

                // Create GUI control to move target down
                EditorGUI.BeginDisabledGroup(!(targetElement.NextNode is XElement));
                if (GUILayout.Button("Down", EditorStyles.miniButton))
                {
                    targetElement.MoveElementDown();
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
                {
                    string anchor = Regex.Replace(typeAttribute.Value, @"(?<=.)([A-Z])", @"-$1").ToLower();
                    Application.OpenURL("http://aiunity.com/products/nlog/manual#" + anchor);
                }

                EditorGUILayout.EndHorizontal();

                // Fade section of Target GUI section
                if (EditorGUILayout.BeginFadeGroup(this.foldoutStates[targetElement].faded))
                {
                    EditorGUI.indentLevel++;
                    IEnumerable<PropertyInfo> targetPropertyInfos = GetTargetPropertyInfo(typeAttribute.Value).Where(p => p.GetAttribute<DisplayAttribute>() != null).OrderBy(p => p.DeclaringType.GetInheritanceDepth()).ThenBy(p => p.GetAttribute<DisplayAttribute>().Order).ThenBy(p => p.GetAttribute<DisplayAttribute>().DisplayName).ThenBy(p => p.Name).ToList();

                    // Increase labelWidth if a target property name exceeds standard label space
                    int maxPropertyLength = targetPropertyInfos
                        .Select(p => (int)targetStyle.CalcSize(new GUIContent(p.GetAttribute<DisplayAttribute>().DisplayName ?? p.Name)).x).DefaultIfEmpty(0).Max();
                    EditorGUIUtility.labelWidth = Math.Min(Math.Max(100f, maxPropertyLength), 150f) + EditorGUI.indentLevel * 20;

                    // Create target name GUI
                    GUIContent nameContent = new GUIContent("Name", "Name used by rules to reference this target");
                    string nameAttributeValue = EditorGUILayout.TextField(nameContent, nameAttribute.Value, GUILayout.ExpandWidth(true));
                    UpdateAttribute(targetElement, "name", nameAttributeValue);

                    // Create target type GUI
                    EditorGUI.BeginDisabledGroup(true);
                    GUIContent typeContent = new GUIContent("Type", "Immutable type of this target.");
                    typeAttribute.Value = EditorGUILayout.TextField(typeContent, typeAttribute.Value, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();

                    IEnumerable<PropertyInfo> basicPropertyInfos = targetPropertyInfos.Where(p => !p.GetAttribute<DisplayAttribute>().Advanced);
                    DrawTargetProperties(targetElement, basicPropertyInfos);

                    IEnumerable<PropertyInfo> advancedPropertyInfos = targetPropertyInfos.Where(p => p.GetAttribute<DisplayAttribute>().Advanced);
                    if (advancedPropertyInfos.Any())
                    {
                        GUIContent advanceContent = new GUIContent("Advance options");
                        GUIStyle advanceStyle = new GUIStyle(EditorStyles.foldout);
                        bool showFoldout = this.showAdvancedTarget.GetOrAdd(targetElement, new AnimBool(true)).target;
                        this.showAdvancedTarget[targetElement].target = EditorGUILayout.Foldout(showFoldout, advanceContent, advanceStyle);

                        if (EditorGUILayout.BeginFadeGroup(this.showAdvancedTarget[targetElement].faded))
                        {
                            DrawTargetProperties(targetElement, advancedPropertyInfos);
                        }
                        EditorGUILayout.EndFadeGroup();
                    }

                    if (targetElement.Elements().Any())
                    {
                        EditorGUILayout.Space();
                        DrawTargetGUI(targetElement.Elements(this.DefaultNamespace + "target").ToList());
                    }
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUI.indentLevel = targetBaseIndexLevel;
            }
        }

        /// <summary>
        /// Draws the target properties.
        /// </summary>
        /// <param name="targetElement">The target element.</param>
        /// <param name="propertyInfos">The property infos.</param>
        private void DrawTargetProperties(XElement targetElement, IEnumerable<PropertyInfo> propertyInfos)
        {
            Type declareType = null;

            foreach (PropertyInfo targetPropertyInfo in propertyInfos)
            {
                if (declareType != null && targetPropertyInfo.DeclaringType != declareType)
                {
                    EditorGUILayout.Space();
                }
                declareType = targetPropertyInfo.DeclaringType;

                string targetPropertyName = targetPropertyInfo.Name.LowercaseLetter();
                string targetDisplayName = targetPropertyInfo.GetAttribute<DisplayAttribute>().DisplayName ?? targetPropertyName;

                XAttribute targetXAttribute = targetElement.Attribute(targetPropertyName);
                string defaultXAttributeValue = targetPropertyInfo.GetAttributes<DefaultValueAttribute>().Select(v => v.Value.ToString()).DefaultIfEmpty(string.Empty).FirstOrDefault();
                string targetXAttributeValue = targetXAttribute != null ? targetXAttribute.Value : defaultXAttributeValue;

                GUIContent targetContent = new GUIContent(targetDisplayName, targetPropertyInfo.GetAttribute<DisplayAttribute>().Tooltip);
                EditorGUILayout.BeginHorizontal();

                if (targetPropertyInfo.PropertyType.Equals(typeof(Layout)))
                {
                    //EditorGUIUtility.labelWidth -= 30;
                    EditorStyles.textField.wordWrap = true;
                    EditorGUILayout.PrefixLabel(targetContent);
                    GUIStyle layoutStyle = new GUIStyle(EditorStyles.textField) { wordWrap = true };
                    targetXAttributeValue = GUILayout.TextField(targetXAttributeValue, layoutStyle);
                    //targetXAttributeValue = EditorGUILayout.TextArea(targetXAttributeValue, layoutStyle);
                    TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    //TextEditor editor = (TextEditor)EditorGUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    //TextEditor editor = typeof(EditorGUI).GetField("activeEditor", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as TextEditor;
                    //EditorGUIUtility.labelWidth += 30;

                    if (this.layoutTarget == targetElement && this.layoutPropertyName == targetPropertyName && !string.IsNullOrEmpty(this.layoutInsertText))
                    {
                        targetXAttributeValue = targetXAttributeValue.Insert(editor.cursorIndex, this.layoutInsertText);
                        this.layoutInsertText = string.Empty;
                    }
                    bool addLayout = GUILayout.Button(string.Empty, CustomEditorStyles.PlusIconStyle);

                    if (Event.current.type == EventType.Repaint)
                    {
                        this.layoutMenuRect = GUILayoutUtility.GetLastRect();
                    }
                    if (addLayout)
                    {
                        this.layoutTarget = targetElement;
                        this.layoutPropertyName = targetPropertyName;
                        this.layoutMenu.DropDown(this.layoutMenuRect);
                    }
                }
                else
                {
                    targetXAttributeValue = CreateTypeBasedGUI(targetPropertyInfo.PropertyType, targetXAttributeValue, targetContent);
                }
                EditorGUILayout.EndHorizontal();
                UpdateAttribute(targetElement, targetPropertyName, targetXAttributeValue, defaultXAttributeValue);
            }

        }

        /// <summary>
        /// Draw the Rules section of the editor GUI
        /// </summary>
        private void DrawRulesGUI()
        {
            // Create rules foldout GUI
            EditorGUILayout.BeginHorizontal();
            GUIContent rulesFoldoutContent = new GUIContent("Rules", "Rules are used to route log messages to targets");
            GUIStyle rulesStyle = new GUIStyle(EditorStyles.foldout);
            rulesStyle.margin.right = (int)rulesStyle.CalcSize(rulesFoldoutContent).x - 45;
            rulesStyle.stretchWidth = false;

            this.rulesFoldoutState.target = EditorGUILayout.Foldout(this.rulesFoldoutState.target, rulesFoldoutContent, rulesStyle);
            if (GUILayout.Button(string.Empty, CustomEditorStyles.PlusIconStyle))
            {
                AddRule();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
            {
                Application.OpenURL("http://aiunity.com/products/nlog/manual#gui-rules");
            }
            EditorGUILayout.EndHorizontal();

            // Sets rule target to None if target was deleted
            foreach (XElement loggerElement in this.loggerXElements.ToList())
            {
                XAttribute targetAttribute = loggerElement.GetOrSetAttribute("target", "UnityConsole");
                if (!this.targetXElements.Any(e => e.Attribute("name").Value == targetAttribute.Value))
                {
                    targetAttribute.Value = "None";
                }
            }
            /*foreach (XAttribute targetAttribute in this.loggerXElements.ToList().Select(e => e.GetOrSetAttribute("target", "UnityConsole"))
                .Where(t => !this.targetXElements.Any(e => e.Attribute("name").Value == t.Value)))
            {
                targetAttribute.Value = "None";
            }*/

            // Fade section of Rules GUI section
            if (EditorGUILayout.BeginFadeGroup(this.rulesFoldoutState.faded))
            {
                EditorGUI.indentLevel++;

                foreach (XElement loggerElement in this.loggerXElements.ToList())
                {
                    XAttribute nameAttribute = loggerElement.GetOrSetAttribute("name", "*");
                    XAttribute namespaceAttribute = loggerElement.GetOrSetAttribute("namespace", "*");
                    XAttribute targetAttribute = loggerElement.GetOrSetAttribute("target", "UnityConsole");

                    bool enableContent;
                    XAttribute loggerEnableAttribute = loggerElement.Attribute("enabled");
                    enableContent = loggerEnableAttribute == null || !bool.TryParse(loggerEnableAttribute.Value, out enableContent) || enableContent;

                    // Create rules foldout GUI
                    EditorGUILayout.BeginHorizontal();
                    GUIContent ruleFoldoutContent = new GUIContent(string.Format("<logger name={0} target={1}>", nameAttribute.Value, targetAttribute.Value));
                    GUIStyle ruleStyle = new GUIStyle(EditorStyles.foldout);

                    ruleStyle.normal.textColor = ruleStyle.onNormal.textColor = enableContent ? EditorStyles.foldout.normal.textColor : new Color(255f / 255f, 140f / 255f, 0);
                    ruleStyle.margin.right = (int)ruleStyle.CalcSize(ruleFoldoutContent).x + (12 * EditorGUI.indentLevel) - 40;
                    ruleStyle.stretchWidth = false;

                    this.foldoutStates[loggerElement].target = EditorGUILayout.Foldout(this.foldoutStates.GetOrAdd(loggerElement).target, ruleFoldoutContent, ruleStyle);
                    if (GUILayout.Button(string.Empty, CustomEditorStyles.MinusIconStyle))
                    {
                        RemoveFoldoutXElement(loggerElement);
                        continue;
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginDisabledGroup(!(loggerElement.PreviousNode is XElement));
                    if (GUILayout.Button("Up", EditorStyles.miniButton))
                    {
                        loggerElement.MoveElementUp();
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(!(loggerElement.NextNode is XElement));
                    if (GUILayout.Button("Down", EditorStyles.miniButton))
                    {
                        loggerElement.MoveElementDown();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();

                    // Fade section of Rule GUI section
                    if (EditorGUILayout.BeginFadeGroup(this.foldoutStates[loggerElement].faded))
                    {
                        EditorGUI.indentLevel++;
                        EditorGUIUtility.labelWidth = 100f + 20f;

                        // Create rules name GUI
                        GUIContent nameContent = new GUIContent("Name", "Pattern that attempts to match against your logger name (\"*\" wildcard permitted).");
                        nameAttribute.Value = EditorGUILayout.TextField(nameContent, nameAttribute.Value);

                        // Create rules namespace GUI
                        GUIContent namespaceContent = new GUIContent("Namespace", "Pattern that attempts to match against your logger namespace (\"*\" wildcard permitted).");
                        namespaceAttribute.Value = EditorGUILayout.TextField(namespaceContent, namespaceAttribute.Value);

                        // Create rules target GUI
                        GUIContent[] targetNames = this.targetXElements.Select(e => e.Attribute("name").Value).MyPrepend("None").Select(s => new GUIContent(s)).ToArray();
                        int targetIndex = Math.Max(Array.FindIndex(targetNames, p => p.text == targetAttribute.Value), 0);
                        GUI.backgroundColor = targetIndex == 0 ? Color.red : Color.white;
                        GUIContent targetContent = new GUIContent("Target", "Target used if rule matches. Wildcard * may be used at beginning and/or end.");
                        targetIndex = EditorGUILayout.Popup(targetContent, targetIndex, targetNames);
                        targetAttribute.Value = targetIndex == 0 ? "None" : targetNames[targetIndex].text;
                        GUI.backgroundColor = Color.white;

                        // Create rules levels GUI
                        XAttribute levelsAttribute = loggerElement.Attribute("levels");
                        LogLevels logLevels = levelsAttribute != null ? levelsAttribute.Value.ToEnum<LogLevels>() : LogLevels.Everything;
                        GUIContent levelsContent = new GUIContent("Levels", "Levels matched by this rule.");
                        logLevels = ((LogLevels)EditorGUILayout.EnumFlagsField(levelsContent, logLevels));
                        string logLevelsUpdate = logLevels.Equals(LogLevels.Everything) ? "Everything" : string.Join(", ", logLevels.GetFlags().Select(p => p.ToString()).ToArray());
                        UpdateAttribute(loggerElement, "levels", logLevelsUpdate, "Everything");

                        // Create rules platforms GUI
                        XAttribute runPlatformAttribute = loggerElement.Attribute("platforms");
                        PlatformEnumFlagWrapper<RuntimePlatform> runtimePlatformFlagWrapper = runPlatformAttribute != null ? runPlatformAttribute.Value : "Everything";
                        GUIContent platformsContent = new GUIContent("Platforms", "Platforms matched by this rule.");
                        runtimePlatformFlagWrapper.EnumFlags = EditorGUILayout.MaskField(platformsContent, runtimePlatformFlagWrapper.EnumFlags, runtimePlatformFlagWrapper.EnumNames.ToArray());
                        UpdateAttribute(loggerElement, "platforms", runtimePlatformFlagWrapper.ToString(), "Everything");

                        // Create rules final GUI
                        bool finalContent;
                        XAttribute loggerFinalAttribute = loggerElement.Attribute("final");
                        finalContent = loggerFinalAttribute != null && bool.TryParse(loggerFinalAttribute.Value, out finalContent) && finalContent;
                        GUIContent finalLabel = new GUIContent("Final", "Final rule if match found.");
                        finalContent = EditorGUILayout.Toggle(finalLabel, finalContent);
                        //string finalValue = finalContent ? "true" : null;
                        UpdateAttribute(loggerElement, "final", finalContent.ToString(), "False");

                        // Create rules enable GUI
                        GUIContent enableLabel = new GUIContent("Enable", "Enable rule.");
                        enableContent = EditorGUILayout.Toggle(enableLabel, enableContent);
                        //string enableValue = enableContent ? null : "false";
                        UpdateAttribute(loggerElement, "enabled", enableContent.ToString(), "True");
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndFadeGroup();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            // Transform foldout States to a simple bool list that can be serialized
            this.rulesFoldoutSaveState = this.rulesFoldoutState.value;
            this.loggerFoldoutSaveStates = this.foldoutStates.Where(x => x.Key.Name.LocalName.Equals("logger")).Select(x => x.Value.value).ToList();
        }

        /// <summary>
        /// Draw the XML Viewer section of the editor GUI
        /// </summary>
        private void DrawXmlViewerGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUIStyle xmlStyle = new GUIStyle(EditorStyles.foldout);
            xmlStyle.onNormal.textColor = this.IsConfigValid ? EditorStyles.foldout.onNormal.textColor : Color.red;

            GUIContent xmlViewerLabel = new GUIContent("XML Viewer", "View and edit NLog XML configuration file directly.  Changes will be reflected in GUI Controls once focus is lost.  For brevity target attributes that match the default value are removed.");
            this.xmlFoldoutState.target = EditorGUILayout.Foldout(this.xmlFoldoutState.target, xmlViewerLabel, xmlStyle);

            if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
            {
                Application.OpenURL("http://aiunity.com/products/nlog/manual#gui-xml-viewer");
            }
            EditorGUILayout.EndHorizontal();

            // Fade section of XML Viewer GUI section
            if (EditorGUILayout.BeginFadeGroup(this.xmlFoldoutState.faded))
            {
                if (this.IsConfigValid && !GUI.GetNameOfFocusedControl().Equals("XMLViewer"))
                {
                    this.xmlEditorText = this.xDocument.ToString();
                }

                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName("XMLViewer");
                this.xmlEditorText = EditorGUILayout.TextArea(this.xmlEditorText);
                if (EditorGUI.EndChangeCheck())
                {
                    LoadParseXML(this.xmlEditorText);
                }
            }
            EditorGUILayout.EndFadeGroup();

            // Transform foldout State to a simple bool list that can be serialized
            this.xmlFoldoutSaveState = this.xmlFoldoutState.value;
        }

        /// <summary>
        /// Called by Unity Internals when windows loses focus
        /// </summary>
        private void OnLostFocus()
        {
            //if (configFileInfo != null && xDocument != null) {
            //SaveXML(configFileInfo);
            //}
            SaveXML();
        }

        /// <summary>
        /// Create and Add the states of the foldout to XElement indexed Dictionary with initial states.
        /// </summary>
        /// <param name="foldoutXElements">Foldout X elements.</param>
        /// <param name="initialStates">The initial states.</param>
        private void AddFoldoutState(IEnumerable<XElement> foldoutXElements, IEnumerable<bool> initialStates = null)
        {
            int index = 0;

            foreach (XElement foldoutXElement in foldoutXElements)
            {
                bool initialState = initialStates != null && initialStates.ElementAtOrDefault(index++);
                AddFoldoutState(foldoutXElement, initialState);
            }
        }

        /// <summary>
        /// Create and Add the state of the foldout to XElement indexed Dictionary with initial state.
        /// </summary>
        /// <param name="foldoutXElement">Foldout X element.</param>
        /// <param name="initialState">If set to <c>true</c> initial state.</param>
        private void AddFoldoutState(XElement foldoutXElement, bool initialState = false)
        {
            this.foldoutStates.Add(foldoutXElement, CreateRepaintAnimBool(initialState));
            this.showAdvancedTarget.Add(foldoutXElement, CreateRepaintAnimBool(false));
        }

        /// <summary>
        /// Adds the nlog rule.
        /// </summary>
        private void AddRule()
        {
            Logger.Debug("Add new rule");
            XElement ruleXElement = new XElement(this.DefaultNamespace + "logger", new XAttribute("name", "*"), new XAttribute("target", "UnityConsole"), new XAttribute("levels", "Assert, Fatal, Error"), new XAttribute("platforms", "Everything"));
            if (RulesXElement != null)
            {
                RulesXElement.Add(ruleXElement);
            }
            else
            {
                if (NLogNode != null)
                {
                    NLogNode.Add(new XElement(this.DefaultNamespace + "rules"));
                    RulesXElement.Add(ruleXElement);
                }
            }
            AddFoldoutState(ruleXElement);
        }

        /// <summary>
        /// Adds a nlog target.
        /// </summary>
        /// <param name="targetTypeName">Name of the target type.</param>
        /// <param name="referenceXElement">The reference x element.</param>
        /// <param name="child">if set to <c>true</c> [child].</param>
        private void AddTarget(string targetTypeName, XElement referenceXElement = null, bool child = true)
        {
            string targetName = targetTypeName;

            for (int i = 1; i < 10; i++)
            {
                if (!this.targetXElements.Any(x => x.Attributes("name").Any(a => a.Value == targetName)))
                {
                    break;
                }
                targetName = targetTypeName + i;
            }

            Logger.Debug("Add new target name = {0}  type = {1}", targetName, targetTypeName);
            XElement targetXElement = new XElement(this.DefaultNamespace + "target", new XAttribute("name", targetName), new XAttribute("type", targetTypeName));

            if (referenceXElement != null)
            {
                if (child)
                {
                    referenceXElement.Add(targetXElement);
                }
                else
                {
                    referenceXElement.AddAfterSelf(targetXElement);
                    referenceXElement.Remove();
                    targetXElement.Add(referenceXElement);
                }
            }
            else
            {
                if (TargetsXElement != null)
                {
                    TargetsXElement.Add(targetXElement);
                }
                else
                {
                    if (NLogNode != null)
                    {
                        NLogNode.Add(new XElement(this.DefaultNamespace + "targets"));
                        TargetsXElement.Add(targetXElement);
                    }
                }
            }
            AddFoldoutState(targetXElement);
        }
        /// <summary>
        /// Creates an animation bool that also triggers GUI Repaint.
        /// </summary>
        /// <param name="initialState">If set to <c>true</c> initial state.</param>
        /// <returns>The repaint animation bool.</returns>
        private AnimBool CreateRepaintAnimBool(bool initialState)
        {
            AnimBool animBool = new AnimBool(initialState);
            animBool.valueChanged.AddListener(Repaint);
            return animBool;
        }

        /// <summary>
        /// Creates the target menu used to display available targets.
        /// </summary>
        /// <param name="referenceXElement">The reference x element.</param>
        /// <returns>GenericMenu.</returns>
        private GenericMenu CreateTargetMenu(XElement referenceXElement = null)
        {
            GenericMenu targetMenu = new GenericMenu();

            bool isWrapperReference = referenceXElement != null && referenceXElement.Attribute("type") != null && this.targetTypeByAttribute.Any(p => p.Key.IsWrapper && p.Key.DisplayName.Equals(referenceXElement.Attribute("type").Value));

            if (referenceXElement == null || isWrapperReference)
            {
                foreach (KeyValuePair<TargetAttribute, Type> target in this.targetTypeByAttribute.Where(p => !p.Key.IsWrapper && !p.Key.IsCompound))
                {
                    string closureTargetTypeName = target.Key.DisplayName;
                    targetMenu.AddItem(new GUIContent(target.Key.DisplayName, "tooltip1"), false, () => AddTarget(closureTargetTypeName, referenceXElement));
                }
                if (this.targetTypeByAttribute.Any(p => p.Key.IsWrapper || p.Key.IsCompound))
                {
                    targetMenu.AddSeparator(string.Empty);
                }
            }

            foreach (KeyValuePair<TargetAttribute, Type> target in this.targetTypeByAttribute.Where(p => p.Key.IsWrapper && !p.Key.IsCompound))
            {
                string closureTargetTypeName = target.Key.DisplayName;
                targetMenu.AddItem(new GUIContent(target.Key.DisplayName, "tooltip2"), false, () => AddTarget(closureTargetTypeName, referenceXElement, false));
            }
            if (this.targetTypeByAttribute.Any(p => p.Key.IsCompound))
            {
                targetMenu.AddSeparator(string.Empty);
            }

            foreach (KeyValuePair<TargetAttribute, Type> target in this.targetTypeByAttribute.Where(p => p.Key.IsCompound))
            {
                string closureTargetTypeName = target.Key.DisplayName;
                targetMenu.AddItem(new GUIContent(target.Key.DisplayName, "tooltip3"), false, () => AddTarget(closureTargetTypeName, referenceXElement, false));
            }

            return targetMenu;
        }

        /// <summary>
        /// Creates the layout menu used to display available layouts.
        /// </summary>
        private void CreateLayoutMenu()
        {
            this.layoutMenu = new GenericMenu();
            foreach (string layoutName in this.layoutRendererTypeByAttribute.Where(p => !p.Key.IsWrapper).Select(p => p.Key.DisplayName))
            {
                string layoutVariable = string.Format("${{{0}}}", layoutName);
                this.layoutMenu.AddItem(new GUIContent(layoutName), false, () => this.layoutInsertText = layoutVariable);
            }
            this.layoutMenu.AddSeparator(string.Empty);

            foreach (string wrapperLayoutName in this.layoutRendererTypeByAttribute.Where(p => p.Key.IsWrapper).Select(p => p.Key.DisplayName))
            {
                string layoutVariable = string.Format(":{0}", wrapperLayoutName);
                this.layoutMenu.AddItem(new GUIContent(wrapperLayoutName), false, () => this.layoutInsertText = layoutVariable);
            }
        }

        /// <summary>
        /// Draw the Configuration section of the editor GUI
        /// </summary>
        private static void DrawEditorSeperator()
        {
            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, CustomEditorStyles.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Gets the property info for the specified type.
        /// </summary>
        /// <param name="targetTypeName">Name of the target type.</param>
        /// <returns>The target property info.</returns>
        private IEnumerable<PropertyInfo> GetTargetPropertyInfo(string targetTypeName)
        {
            Type targetType = this.targetTypeByAttribute.Where(a => a.Key.DisplayName.Equals(targetTypeName)).Select(p => p.Value).FirstOrDefault();

            if (targetType == null)
            {
                Logger.Error("Unable to find NLog Target Type={0} as specified in {1}", targetTypeName, NLogConfigFile.Instance.RelativeName);
                return Enumerable.Empty<PropertyInfo>();
            }
            return targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

#if NLOG_BETA
		// Future feature to allow multiple XML configuration files
		public void CreateConfig(string menuFullName)
		{
			try {
				ProjectNameEditAction ProjectNameEditAction = ScriptableObject.CreateInstance<ProjectNameEditAction>();
				ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ProjectNameEditAction, "Assets", null, menuFullName);
			}
			catch {
				var assetPath = FileService.Instance.GetSelectedPathOrFallback();
				var scriptRelativeName = string.Format("{0}/NewScript.cs", assetPath);
				CreateScript(menuFullName, scriptRelativeName);
			}
		}
#endif

        /// <summary>
        /// Load and parse the configuration XML file.
        /// </summary>
        private void LoadParseXML()
        {
            LoadXML();
            if (this.IsConfigValid)
            {
                ParseXML();
            }
        }

        /// <summary>
        /// Load and parse the configuration XML text.
        /// </summary>
        /// <param name="xmlText">Xml text.</param>
        private void LoadParseXML(string xmlText)
        {
            LoadXML(xmlText);
            if (this.IsConfigValid)
            {
                ParseXML();
            }
        }

        /// <summary>
        /// Load the configuration XML file.
        /// </summary>
        private void LoadXML()
        {
            TextAsset configXMLAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(NLogConfigFile.Instance.RelativeName);
            string configText = configXMLAsset != null ? configXMLAsset.text : string.Empty;

            if (string.IsNullOrEmpty(configText))
            {
                this.IsConfigLoaded = false;
                this.IsConfigValid = false;
                this.xDocument = null;
            }
            else
            {
                try
                {
                    LoadXML(configText);

                    this.storedConfig = this.xDocument.ToString();
                    this.IsConfigLoaded = true;
                    this.IsConfigValid = true;
                }
                catch (XmlException xmlException)
                {

                    Logger.Error("Unable to load Config XML file = " + NLogConfigFile.Instance.RelativeName, xmlException);
                    this.IsConfigLoaded = true;
                    this.IsConfigValid = false;
                    this.xDocument = null;

                    this.storedConfig = this.xmlEditorText = configText;
                }
            }
        }

        /// <summary>
        /// Load the configuration XML text.
        /// </summary>
        /// <param name="xmlText">Xml text.</param>
        private void LoadXML(string xmlText)
        {
            try
            {
                this.xDocument = XDocument.Parse(xmlText);
                this.IsConfigValid = true;
            }
            catch (XmlException)
            {
                this.IsConfigValid = false;
            }
        }

        /// <summary>
        /// Parses the loaded config xDocument for elements pertaining to nlog
        /// </summary>
        private void ParseXML()
        {
            if (this.xDocument != null && this.xDocument.Root != null && this.xDocument.Root.Name.LocalName.Equals("nlog"))
            {
                this.nlogNodes = this.xDocument.Root.Yield();
            }
            else
            {
                this.nlogNodes = Enumerable.Empty<XElement>();
            }

            this.IsConfigValid = this.nlogNodes.Count().Equals(1);
            this.DefaultNamespace = this.nlogNodes.Select(e => e.GetDefaultNamespace()).DefaultIfEmpty(string.Empty).FirstOrDefault();

            this.targetsXElements = this.nlogNodes.Descendants(this.DefaultNamespace + "targets");
            this.targetXElements = this.targetsXElements.Descendants(this.DefaultNamespace + "target");
            this.rootTargetXElements = this.targetsXElements.Elements(this.DefaultNamespace + "target");
            AddFoldoutState(this.targetXElements, this.targetFoldoutSaveStates);

            this.rulesXElements = this.nlogNodes.Descendants(this.DefaultNamespace + "rules");
            this.loggerXElements = this.rulesXElements.Descendants(this.DefaultNamespace + "logger");
            AddFoldoutState(this.loggerXElements, this.loggerFoldoutSaveStates);
        }

        /// <summary>
        /// Save the configuration XML file.
        /// </summary>
        private void SaveXML()
        {
            if (this.xDocument != null && this.IsConfigValid)
            {
                if (!this.xDocument.ToString().Equals(this.storedConfig))
                {
                    Logger.Info("Saving XML File = {0}", NLogConfigFile.Instance.RelativeName);
                    this.xDocument.Save(NLogConfigFile.Instance.FileInfo.FullName);
                    this.storedConfig = this.xDocument.ToString();
                    AssetDatabase.Refresh();
                    NLogManager.Instance.ReloadConfig();
                }
            }
        }

        /// <summary>
        /// Delete the configuration XML file.
        /// </summary>
        //private void DeleteXML(UnityFileInfo configFileInfo)
        private void DeleteXML()
        {
            this.storedConfig = string.Empty;
            this.IsConfigLoaded = false;
            this.IsConfigValid = false;
            this.xDocument = null;

            if (string.IsNullOrEmpty(NLogConfigFile.Instance.RelativeName))
            {
                Logger.Warn("Failed to delete XML File = {0}", NLogConfigFile.Instance.RelativeName);
            }
            else
            {
                Logger.Info("Deleting XML File = {0}", NLogConfigFile.Instance.RelativeName);
                AssetDatabase.DeleteAsset(NLogConfigFile.Instance.RelativeName);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Determine build platforms based upon preprocessor defines
        /// </summary>
        private void SetupPlatforms()
        {
            try
            {
                // Determine current active platforms based upon preprocessor defines
                this.buildTargetGroupFlagWrapper = BuildTargetGroup.Standalone;

                foreach (BuildTargetGroup buildTargetGroup in this.buildTargetGroupFlagWrapper.EnumValues)
                {
                    //BuildTargetGroup buildTargetGroup = platformTarget.ToString().ToEnum<BuildTargetGroup>();
                    string[] defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');
                    if (defines.Any(d => d.Contains("NLOG_")))
                    {
                        this.buildTargetGroupFlagWrapper.Add(buildTargetGroup);
                    }
                }
                /*if (EnumExtensions.GetValues<BuildPlatforms>().All(p => this.buildPlatforms.Has(p)))
                {
                    this.buildPlatforms = BuildPlatforms.Everything;
                }*/
            }
            catch
            {
                Logger.Error("Failed to get preprocessor defines on platform(s): ", string.Join(Environment.NewLine, this.buildTargetGroupFlagWrapper.EnumNames.ToArray()));
            }
        }

        /// <summary>
        /// Get NLog DLL plugin Importers
        /// </summary>
        private void SetupImporters()
        {
            // Load plugin importers related to NLog
            this.aiUnityImporters = PluginImporter.GetAllImporters().Where(p => p.assetPath.Contains("/AiUnity/"));
            this.nlogImporter = this.aiUnityImporters.FirstOrDefault(p => p.assetPath.EndsWith("NLog.dll"));

            this.nlogSource = (this.nlogImporter != null && this.nlogImporter.GetCompatibleWithEditor()) ? LibSource.Dll : LibSource.Code;
        }

        /// <summary>
        /// Automatically determines Source (DLL/Code) based upon plugin importer
        /// </summary>
        /// <returns><c>true</c> if source changed, <c>false</c> otherwise.</returns>
        private bool AutoSourceDetect()
        {
            bool aiUnityCodeDefined = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains("AIUNITY_CODE");

            // Switch to NLog source code if dll plugin importers cannot be located
            if (IsNLogDll)
            {
                if (aiUnityCodeDefined || this.nlogImporter == null || (!this.nlogImporter.GetCompatibleWithAnyPlatform() && !this.nlogImporter.GetCompatibleWithEditor()))
                {
                    if (EditorUtility.DisplayDialog("NLog auto source switch", "Switching to source code due to DLL importer and preprocessor defines settings.", "OK", "Cancel"))
                    {
                        this.nlogSource = LibSource.Code;
                        return true;
                    }
                }
            }
            // Switch to NLog dll if importer enabled
            else if (!aiUnityCodeDefined || (this.nlogImporter != null && (this.nlogImporter.GetCompatibleWithAnyPlatform() || this.nlogImporter.GetCompatibleWithEditor())))
            {
                if (EditorUtility.DisplayDialog("NLog auto source switch", "Switching to DLLs due to DLL importer and preprocessor defines settings.", "OK", "Cancel"))
                {
                    this.nlogSource = LibSource.Dll;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Create Editor GUI that corresponds to propertyType
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="targetXAttributeValue">The target x attribute value.</param>
        /// <param name="targetContent">Content of the target.</param>
        /// <returns>System.String.</returns>
        private string CreateTypeBasedGUI(Type propertyType, string targetXAttributeValue, GUIContent targetContent)
        {
            if (propertyType.IsEnum && !string.IsNullOrEmpty(targetXAttributeValue))
            {
                Enum enumValue = targetXAttributeValue.ToEnum(propertyType);
                try
                {
                    if (propertyType.GetCustomAttributes(typeof(FlagsAttribute), true).Any())
                    {
                        return EditorGUILayout.EnumFlagsField(targetContent, enumValue).ToString();
                    }
                    else
                    {
                        return EditorGUILayout.EnumPopup(targetContent, enumValue).ToString();
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
            else if (propertyType == typeof(Color))
            {
                Color color;
                ColorUtility.TryParseHtmlString(targetXAttributeValue, out color);
                return "#" + ColorUtility.ToHtmlStringRGBA(EditorGUILayout.ColorField(targetContent, color));
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(propertyType))
            {
                return string.Empty;
            }
            else
            {
                switch (Type.GetTypeCode(propertyType))
                {
                    case TypeCode.Boolean:
                        bool boolAttribute = false;
                        bool.TryParse(targetXAttributeValue, out boolAttribute);
                        return EditorGUILayout.Toggle(targetContent, boolAttribute).ToString();
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        int intAttribute;
                        int.TryParse(targetXAttributeValue, out intAttribute);
                        return EditorGUILayout.IntField(targetContent, intAttribute).ToString();
                    case TypeCode.Single:
                        float floatAttribute;
                        float.TryParse(targetXAttributeValue, out floatAttribute);
                        return EditorGUILayout.FloatField(targetContent, floatAttribute).ToString();
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                        double doubleAttribute;
                        double.TryParse(targetXAttributeValue, out doubleAttribute);
                        return EditorGUILayout.DoubleField(targetContent, doubleAttribute).ToString();
                    case TypeCode.Empty:
                    case TypeCode.Object:
                    case TypeCode.DBNull:
                    case TypeCode.DateTime:
                    case TypeCode.Char:
                    case TypeCode.String:
                    default:
                        return EditorGUILayout.TextField(targetContent, targetXAttributeValue);
                }
            }
        }

        /// <summary>
        /// Reflects relevant assemblies to discover elements used by nlog.
        /// </summary>
        private void ReflectAssembly()
        {
            List<string> searchAssemblyNames = new List<string>() { "Assembly-CSharp", Assembly.GetExecutingAssembly().GetName().Name, "NLog" };
            IEnumerable<Assembly> searchAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => searchAssemblyNames.Any(t => a.FullName.StartsWith(t)));
            IEnumerable<Type> targetTypes = searchAssemblies.SelectMany(a => a.GetTypes()).Where(t => t.IsSubclassOf(typeof(Target)) && !t.IsAbstract);

            foreach (Type targetType in targetTypes)
            {
                TargetAttribute targetAttribute = targetType.GetCustomAttributes(typeof(TargetAttribute), true).FirstOrDefault() as TargetAttribute;
                if (targetAttribute != null)
                {
                    this.targetTypeByAttribute[targetAttribute] = targetType;
                }
            }

            IEnumerable<Type> layoutRenderTypes = searchAssemblies.SelectMany(a => a.GetTypes()).Where(t => t.IsSubclassOf(typeof(LayoutRenderer)));
            foreach (Type layoutType in layoutRenderTypes)
            {
                LayoutRendererAttribute layoutAttribute = layoutType.GetCustomAttributes(typeof(LayoutRendererAttribute), true).FirstOrDefault() as LayoutRendererAttribute;
                if (layoutAttribute != null)
                {
                    this.layoutRendererTypeByAttribute[layoutAttribute] = layoutType;
                }
            }
        }

        /// <summary>
        /// Removes the state of the foldout.
        /// </summary>
        /// <param name="foldoutXElement">Foldout XElement.</param>
        private void RemoveFoldoutState(XElement foldoutXElement)
        {
            if (!this.foldoutStates.Remove(foldoutXElement))
            {
                Logger.Assert(false, "Unable to Remove Target Foldout State");
            }
            if (!this.showAdvancedTarget.Remove(foldoutXElement))
            {
                Logger.Assert(false, "Unable to Remove Target Advanced Foldout State");
            }
        }

        /// <summary>
        /// Removes the foldout XElement.
        /// </summary>
        /// <param name="targetXElement">Target XElement.</param>
        private void RemoveFoldoutXElement(XElement targetXElement)
        {
            Logger.Assert(targetXElement != null, "Unable to Remove Target XElement");

            if (targetXElement != null)
            {
                RemoveFoldoutState(targetXElement);
                targetXElement.Remove();
            }
        }

        /// <summary>
        /// Updates the attribute with checks.
        /// </summary>
        /// <param name="xElement">XElement.</param>
        /// <param name="xAttributeName">XAttribute name.</param>
        /// <param name="xAttributeValue">XAttribute value.</param>
        /// <param name="defaultXAttributeValue">The default x attribute value.</param>
        private void UpdateAttribute(XElement xElement, string xAttributeName, string xAttributeValue, string defaultXAttributeValue = null)
        {
            XAttribute xAttribute = xElement != null ? xElement.Attribute(xAttributeName) : null;

            if (xAttributeValue == null || xAttributeValue == defaultXAttributeValue)
            {
                if (xAttribute != null)
                {
                    xAttribute.Remove();
                }
            }
            else if (xAttribute == null)
            {
                if (xElement != null && xAttributeValue != null)
                {
                    xElement.Add(new XAttribute(xAttributeName, xAttributeValue));
                }
            }
            else
            {
                xAttribute.Value = xAttributeValue;
            }
        }
        #endregion
    }
}