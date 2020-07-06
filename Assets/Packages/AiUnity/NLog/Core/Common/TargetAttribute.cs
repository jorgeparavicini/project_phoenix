#if AIUNITY_CODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Attributes;
using System.ComponentModel;

namespace AiUnity.NLog.Core.Common
{
    using System;
    //using AiUnity.NLog.Core.Config;

    /// <summary>
    /// Marks class as a logging target and assigns a name to it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TargetAttribute : DisplayNameAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TargetAttribute" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public TargetAttribute(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether to the target is a wrapper target (used to generate the target summary documentation page).
        /// </summary>
        public bool IsWrapper { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to the target is a compound target (used to generate the target summary documentation page).
        /// </summary>
        public bool IsCompound { get; set; }
    }
}
#endif