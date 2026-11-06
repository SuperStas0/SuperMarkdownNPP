using System;
using System.Drawing;

namespace SuperMarkdown
{
    internal static class Styles
    {
        public const string Default = "github";

        // Index order matches the style items in the plugin menu.
        public static readonly string[] Names = { "github", "gitlab", "forgejo" };
        public static readonly string[] Titles = { "GitHub", "GitLab", "Forgejo" };

        public static string Normalize(string style)
        {
            foreach (string n in Names)
            {
                if (string.Equals(n, style, StringComparison.OrdinalIgnoreCase)) return n;
            }
            return Default;
        }

        public static int IndexOf(string style)
        {
            return Array.IndexOf(Names, Normalize(style));
        }

        // Page background, so the panel does not flash before the web view paints.
        public static Color Background(string style, string theme)
        {
            if (theme == "light") return Color.White;
            switch (Normalize(style))
            {
                case "gitlab": return Color.FromArgb(31, 30, 36);
                case "forgejo": return Color.FromArgb(22, 23, 24);
                default: return Color.FromArgb(13, 17, 23);
            }
        }
    }
}
