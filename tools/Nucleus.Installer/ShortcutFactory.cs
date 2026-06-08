using System;
using System.Diagnostics;
using System.IO;

namespace Nucleus.Installer
{
    /// <summary>Creates the "Play Nuclear Option (Nucleus)" desktop shortcut via the WScript.Shell COM object
    /// (shelled through PowerShell — guaranteed on Windows). The shortcut runs the installer's `launch` verb
    /// minimized, with our custom icon.</summary>
    public static class ShortcutFactory
    {
        public const string ShortcutName = "Play Nuclear Option (Nucleus)";

        public static string DesktopPath() =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), ShortcutName + ".lnk");

        /// <summary>Create/refresh the desktop shortcut. minimized=true so the launcher's console doesn't intrude.</summary>
        public static bool Create(string targetExe, string args, string workingDir, string iconPath, Action<string> log)
        {
            var lnk = DesktopPath();
            string Q(string s) => (s ?? "").Replace("'", "''");
            var icon = string.IsNullOrEmpty(iconPath) || !File.Exists(iconPath) ? targetExe : iconPath;
            var script =
                "$s=(New-Object -ComObject WScript.Shell).CreateShortcut('" + Q(lnk) + "');" +
                "$s.TargetPath='" + Q(targetExe) + "';" +
                "$s.Arguments='" + Q(args) + "';" +
                "$s.WorkingDirectory='" + Q(workingDir) + "';" +
                "$s.IconLocation='" + Q(icon) + "';" +
                "$s.WindowStyle=7;" +                       // minimized
                "$s.Description='Launch Nuclear Option with the Nucleus mod';" +
                "$s.Save()";
            try
            {
                var psi = new ProcessStartInfo("powershell",
                    "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"" + script.Replace("\"", "\\\"") + "\"")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true };
                using var p = Process.Start(psi);
                p?.WaitForExit(15000);
                if (File.Exists(lnk)) { log("Created shortcut: " + lnk); return true; }
                log("WARNING: shortcut was not created (continuing).");
                return false;
            }
            catch (Exception ex) { log("WARNING: shortcut creation failed: " + ex.Message); return false; }
        }

        public static void Remove(Action<string> log)
        {
            try { var lnk = DesktopPath(); if (File.Exists(lnk)) { File.Delete(lnk); log("Removed shortcut: " + lnk); } }
            catch (Exception ex) { log("WARNING: could not remove shortcut: " + ex.Message); }
        }
    }
}
