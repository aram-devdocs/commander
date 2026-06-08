using System;
using System.IO;

namespace Nucleus.Installer
{
    /// <summary>Flips the single Doorstop `enabled` flag in doorstop_config.ini. Install leaves it FALSE so a plain
    /// Steam launch is vanilla; the modded launcher flips it TRUE for the session and restores FALSE on exit.
    /// (BepInEx 5 ships Doorstop 3, which has no env-enable — the ini flag is the toggle.)</summary>
    public static class DoorstopToggle
    {
        private static string IniPath(string gameDir) => Path.Combine(gameDir, "doorstop_config.ini");

        public static bool TryGet(string gameDir, out bool enabled)
        {
            enabled = false;
            var ini = IniPath(gameDir);
            if (!File.Exists(ini)) return false;
            foreach (var line in File.ReadAllLines(ini))
            {
                var t = line.TrimStart();
                if (t.StartsWith("enabled", StringComparison.OrdinalIgnoreCase) &&
                    !t.StartsWith("debug", StringComparison.OrdinalIgnoreCase) && t.Contains("="))
                {
                    enabled = t.Substring(t.IndexOf('=') + 1).Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                    return true;
                }
            }
            return false;
        }

        public static void SetEnabled(string gameDir, bool on)
        {
            var ini = IniPath(gameDir);
            if (!File.Exists(ini)) return;
            var lines = File.ReadAllLines(ini);
            for (int i = 0; i < lines.Length; i++)
            {
                var t = lines[i].TrimStart();
                if (t.StartsWith("enabled", StringComparison.OrdinalIgnoreCase) &&
                    !t.StartsWith("debug", StringComparison.OrdinalIgnoreCase) && t.Contains("="))
                {
                    lines[i] = "enabled = " + (on ? "true" : "false");
                    File.WriteAllLines(ini, lines);
                    return;
                }
            }
        }
    }
}
