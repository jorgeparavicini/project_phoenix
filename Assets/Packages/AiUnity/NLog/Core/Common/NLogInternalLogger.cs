// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 11-30-2016
// Modified         : 08-27-2017
// ***********************************************************************
#if AIUNITY_CODE

using AiUnity.Common.InternalLog;
using AiUnity.Common.Extensions;
using AiUnity.Common.Log;

namespace AiUnity.NLog.Core.Common
{
    /// <summary>
    /// NLog internal logger.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class NLogInternalLogger : InternalLogger<NLogInternalLogger>
    {
        static NLogInternalLogger()
        {
            Instance.Assert(true, "This log statement is executed prior to unity editor serialization due to InitializeOnLoad attribute.  The allows NLog logger to work in all phases of Unity Editor compile (ie. serialization).");
            CommonInternalLogger.Instance.Assert(true, "This log statement is executed prior to unity editor serialization due to InitializeOnLoad attribute.  The allows Common logger to work in all phases of Unity Editor compile (ie. serialization).");
        }
    }
}

#endif