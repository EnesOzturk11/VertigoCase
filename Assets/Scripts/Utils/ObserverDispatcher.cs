using System;
using UnityEngine;

namespace VertigoCase.Utils
{
    /// <summary>Isolates view observers so one faulty listener cannot block the remaining UI.</summary>
    internal static class ObserverDispatcher
    {
        public static void Notify<T>(Action<T> observers, T value, UnityEngine.Object context)
        {
            if (observers == null) return;

            foreach (Action<T> observer in observers.GetInvocationList())
            {
                try
                {
                    observer(value);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, context);
                }
            }
        }

        public static void Notify<TFirst, TSecond>(
            Action<TFirst, TSecond> observers,
            TFirst first,
            TSecond second,
            UnityEngine.Object context)
        {
            if (observers == null) return;

            foreach (Action<TFirst, TSecond> observer in observers.GetInvocationList())
            {
                try
                {
                    observer(first, second);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, context);
                }
            }
        }
    }
}
