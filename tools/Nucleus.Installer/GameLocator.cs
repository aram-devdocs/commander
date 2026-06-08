using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Nucleus.Installer
{
    /// <summary>Locates the Nuclear Option install (Steam AppId 2168680) on Windows: reads the Steam path from the
    /// registry (reg.exe — no extra deps), parses libraryfolders.vdf for the library that holds the app, and checks
    /// steamapps\common\Nuclear Option. Read-only; never writes.</summary>
    public static class GameLocator
    {
        public const string AppId = "2168680";
        public const string FolderName = "Nuclear Option";

        /// <summary>The game folder, or null if not found.</summary>
        public static string Locate()
        {
            var steam = SteamPath();
            if (steam == null) return null;

            var candidates = new List<string>();
            var vdf = Path.Combine(steam, "steamapps", "libraryfolders.vdf");
            foreach (var lib in LibraryPaths(vdf))
                candidates.Add(Path.Combine(lib, "steamapps", "common", FolderName));
            candidates.Add(Path.Combine(steam, "steamapps", "common", FolderName)); // default library fallback

            foreach (var c in candidates)
                if (IsGameDir(c)) return Path.GetFullPath(c);
            return null;
        }

        /// <summary>True when the folder looks like a Nuclear Option install.</summary>
        public static bool IsGameDir(string dir) =>
            !string.IsNullOrEmpty(dir) && File.Exists(Path.Combine(dir, "NuclearOption.exe"));

        private static string SteamPath()
        {
            var reg = RegQuery(@"HKCU\Software\Valve\Steam", "SteamPath");
            if (reg != null) { reg = reg.Replace('/', '\\'); if (Directory.Exists(reg)) return reg; }
            var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var def = Path.Combine(pf86, "Steam");
            return Directory.Exists(def) ? def : reg;
        }

        private static IEnumerable<string> LibraryPaths(string vdf)
        {
            if (!File.Exists(vdf)) yield break;
            var text = File.ReadAllText(vdf);
            foreach (Match m in Regex.Matches(text, "\"path\"\\s*\"([^\"]+)\""))
                yield return m.Groups[1].Value.Replace("\\\\", "\\");
        }

        // reg.exe query — avoids a Microsoft.Win32.Registry package dependency; Windows-only by nature.
        private static string RegQuery(string key, string value)
        {
            try
            {
                var psi = new ProcessStartInfo("reg", $"query \"{key}\" /v {value}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using var p = Process.Start(psi);
                if (p == null) return null;
                var outp = p.StandardOutput.ReadToEnd();
                p.WaitForExit(5000);
                // line: "    SteamPath    REG_SZ    C:\Program Files (x86)\Steam"
                var m = Regex.Match(outp, value + @"\s+REG_SZ\s+(.+)");
                return m.Success ? m.Groups[1].Value.Trim() : null;
            }
            catch { return null; }
        }
    }
}
