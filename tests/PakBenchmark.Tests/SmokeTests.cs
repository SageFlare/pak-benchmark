using Xunit;

namespace PakBenchmark.Tests;

public class SmokeTests
{
    [Fact]
    public void Version_is_present() =>
        Assert.False(string.IsNullOrEmpty(PakBenchmark.BenchVersion.Current));
}
