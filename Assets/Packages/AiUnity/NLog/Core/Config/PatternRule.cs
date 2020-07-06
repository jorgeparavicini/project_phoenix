#if AIUNITY_CODE
namespace AiUnity.NLog.Core.Config
{
    using UnityEngine;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.Collections.Generic;
    using System;
	using AiUnity.Common.InternalLog;
    using AiUnity.NLog.Core.Common;

    public class PatternMatch
    {


    private string loggerNamePattern;
        private MatchMode loggerNameMatchMode;
        private string loggerNameMatchArgument;

        // Internal logger singleton
        private static IInternalLogger Logger { get { return NLogInternalLogger.Instance; } }

        /// <summary>
        /// Gets or sets logger name pattern.
        /// </summary>
        /// <remarks>
        /// Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends but not anywhere else.
        /// </remarks>
        public string LoggerNamePattern
        {
        get
        {
            return this.loggerNamePattern;
        }

        set
        {
            this.loggerNamePattern = value;
            int firstPos = this.loggerNamePattern.IndexOf('*');
            int lastPos = this.loggerNamePattern.LastIndexOf('*');

            if (firstPos < 0) {
                this.loggerNameMatchMode = MatchMode.Equals;
                this.loggerNameMatchArgument = value;
                return;
            }

            if (firstPos == lastPos) {
                string before = this.LoggerNamePattern.Substring(0, firstPos);
                string after = this.LoggerNamePattern.Substring(firstPos + 1);

                if (before.Length > 0) {
                    this.loggerNameMatchMode = MatchMode.StartsWith;
                    this.loggerNameMatchArgument = before;
                    return;
                }

                if (after.Length > 0) {
                    this.loggerNameMatchMode = MatchMode.EndsWith;
                    this.loggerNameMatchArgument = after;
                    return;
                }

                return;
            }

            // *text*
            if (firstPos == 0 && lastPos == this.LoggerNamePattern.Length - 1) {
                string text = this.LoggerNamePattern.Substring(1, this.LoggerNamePattern.Length - 2);
                this.loggerNameMatchMode = MatchMode.Contains;
                this.loggerNameMatchArgument = text;
                return;
            }

            this.loggerNameMatchMode = MatchMode.None;
            this.loggerNameMatchArgument = string.Empty;
        }
    }



        public PatternMatch(string loggerNamePattern = "*")
        {
            this.LoggerNamePattern = loggerNamePattern;
        }

        /// <summary>
        /// Checks whether given name matches the logger name pattern.
        /// </summary>
        /// <param name="loggerName">String to be matched.</param>
        /// <returns>A value of <see langword="true"/> when the name matches, <see langword="false" /> otherwise.</returns>
        public bool NameMatches(string loggerName)
        {
            Logger.Debug("Matching rule = {0} (Pattern={1}) LoggerName={2}", this.loggerNameMatchArgument, this.loggerNamePattern, loggerName);

            switch (this.loggerNameMatchMode) {
                case MatchMode.All:
                    return true;

                default:
                case MatchMode.None:
                    return false;

                case MatchMode.Equals:
                    return loggerName.Equals(this.loggerNameMatchArgument, StringComparison.Ordinal);

                case MatchMode.StartsWith:
                    return loggerName.StartsWith(this.loggerNameMatchArgument, StringComparison.Ordinal);

                case MatchMode.EndsWith:
                    return loggerName.EndsWith(this.loggerNameMatchArgument, StringComparison.Ordinal);

                case MatchMode.Contains:
                    return loggerName.IndexOf(this.loggerNameMatchArgument, StringComparison.Ordinal) >= 0;
            }
        }

        public override string ToString()
        {
            return string.Format("logNamePattern: ({0}:{1})", this.loggerNameMatchArgument, this.loggerNameMatchMode);
        }

        internal enum MatchMode
        {
            All,
            None,
            Equals,
            StartsWith,
            EndsWith,
            Contains,
        }


    }
}
#endif