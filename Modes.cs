using System;

namespace SuperMarkdown
{
    internal static class Modes
    {
        public const string Hidden = "hidden";
        public const string Sidebar = "sidebar";
        public const string Replace = "replace";

        // Index order matches the mode items in the plugin menu.
        public static readonly string[] Names = { Hidden, Sidebar, Replace };

        public static string Normalize(string mode)
        {
            foreach (string n in Names)
            {
                if (string.Equals(n, mode, StringComparison.OrdinalIgnoreCase)) return n;
            }
            return Hidden;
        }
    }
}
