# pak-benchmark

Grades [pak-scanner](https://github.com/SageFlare/pak-scanner) against the labeled
[pak-corpus](https://github.com/SageFlare/pak-corpus): runs the scanner over every sample,
compares each verdict to the manifest's ground truth, and reports a three-state scorecard
(benign / attempted / malicious) — false-positive rate, recall, per-vector hit/miss — plus
per-sample timing.

Part of a 3-repo system: pak-corpus (dataset) · pak-scanner (detector) · **pak-benchmark**
(this, the grader).

## What it answers

- Does the scanner keep benign mods benign? (false-positive rate)
- Does it catch malicious ones? (recall — reported once the corpus includes malicious/attempt
  samples; N/A while the corpus is benign-only)

## Build & run

Requires the **.NET 8 SDK**.

```
dotnet test
dotnet run --project src/PakBenchmark -- --corpus ../pak-corpus --out out
```

Writes `out/scorecard.json` and `out/report.md`. Exit code 0 when the gate passes (no benign
scored non-benign; no malicious scored benign), 1 when it fails.

## License

GPLv3 (see [LICENSE](LICENSE)); it transitively links the GPLv3 UnchainedLauncher via
pak-scanner. See [NOTICE](NOTICE).
