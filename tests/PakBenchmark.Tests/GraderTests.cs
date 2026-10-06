using PakBenchmark;
using PakScanner;
using PakScanner.Rules;
using Xunit;

namespace PakBenchmark.Tests;

public class GraderTests
{
    private static string RealSamples =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..",
            "pak-corpus", "samples"));

    // Build an isolated temp corpus: manifest.json + samples/ copied from the real benign paks.
    private static string MakeCorpus(params (string name, string state)[] entries)
    {
        var dir = Path.Combine(Path.GetTempPath(), "benchcorpus_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(dir, "samples"));
        var json = "[" + string.Join(",", entries.Select(e =>
            $"{{\"name\":\"{e.name}\",\"vector\":\"benign\",\"intent\":\"t\",\"expected_state\":\"{e.state}\",\"notes\":\"\"}}")) + "]";
        File.WriteAllText(Path.Combine(dir, "manifest.json"), json);
        foreach (var e in entries)
            File.Copy(Path.Combine(RealSamples, e.name + ".pak"),
                      Path.Combine(dir, "samples", e.name + ".pak"), overwrite: true);
        return dir;
    }

    private static Grader NewGrader() =>
        new(new SecurityScanner(new ISecurityRule[] { new AssetReplacementRule(), new LaunchUrlRule() }));

    [Fact]
    public void Benign_corpus_scores_perfect_and_gate_passes()
    {
        var dir = MakeCorpus(("benign_map", "benign"), ("benign_cosmetic", "benign"));
        try
        {
            var sc = NewGrader().Run(dir, out var results);
            Assert.Equal(2, results.Count);
            Assert.Equal(1.0, sc.Accuracy);
            Assert.Equal(0.0, sc.FalsePositiveRate);
            var (pass, _) = Grader.Gate(sc);
            Assert.True(pass);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void Mislabeling_a_benign_as_malicious_fails_gate_via_missed_recall()
    {
        // The pak really scans Benign; labeling it malicious makes it a missed detection.
        var dir = MakeCorpus(("benign_map", "flagged-active"));
        try
        {
            var sc = NewGrader().Run(dir, out _);
            Assert.Equal(0.0, sc.Recall);
            var (pass, reason) = Grader.Gate(sc);
            Assert.False(pass);
            Assert.False(string.IsNullOrEmpty(reason));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void Missing_pak_throws()
    {
        var dir = Path.Combine(Path.GetTempPath(), "benchcorpus_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(dir, "samples"));
        File.WriteAllText(Path.Combine(dir, "manifest.json"),
            "[{\"name\":\"ghost\",\"vector\":\"benign\",\"intent\":\"t\",\"expected_state\":\"benign\",\"notes\":\"\"}]");
        try
        {
            Assert.ThrowsAny<Exception>(() => NewGrader().Run(dir, out _));
        }
        finally { Directory.Delete(dir, true); }
    }
}
