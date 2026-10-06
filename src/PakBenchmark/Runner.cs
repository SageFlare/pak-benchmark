using System.Text;
using PakScanner;
using PakScanner.Rules;

namespace PakBenchmark;

/// <summary>Runs the benchmark end to end and writes outputs. Returns a process exit code.</summary>
public static class Runner
{
    public static int Run(string corpusDir, string outDir)
    {
        var scanner = new SecurityScanner(new ISecurityRule[]
        {
            new AssetReplacementRule(),
            new LaunchUrlRule(),
            new HiddenModRule(),
        });

        var scorecard = new Grader(scanner).Run(corpusDir, out var results);
        var (pass, reason) = Grader.Gate(scorecard);

        Directory.CreateDirectory(outDir);
        File.WriteAllText(Path.Combine(outDir, "scorecard.json"), scorecard.ToJson());
        File.WriteAllText(Path.Combine(outDir, "report.md"), RenderReport(scorecard, pass, reason));

        Console.WriteLine($"Samples: {scorecard.Total}  Accuracy: {scorecard.Accuracy:P0}  "
                          + $"FP-rate: {scorecard.FalsePositiveRate:P0}  "
                          + $"Recall: {(scorecard.Recall is { } r ? r.ToString("P0") : "N/A")}");
        Console.WriteLine($"Gate: {(pass ? "PASS" : "FAIL")} — {reason}");
        return pass ? 0 : 1;
    }

    private static string RenderReport(Scorecard sc, bool pass, string reason)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# pak-benchmark scorecard");
        sb.AppendLine();
        sb.AppendLine($"- Samples: {sc.Total}");
        sb.AppendLine($"- Accuracy: {sc.Accuracy:P0}");
        sb.AppendLine($"- False-positive rate: {sc.FalsePositiveRate:P0}");
        sb.AppendLine($"- Recall: {(sc.Recall is { } r ? r.ToString("P0") : "N/A (no flagged-active samples in corpus)")}");
        sb.AppendLine($"- Gate: **{(pass ? "PASS" : "FAIL")}** — {reason}");
        sb.AppendLine();
        sb.AppendLine("## Per-vector");
        sb.AppendLine();
        sb.AppendLine("| vector | hit | miss |");
        sb.AppendLine("| --- | --- | --- |");
        foreach (var kv in sc.PerVector.OrderBy(k => k.Key))
            sb.AppendLine($"| {kv.Key} | {kv.Value.hit} | {kv.Value.miss} |");
        sb.AppendLine();
        sb.AppendLine("## Samples");
        sb.AppendLine();
        sb.AppendLine("| sample | vector | expected | actual | seconds |");
        sb.AppendLine("| --- | --- | --- | --- | --- |");
        foreach (var s in sc.Results)
            sb.AppendLine($"| {s.Name} | {s.Vector} | {s.Expected} | {s.Actual} | {s.Seconds:F3} |");
        return sb.ToString();
    }
}
