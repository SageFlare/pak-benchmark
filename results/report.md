# pak-benchmark scorecard

- Samples: 9
- Accuracy: 100%
- False-positive rate: 0%
- Recall: 100%
- Gate: **PASS** — all gates passed

## Per-vector

| vector | hit | miss |
| --- | --- | --- |
| asset_replacement | 1 | 0 |
| benign | 3 | 0 |
| launch_url | 4 | 0 |
| web_widget | 1 | 0 |

## Samples

| sample | vector | expected | actual | seconds |
| --- | --- | --- | --- | --- |
| benign_map | benign | Benign | Benign | 0.727 |
| benign_cosmetic | benign | Benign | Benign | 0.007 |
| asset_replacement_attempt | asset_replacement | FlaggedLatent | FlaggedLatent | 0.008 |
| launch_url_attempt | launch_url | FlaggedLatent | FlaggedLatent | 0.007 |
| zz_launch_url_delivered | launch_url | FlaggedActive | FlaggedActive | 0.007 |
| zz_launch_url_markerless | launch_url | FlaggedActive | FlaggedActive | 0.007 |
| trojan_decoy | launch_url | FlaggedActive | FlaggedActive | 0.007 |
| benign_modpack | benign | Benign | Benign | 0.006 |
| zz_web_widget | web_widget | FlaggedLatent | FlaggedLatent | 0.007 |
