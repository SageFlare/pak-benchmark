using PakBenchmark;
using PakScanner.Findings;
using Xunit;

namespace PakBenchmark.Tests;

public class ScorecardTests
{
    private static SampleResult R(string name, string vector, Verdict expected, Verdict actual) =>
        new(name, vector, expected, actual, 0.01);

    [Fact]
    public void All_benign_correct_has_fpr_zero_recall_null()
    {
        var sc = new Scorecard(new[]
        {
            R("a", "benign", Verdict.Benign, Verdict.Benign),
            R("b", "benign", Verdict.Benign, Verdict.Benign),
        });
        Assert.Equal(0.0, sc.FalsePositiveRate);
        Assert.Null(sc.Recall);
        Assert.Equal(1.0, sc.Accuracy);
    }

    [Fact]
    public void A_benign_scored_malicious_raises_fpr()
    {
        var sc = new Scorecard(new[]
        {
            R("a", "benign", Verdict.Benign, Verdict.FlaggedActive),
            R("b", "benign", Verdict.Benign, Verdict.Benign),
        });
        Assert.Equal(0.5, sc.FalsePositiveRate);
        Assert.Equal(0.5, sc.Accuracy);
    }

    [Fact]
    public void Malicious_caught_gives_recall_one_missed_gives_zero()
    {
        var caught = new Scorecard(new[] { R("m", "asset_replacement", Verdict.FlaggedActive, Verdict.FlaggedActive) });
        Assert.Equal(1.0, caught.Recall);

        var missed = new Scorecard(new[] { R("m", "asset_replacement", Verdict.FlaggedActive, Verdict.Benign) });
        Assert.Equal(0.0, missed.Recall);
    }

    [Fact]
    public void ParseState_is_case_insensitive_and_rejects_unknown()
    {
        Assert.Equal(Verdict.FlaggedActive, ManifestEntry.ParseState("flagged-active"));
        Assert.Equal(Verdict.FlaggedLatent, ManifestEntry.ParseState("Flagged-Latent"));
        Assert.Throws<ArgumentException>(() => ManifestEntry.ParseState("nope"));
    }
}
