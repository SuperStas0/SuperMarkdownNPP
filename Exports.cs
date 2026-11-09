using System;
using System.Runtime.InteropServices;

namespace SuperMarkdown
{
    // Called from the native shim (shim\shim.c) through hostfxr.
    // The shim itself exports isUnicode, getName and messageProc.
    public static class Exports
    {
        [UnmanagedCallersOnly]
        public static void SetInfo(NppData nppData)
        {
            try { Plugin.SetInfo(nppData); } catch { }
        }

        [UnmanagedCallersOnly]
        public static IntPtr GetFuncsArray(IntPtr nbF)
        {
            try { return Plugin.GetFuncsArray(nbF); } catch { return IntPtr.Zero; }
        }

        [UnmanagedCallersOnly]
        public static void BeNotified(IntPtr notification)
        {
            try { Plugin.BeNotified(notification); } catch { }
        }
    }
}
