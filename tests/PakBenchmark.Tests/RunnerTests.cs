using PakBenchmark;
using Xunit;

namespace PakBenchmark.Tests;

public class RunnerTests
{
    private static string RealCorpus =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "pak-corpus"));

    [Fact]
    public void Run_against_real_corpus_passes_gate_and_writes_scorecard()
    {
        var outDir = Path.Combine(Path.GetTempPath(), "benchout_" + Guid.NewGuid().ToString("N"));
        try
        {
            var exit = Runner.Run(RealCorpus, outDir);
            Assert.Equal(0, exit); // gate passed: no benign scored non-benign
            Assert.True(File.Exists(Path.Combine(outDir, "scorecard.json")));
            Assert.True(File.Exists(Path.Combine(outDir, "report.md")));
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
        }
    }
}
