using System.Diagnostics;
using System.Text.Json;
using PakScanner;
using PakScanner.Findings;

namespace PakBenchmark;

/// <summary>
/// Runs a scanner over a labeled corpus and scores the results. The gate encodes the
/// push/no-push decision: no benign sample may score non-benign, and no flagged-active sample may
/// score benign.
/// </summary>
public sealed class Grader
{
    private readonly SecurityScanner _scanner;

    public Grader(SecurityScanner scanner) => _scanner = scanner;

    public Scorecard Run(string corpusDir, out IReadOnlyList<SampleResult> results)
    {
        var manifestPath = Path.Combine(corpusDir, "manifest.json");
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("corpus manifest not found", manifestPath);

        var entries = JsonSerializer.Deserialize<List<ManifestEntry>>(File.ReadAllText(manifestPath))
                      ?? throw new InvalidOperationException("manifest.json did not parse to a list");

        var list = new List<SampleResult>();
        foreach (var e in entries)
        {
            var pak = Path.Combine(corpusDir, "samples", e.Name + ".pak");
            if (!File.Exists(pak))
                throw new FileNotFoundException($"sample pak missing for '{e.Name}'", pak);

            var expected = ManifestEntry.ParseState(e.ExpectedState);
            var sw = Stopwatch.StartNew();
            var scan = _scanner.Scan(pak);
            sw.Stop();
            list.Add(new SampleResult(e.Name, e.Vector, expected, scan.Verdict, sw.Elapsed.TotalSeconds));
        }

        results = list;
        return new Scorecard(list);
    }

    /// <summary>
    /// The push/no-push gate. Fails if:
    /// - any benign sample scored non-benign (false positive), or
    /// - any flagged-active sample scored anything other than flagged-active (a real miss of
    ///   execution-reachability — scoring it benign OR flagged-latent both understate a pak that
    ///   actually runs on load).
    /// </summary>
    public static (bool pass, string reason) Gate(Scorecard sc)
    {
        var falsePositives = sc.Results.Where(r => r.Expected == Verdict.Benign && r.Actual != Verdict.Benign).ToList();
        if (falsePositives.Count > 0)
            return (false, $"{falsePositives.Count} benign sample(s) scored non-benign: "
                           + string.Join(", ", falsePositives.Select(r => $"{r.Name}->{r.Actual}")));

        var missed = sc.Results.Where(r => r.Expected == Verdict.FlaggedActive && r.Actual != Verdict.FlaggedActive).ToList();
        if (missed.Count > 0)
            return (false, $"{missed.Count} flagged-active sample(s) under-scored: "
                           + string.Join(", ", missed.Select(r => $"{r.Name}->{r.Actual}")));

        return (true, "all gates passed");
    }
}
