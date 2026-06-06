using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Nucleus.LogAudit
{
    public sealed class AuditCheck
    {
        public string Name { get; set; } = "";
        public bool Pass { get; set; }
        public string Detail { get; set; } = "";
    }

    public sealed class AuditReport
    {
        public bool Pass { get; set; }
        public List<AuditCheck> Checks { get; set; } = new();
        public List<string> Exceptions { get; set; } = new();
        public Dictionary<string, int> Metrics { get; set; } = new();

        public string ToJson() =>
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Turns a BepInEx/Player.log into a structured PASS/FAIL verdict for a playtest: did the plugin load, did
    /// the Harmony patches apply, did the host-driven tick reach the runtime, and were there any exceptions.
    /// Pure string analysis so it is fully unit-testable headlessly.
    /// </summary>
    public static class LogAuditor
    {
        public static AuditReport Analyze(IEnumerable<string> lines, int expectedPatches = 4)
        {
            var all = lines as IList<string> ?? lines.ToList();
            var r = new AuditReport();

            bool loaded = all.Any(l => l.Contains("Commander Layer loaded"));
            int patches = all.Count(l => l.Contains("Patched: "));
            bool firstTick = all.Any(l => l.Contains("first Tick"));

            var exceptions = all
                .Where(l => l.Contains("Exception") || l.Contains(" threw") || l.Contains("Unhandled"))
                .Distinct()
                .Take(25)
                .ToList();

            r.Metrics["patches"] = patches;
            r.Metrics["exceptions"] = exceptions.Count;
            r.Exceptions = exceptions;

            Add(r, "plugin-loaded", loaded, loaded ? "" : "'Commander Layer loaded.' not found");
            Add(r, "patches-applied", patches >= expectedPatches, $"{patches}/{expectedPatches}");
            Add(r, "runtime-tick", firstTick, firstTick ? "" : "'first Tick' not found (host tick may not reach the runtime)");
            Add(r, "no-exceptions", exceptions.Count == 0, exceptions.Count == 0 ? "" : $"{exceptions.Count} exception/error line(s)");

            r.Pass = r.Checks.All(c => c.Pass);
            return r;
        }

        private static void Add(AuditReport r, string name, bool pass, string detail) =>
            r.Checks.Add(new AuditCheck { Name = name, Pass = pass, Detail = detail });
    }
}
