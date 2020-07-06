// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 02-17-2017
// Modified         : 06-26-2018
// ***********************************************************************
#if AIUNITY_CODE
namespace AiUnity.NLog.Core.Targets
{
    using AiUnity.Common.Attributes;
    using AiUnity.Common.Extensions;
    using AiUnity.Common.InternalLog;
    using AiUnity.Common.Log;
    using AiUnity.NLog.Core.Common;
    using AiUnity.NLog.Core.Config;
    using AiUnity.NLog.Core.Layouts;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.Scripting;

    /// <summary>
    /// Class GameConsoleTarget. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="AiUnity.NLog.Core.Targets.UnityConsoleTarget" />
    [Target("GameConsole")]
    [Preserve]
    public sealed class GameConsoleTarget : UnityConsoleTarget
    {
        #region Constants
        private const bool ConsoleActiveDefault = true;
        private const int FontSizeDefault = 8;
        private const bool IconEnableDefault = true;
        // TextMesh Pro available colors by name http://digitalnativestudios.com/textmeshpro/docs/rich-text/
        private const string GameLayoutDefault = "<color=orange>[${level}] ${callsite}</color>${newline}<color=white>${message}</color><color=red>${exception}</color>";
        private const LogLevels LogLevelsFilterDefault = LogLevels.Assert | LogLevels.Fatal | LogLevels.Error | LogLevels.Warn;
        #endregion

        #region Properties
        [RequiredParameter]
        [DefaultValue(ConsoleActiveDefault)]
        [Display("Start console", "Make Game Console Log window active at startup.", false)]
        public bool ConsoleActive { get; set; }

        [RequiredParameter]
        [DefaultValue(FontSizeDefault)]
        [Display("Font size", "Font size for game console", false)]
        public int FontSize { get; set; }

        [RequiredParameter]
        [DefaultValue(IconEnableDefault)]
        //[Display("Enable Icon", "Display shortcut icon when NLOG Console minimized.  Alternatively NLOG Console can be restored with a gesture (TBD).", false)]
        public bool IconEnable { get; set; }

        /// <summary>
        /// Gets or sets the layout used to format log messages.
        /// </summary>
        [RequiredParameter]
        [DefaultValue(GameLayoutDefault)]
        [Display("Layout", "Specifies the layout and content of log message.  The + icon will present a list of variables that can be added.  For the Unity Console all excepted xml formating can be used (http://docs.unity3d.com/Manual/StyledText.html)", false, -100)]
        public override Layout Layout { get; set; }

        [RequiredParameter]
        [DefaultValue(LogLevelsFilterDefault)]
        [Display("Filter levels", "Starting log level filter that is runtime adjustable.", false)]
        public LogLevels LogLevelsFilter { get; set; }

        // Internal logger singleton
        private static IInternalLogger Logger { get { return NLogInternalLogger.Instance; } }

        private IGameConsoleController GameConsoleController { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GameConsoleTarget"/> class.
        /// </summary>
        /// <remarks>The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code></remarks>
        public GameConsoleTarget()
        {
            if (Application.isPlaying)
            {
                Layout = GameLayoutDefault;
                LogLevelsFilter = LogLevelsFilterDefault;
                FontSize = FontSizeDefault;
                IconEnable = IconEnableDefault;
                ConsoleActive = ConsoleActiveDefault;

                Scene activeScene = SceneManager.GetActiveScene();
                UpdateNLogMessageTarget(activeScene);
                SceneManager.activeSceneChanged += (s1, s2) => UpdateNLogMessageTarget(s2);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes logging event to the log target.
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        /// <exception cref="AssertException"></exception>
        protected override void Write(LogEventInfo logEvent)
        {
            if (Application.isPlaying && GameConsoleController != null)
            {
                //GameConsoleController.AddMessage(logEvent.LoggerName, (int)logEvent.Level, logEvent.TimeStamp, FixUnityConsoleXML(Layout.Render(logEvent)));
                string logMessage = FixUnityConsoleXML(Layout.Render(logEvent) + Environment.NewLine);
                GameConsoleController.AddMessage((int)logEvent.Level, logMessage, logEvent.LoggerName, logEvent.TimeStamp);

                if (logEvent.Level.Has(LogLevels.Assert) && NLogManager.Instance.AssertException)
                {
                    Debug.Break();
                    throw new AssertException(this.Layout.Render(logEvent), logEvent.Exception);
                }
            }
        }

        /// <summary>
        /// Updates the log message target.
        /// </summary>
        /// <param name="scene">The scene.</param>
        void UpdateNLogMessageTarget(Scene scene)
        {
            GameConsoleController = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<IGameConsoleController>()).FirstOrDefault();
            if (GameConsoleController != null)
            {
                GameConsoleController.SetIconEnable(IconEnable);
                GameConsoleController.SetConsoleActive(ConsoleActive);
                GameConsoleController.SetFontSize(FontSize);
                GameConsoleController.SetLogLevelFilter(LogLevelsFilter);
            }
            else
            {
                Logger.Warn("Unable to locate GameConsole GameObject.  Please place NLog prefab GameConsole in your hierarchy.");
            }
        }
        #endregion
    }
}
#endif
