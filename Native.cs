using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SuperMarkdown
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NppData
    {
        public IntPtr NppHandle;
        public IntPtr ScintillaMainHandle;
        public IntPtr ScintillaSecondHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public int Left, Top, Right, Bottom;
    }

    // Notepad++ tTbData (docking dialog registration)
    [StructLayout(LayoutKind.Sequential)]
    internal struct TbData
    {
        public IntPtr hClient;
        public IntPtr pszName;
        public int dlgID;
        public uint uMask;
        public IntPtr hIconTab;
        public IntPtr pszAddInfo;
        public Rect rcFloat;
        public int iPrevCont;
        public IntPtr pszModuleName;
    }

    internal static class Native
    {
        public const uint WM_USER = 0x400;
        public const uint NPPMSG = WM_USER + 1000;

        public const uint NPPM_GETCURRENTSCINTILLA = NPPMSG + 4;
        public const uint NPPM_DMMSHOW = NPPMSG + 30;
        public const uint NPPM_DMMHIDE = NPPMSG + 31;
        public const uint NPPM_DMMREGASDCKDLG = NPPMSG + 33;
        public const uint NPPM_GETFULLCURRENTPATH = WM_USER + 3000 + 1;

        public const uint NPPN_READY = 1001;
        public const uint NPPN_FILESAVED = 1008;
        public const uint NPPN_SHUTDOWN = 1009;
        public const uint NPPN_BUFFERACTIVATED = 1010;
        public const uint NPPN_LANGCHANGED = 1011;

        public const uint SCN_MODIFIED = 2008;

        public const uint SCI_GETLENGTH = 2006;
        public const uint SCI_GETCODEPAGE = 2137;
        public const uint SCI_GETTEXT = 2182;

        public const uint DWS_DF_CONT_RIGHT = 1u << 28;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
    }
}
