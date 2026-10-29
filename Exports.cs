using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MarkdownPreviewPlugin
{
    // Native entry points Notepad++ looks for. DNNE turns every
    // [UnmanagedCallersOnly] method here into an export of MarkdownPreview.dll.
    public static class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "isUnicode", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static int IsUnicode()
        {
            return 1;
        }

        [UnmanagedCallersOnly(EntryPoint = "setInfo", CallConvs = new[] { typeof(CallConvCdecl) })]
        [DNNE.C99DeclCode("typedef struct { void* npp; void* main; void* second; } NppData;")]
        public static void SetInfo([DNNE.C99Type("NppData")] NppData data)
        {
            try { Plugin.SetInfo(data); } catch { }
        }

        [UnmanagedCallersOnly(EntryPoint = "getName", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static IntPtr GetName()
        {
            try { return Plugin.GetName(); } catch { return IntPtr.Zero; }
        }

        [UnmanagedCallersOnly(EntryPoint = "getFuncsArray", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static IntPtr GetFuncsArray(IntPtr nbF)
        {
            try { return Plugin.GetFuncsArray(nbF); } catch { return IntPtr.Zero; }
        }

        [UnmanagedCallersOnly(EntryPoint = "beNotified", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static void BeNotified(IntPtr notification)
        {
            try { Plugin.BeNotified(notification); } catch { }
        }

        [UnmanagedCallersOnly(EntryPoint = "messageProc", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static IntPtr MessageProc(uint msg, IntPtr wParam, IntPtr lParam)
        {
            return (IntPtr)1;
        }
    }
}
