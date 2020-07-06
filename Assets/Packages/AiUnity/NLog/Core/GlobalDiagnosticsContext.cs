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

namespace AiUnity.NLog.Core
{
    using System;
    using AiUnity.NLog.Core.Common;
    using System.Collections.Generic;
    using AiUnity.Common.InternalLog;
    using AiUnity.Common.Log;

    /// <summary>
    /// Global Diagnostics Context - a dictionary structure to hold per-application-instance values.
    /// </summary>
    //public static class GlobalDiagnosticsContext : IVariablesContext
    public sealed class GlobalDiagnosticsContext : IVariablesContext
    {
        private static Dictionary<string, string> contextVariables = new Dictionary<string, string>();

        /// <summary>
        /// Sets the Global Diagnostics Context item to the specified value.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <param name="value">Item value.</param>
        public static void Set(string item, string value)
        {
            lock (contextVariables) {
                contextVariables[item] = value;
            }
        }

        void IVariablesContext.Set(string key, object value)
        {
            GlobalDiagnosticsContext.Set(key, value.ToString());
        }

        /// <summary>
        /// Gets the Global Diagnostics Context named item.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>The item value of string.Empty if the value is not present.</returns>
        public static string Get(string item)
        {
            lock (contextVariables)
            {
                string s;

                if (!contextVariables.TryGetValue(item, out s))
                {
                    s = string.Empty;
                }

                return s;
            }
        }

        object IVariablesContext.Get(string key)
        {
            return GlobalDiagnosticsContext.Get(key);
        }

        /// <summary>
        /// Checks whether the specified item exists in the Global Diagnostics Context.
        /// </summary>
        /// <param name="item">Item name.</param>
        /// <returns>A boolean indicating whether the specified item exists in current thread GDC.</returns>
        public static bool Contains(string item)
        {
            lock (contextVariables)
            {
                return contextVariables.ContainsKey(item);
            }
        }

        bool IVariablesContext.Contains(string key)
        {
            return GlobalDiagnosticsContext.Contains(key);
        }

        /// <summary>
        /// Removes the specified item from the Global Diagnostics Context.
        /// </summary>
        /// <param name="item">Item name.</param>
        public static void Remove(string item)
        {
            lock (contextVariables)
            {
                contextVariables.Remove(item);
            }
        }

        void IVariablesContext.Remove(string key)
        {
            GlobalDiagnosticsContext.Remove(key);
        }

        /// <summary>
        /// Clears the content of the GDC.
        /// </summary>
        public static void Clear()
        {
            lock (contextVariables)
            {
                contextVariables.Clear();
            }
        }

        void IVariablesContext.Clear()
        {
            GlobalDiagnosticsContext.Clear();
        }
    }
}
#endif
