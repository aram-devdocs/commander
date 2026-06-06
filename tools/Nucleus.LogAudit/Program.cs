using System;
using System.IO;
using System.Linq;
using Nucleus.LogAudit;

if (args.Length < 1)
{
    Console.Error.WriteLine("usage: Nucleus.LogAudit <BepInEx-log-path> [--json]");
    return 2;
}

var path = args[0];
if (!File.Exists(path))
{
    Console.Error.WriteLine($"log not found: {path}");
    return 2;
}

var report = LogAuditor.Analyze(File.ReadAllLines(path));

if (args.Contains("--json"))
{
    Console.WriteLine(report.ToJson());
}
else
{
    foreach (var c in report.Checks)
        Console.WriteLine($"[{(c.Pass ? "PASS" : "FAIL")}] {c.Name}{(string.IsNullOrEmpty(c.Detail) ? "" : " - " + c.Detail)}");
    if (report.Exceptions.Count > 0)
    {
        Console.WriteLine("--- first exception/error lines ---");
        foreach (var e in report.Exceptions.Take(5)) Console.WriteLine("  " + e);
    }
    Console.WriteLine($"LOG-AUDIT: {(report.Pass ? "PASS" : "FAIL")}");
}

return report.Pass ? 0 : 1;
