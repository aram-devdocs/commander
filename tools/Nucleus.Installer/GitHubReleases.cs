using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace Nucleus.Installer
{
    /// <summary>Minimal GitHub Releases client (no octokit dep): find the latest release tag + a matching asset, and
    /// download files. Used to bootstrap BepInEx and to self-update the mod from the Nucleus releases.</summary>
    public static class GitHubReleases
    {
        public const string NucleusRepo = "aram-devdocs/no_nucleus";

        public sealed class Asset { public string Tag; public string Name; public string Url; }

        private static HttpClient NewClient()
        {
            var c = new HttpClient();
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Nucleus.Installer");
            c.Timeout = TimeSpan.FromSeconds(60);
            return c;
        }

        /// <summary>Latest release of <paramref name="repo"/> + the first asset whose name matches
        /// <paramref name="assetPattern"/> (regex), or null when none. Tag is always returned when the release exists.</summary>
        public static Asset Latest(string repo, string assetPattern)
        {
            try
            {
                using var c = NewClient();
                var json = c.GetStringAsync($"https://api.github.com/repos/{repo}/releases/latest").GetAwaiter().GetResult();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var tag = root.TryGetProperty("tag_name", out var t) ? t.GetString() : null;
                if (tag == null) return null;
                var result = new Asset { Tag = tag };
                if (root.TryGetProperty("assets", out var assets) && assetPattern != null)
                    foreach (var a in assets.EnumerateArray())
                    {
                        var name = a.GetProperty("name").GetString();
                        if (name != null && Regex.IsMatch(name, assetPattern))
                        {
                            result.Name = name;
                            result.Url = a.GetProperty("browser_download_url").GetString();
                            break;
                        }
                    }
                return result;
            }
            catch { return null; }
        }

        public static void Download(string url, string outFile)
        {
            using var c = NewClient();
            using var s = c.GetStreamAsync(url).GetAwaiter().GetResult();
            using var fs = File.Create(outFile);
            s.CopyTo(fs);
        }

        /// <summary>Compare two "vX.Y.Z" / "X.Y.Z" version strings; >0 when <paramref name="a"/> is newer.</summary>
        public static int CompareVersions(string a, string b)
        {
            Version Pa(string s) => Version.TryParse((s ?? "0").TrimStart('v', 'V'), out var v) ? v : new Version(0, 0, 0);
            return Pa(a).CompareTo(Pa(b));
        }
    }
}
