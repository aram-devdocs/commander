using System;
using System.Diagnostics;
using System.IO;

namespace Nucleus.Installer
{
    /// <summary>Launches Nuclear Option WITH Nucleus: flips Doorstop on, starts the game, waits, and restores
    /// Doorstop off so the next plain Steam launch is vanilla again. The desktop shortcut runs this minimized.</summary>
    public static class Launcher
    {
        public static int LaunchModded(string gameDir, Action<string> log)
        {
            var exe = Path.Combine(gameDir, "NuclearOption.exe");
            if (!File.Exists(exe)) { log($"ERROR: NuclearOption.exe not found in {gameDir}"); return 1; }
            if (!BepInExBootstrapper.IsInstalled(gameDir) && !BepInExBootstrapper.EnsureInstalled(gameDir, log))
            { log("ERROR: BepInEx is not installed; run 'install' first."); return 1; }

            DoorstopToggle.SetEnabled(gameDir, true);
            try
            {
                var psi = new ProcessStartInfo(exe) { WorkingDirectory = gameDir, UseShellExecute = true };
                var p = Process.Start(psi);
                log("Launched Nuclear Option with Nucleus — fly freely, the AI commands your side.");
                p?.WaitForExit();
                return 0;
            }
            finally
            {
                // Restore vanilla for the next Steam launch.
                DoorstopToggle.SetEnabled(gameDir, false);
            }
        }
    }
}
