using System.Diagnostics;
using UnityEngine;

namespace VertigoCase.Utils
{
    /// <summary>
    /// Development-only logging gateway. Calls and their arguments are omitted from non-development
    /// player builds, avoiding both console noise and message-allocation cost in production.
    /// </summary>
    public static class GameLog
    {
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Info(string message, Object context = null)
        {
            if (context == null)
                UnityEngine.Debug.Log(message);
            else
                UnityEngine.Debug.Log(message, context);
        }
    }
}
