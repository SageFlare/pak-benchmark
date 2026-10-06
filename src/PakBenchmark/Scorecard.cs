using System.Text.Json;
using PakScanner.Findings;

namespace PakBenchmark;

/// <summary>One sample's benchmark outcome: expected vs actual verdict, and scan time.</summary>
public record SampleResult(string Name, string Vector, Verdict Expected, Verdict Actual, double Seconds);

/// <summary>
/// Three-state scorecard over a set of <see cref="SampleResult"/>. Recall is null when the
/// corpus contains no flagged-active samples (so a benign-only run reports false positives without a
/// misleading 0 recall).
/// </summary>
public sealed class Scorecard
{
    public Scorecard(IReadOnlyList<SampleResult> results)
    {
        Results = results;
        Total = results.Count;
        Correct = results.Count(r => r.Expected == r.Actual);

        var benign = results.Where(r => r.Expected == Verdict.Benign).ToList();
        FalsePositiveRate = benign.Count == 0
            ? 0.0
            : (double)benign.Count(r => r.Actual != Verdict.Benign) / benign.Count;

        var malicious = results.Where(r => r.Expected == Verdict.FlaggedActive).ToList();
        Recall = malicious.Count == 0
            ? null
            : (double)malicious.Count(r => r.Actual == Verdict.FlaggedActive) / malicious.Count;

        PerVector = results
            .GroupBy(r => r.Vector)
            .ToDictionary(
                g => g.Key,
                g => (hit: g.Count(r => r.Expected == r.Actual), miss: g.Count(r => r.Expected != r.Actual)));
    }

    public IReadOnlyList<SampleResult> Results { get; }
    public int Total { get; }
    public int Correct { get; }
    public double Accuracy => Total == 0 ? 0.0 : (double)Correct / Total;
    public double FalsePositiveRate { get; }
    public double? Recall { get; }
    public IReadOnlyDictionary<string, (int hit, int miss)> PerVector { get; }

    public string ToJson() => JsonSerializer.Serialize(new
    {
        total = Total,
        correct = Correct,
        accuracy = Accuracy,
        falsePositiveRate = FalsePositiveRate,
        recall = Recall,
        perVector = PerVector.ToDictionary(kv => kv.Key, kv => new { hit = kv.Value.hit, miss = kv.Value.miss }),
        samples = Results.Select(r => new
        {
            r.Name, r.Vector, expected = r.Expected.ToString(), actual = r.Actual.ToString(), r.Seconds,
        }),
    }, new JsonSerializerOptions { WriteIndented = true });
}
