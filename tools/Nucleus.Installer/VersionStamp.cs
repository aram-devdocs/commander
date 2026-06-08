using System;
using System.IO;
using System.Reflection;

namespace Nucleus.Installer
{
    /// <summary>Records which Nucleus version is installed (a stamp the `update` check compares against the latest
    /// GitHub release). Lives next to the plugins so it travels with the install.</summary>
    public static class VersionStamp
    {
        private static string StampPath(string gameDir) =>
            Path.Combine(gameDir, "BepInEx", "plugins", "nucleus-version.txt");

        public static string ReadInstalled(string gameDir)
        {
            var p = StampPath(gameDir);
            return File.Exists(p) ? File.ReadAllText(p).Trim() : null;
        }

        public static void Write(string gameDir, string version)
        {
            var p = StampPath(gameDir);
            Directory.CreateDirectory(Path.GetDirectoryName(p));
            File.WriteAllText(p, version);
        }

        /// <summary>The version this installer ships: a `nucleus-version.txt` next to the payload, else the assembly version.</summary>
        public static string Shipping(string sourceDir)
        {
            var f = Path.Combine(sourceDir, "nucleus-version.txt");
            if (File.Exists(f)) return File.ReadAllText(f).Trim();
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v != null ? $"{v.Major}.{v.Minor}.{v.Build}" : "0.0.0";
        }
    }
}
