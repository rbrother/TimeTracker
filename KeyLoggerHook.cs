using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace KeyLoggerGui
{
    static class KeyLoggerHook
    {
        [DllImport("kbhook.dll")]
        extern public static void SetKBHook();

        [DllImport("kbhook.dll")]
        extern public static uint GetKeyStrokes();

        [DllImport("kbhook.dll")]
        extern public static void KillKBHook();
    }
}
