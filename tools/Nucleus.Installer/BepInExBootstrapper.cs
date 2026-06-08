using System;
using System.IO;
using System.IO.Compression;

namespace Nucleus.Installer
{
    /// <summary>Ensures BepInEx 5 (x64, Mono) is present in the game folder — downloads + extracts the latest BepInEx 5
    /// release if missing. Only adds files alongside the game (winhttp.dll + BepInEx\); never touches game assemblies.</summary>
    public static class BepInExBootstrapper
    {
        // BepInEx 5 win x64 asset (Mono build). The 5.x line ships the Doorstop winhttp proxy + the Preloader.
        private const string Bep5Asset = @"BepInEx_win_x64_5\..*\.zip$";

        public static bool IsInstalled(string gameDir) =>
            File.Exists(Path.Combine(gameDir, "winhttp.dll")) &&
            Directory.Exists(Path.Combine(gameDir, "BepInEx", "core"));

        /// <summary>Install BepInEx if absent. Returns true if it is present afterwards.</summary>
        public static bool EnsureInstalled(string gameDir, Action<string> log)
        {
            if (IsInstalled(gameDir)) { log("BepInEx already present."); return true; }

            var asset = GitHubReleases.Latest("BepInEx/BepInEx", Bep5Asset);
            if (asset?.Url == null) { log("ERROR: could not resolve a BepInEx 5 download from GitHub."); return false; }

            var tmp = Path.Combine(Path.GetTempPath(), "nucleus_bepinex.zip");
            log($"Downloading BepInEx {asset.Tag} ({asset.Name})...");
            GitHubReleases.Download(asset.Url, tmp);
            log("Extracting BepInEx into the game folder...");
            ZipFile.ExtractToDirectory(tmp, gameDir, overwriteFiles: true);
            try { File.Delete(tmp); } catch { /* temp cleanup best-effort */ }

            // First run normally generates the config; we just need the loader present now.
            return IsInstalled(gameDir);
        }
    }
}
