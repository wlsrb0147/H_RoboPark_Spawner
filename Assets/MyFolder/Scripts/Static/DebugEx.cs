using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public static class DebugEx
{
#if UNITY_EDITOR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(object message)
    {
        Debug.LogError(message);
    }
#else
    public static void Log(object message,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        Debug.Log($"[{DateTime.Now:HH:mm:ss}] ({System.IO.Path.GetFileName(file)}:{line}) {message}");
    }

    public static void LogWarning(object message,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        Debug.LogWarning($"[{DateTime.Now:HH:mm:ss}] ({System.IO.Path.GetFileName(file)}:{line}) {message}");
    }

    public static void LogError(object message,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        Debug.LogError($"[{DateTime.Now:HH:mm:ss}] ({System.IO.Path.GetFileName(file)}:{line}) {message}");
    }
#endif
}