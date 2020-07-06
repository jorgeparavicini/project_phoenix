// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 02-17-2017
// Modified         : 06-26-2018
// ***********************************************************************
using System;

#if AIUNITY_CODE

namespace AiUnity.NLog.Core.Targets
{
    using AiUnity.Common.Attributes;
    using AiUnity.Common.Extensions;
    using AiUnity.Common.Log;
    using AiUnity.NLog.Core.Common;
    using AiUnity.NLog.Core.Config;
    using AiUnity.NLog.Core.Layouts;
    using System.ComponentModel;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine.Scripting;

    /// <summary>
    /// Class UnityConsoleTarget.
    /// </summary>
    /// <seealso cref="AiUnity.NLog.Core.Targets.TargetWithLayout" />
    [Target("UnityConsole")]
    public class UnityConsoleTarget : TargetWithLayout
    {
#region Constants
        private const string LayoutDefault = "<color=olive>[${level}] ${callsite}</color>${newline}<color=black>${message}</color>${newline}<color=red>${exception}</color>";
#endregion

#region Properties
        /// <summary>
        /// Gets or sets the layout used to format log messages.
        /// </summary>
        [RequiredParameter]
        [DefaultValue(LayoutDefault)]
        [Display("Layout", "Specifies the layout and content of log message.  The + icon will present a list of variables that can be added.  For the Unity Console all excepted xml formating can be used (http://docs.unity3d.com/Manual/StyledText.html)", false, -100)]
        [Preserve]
        public override Layout Layout { get; set; }
#endregion

#region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UnityConsoleTarget"/> class.
        /// </summary>
        /// <remarks>The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code></remarks>
        public UnityConsoleTarget()
        {
            //bool isDarkTheme = Application.isEditor && Convert.ToBoolean(PlayerPrefs.GetInt("AiUnityIsProSkin", 0));
            Layout = LayoutDefault;
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
            string logMessage = Layout.Render(logEvent);

            MonoBehaviour monoBehaviour = logEvent.Context as MonoBehaviour;
            GameObject unityGameObject = monoBehaviour != null ? monoBehaviour.gameObject : logEvent.Context as GameObject;

            if (logEvent.Level.Has(LogLevels.Info) || logEvent.Level.Has(LogLevels.Debug) || logEvent.Level.Has(LogLevels.Trace))
            {
                Debug.Log(logMessage, unityGameObject);
            }
            else if (logEvent.Level.Has(LogLevels.Warn))
            {
                Debug.LogWarning(logMessage, unityGameObject);
            }
            else
            {
                Debug.LogError(logMessage, unityGameObject);

                if (logEvent.Level.Has(LogLevels.Assert) && NLogManager.Instance.AssertException)
                {
                    Debug.Break();
                    throw new AssertException(logMessage, logEvent.Exception); ;
                }
            }
        }

        /// <summary>
        /// Unity Console literally shows the Rich Text XML if an active tag crosses the second line boundary.
        /// For example if <b> exists on line 2 and </b> on line 3 then all the XML tags will unfortunately be shown.
        /// This method will close any open tags at the end of line 2 and then reopen those tags on line 3.
        /// </summary>
        /// <param name="message">Message that will have open tags corrected.</param>
        /// <returns>String with no tags crossing the second line boundary.</returns>
        protected string FixUnityConsoleXML(string message)
        {
            Match secondLineEndMatch = Regex.Match(message.TrimEnd('\n'), @"\r?\n").NextMatch();
            int secondLineEndIndex = secondLineEndMatch.Index;

            if (secondLineEndIndex > 0)
            {
                Stack<string> tags = new Stack<string>();

                string messageConsole = message.Substring(0, secondLineEndIndex);
                //MatchCollection startTags = Regex.Matches(messageConsole, @"(<\w+[^<>]*>)|(</\w+[^<>]*>)");
                MatchCollection startTags = Regex.Matches(messageConsole, @"(<(b|i|size|color)\s*>)|(</(b|i|size|color)\s*)");

                foreach (Match match in startTags)
                {
                    if (string.IsNullOrEmpty(match.Groups[1].Value))
                    {
                        // If tags do not match up just return original message
                        if (tags.Count() == 0)
                        {
                            return message;
                        }
                        tags.Pop();
                    }
                    else
                    {
                        tags.Push(match.Groups[1].Value);
                    }
                }

                // Alter message if second line leaves outstanding tags
                if (tags.Count != 0) {
                    StringBuilder sb = new StringBuilder(messageConsole);

                    // End outstanding tags
                    foreach (string tag in tags)
                    {
                        Match tagName = Regex.Match(tag, @"<(\w+)[^<>]*>");
                        sb.AppendFormat("</{0}>", tagName.Groups[1].Value);
                    }
                    sb.AppendLine();

                    // Restart outstanding tags
                    sb.Append(string.Join("", tags.Reverse().ToArray()));

                    // Add back third line and above back to the message
                    sb.Append(message.Substring(secondLineEndIndex + secondLineEndMatch.Value.Length));
                    return sb.ToString();
                }
            }
            return message;
        }

#endregion
    }
}
#endif
