using System.Text.Json.Serialization;
using PakScanner.Findings;

namespace PakBenchmark;

/// <summary>One labeled corpus sample, as stored in pak-corpus/manifest.json.</summary>
public record ManifestEntry(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("vector")] string Vector,
    [property: JsonPropertyName("intent")] string Intent,
    [property: JsonPropertyName("expected_state")] string ExpectedState,
    [property: JsonPropertyName("notes")] string Notes)
{
    /// <summary>Parse a manifest state string to a Verdict (case-insensitive; throws on unknown).</summary>
    public static Verdict ParseState(string state) => state?.Trim().ToLowerInvariant() switch
    {
        "benign" => Verdict.Benign,
        "attempted" => Verdict.Attempted,
        "malicious" => Verdict.Malicious,
        _ => throw new ArgumentException($"unknown expected_state: '{state}'"),
    };
}
