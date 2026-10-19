# LandmarkEmulator.Parser

**Tool** (console program). A research tool that reads network captures of the game (`.pcap` / `.pcapng`) and
decodes the SOE, gateway and zone layers, so packet layouts can be studied and reference records extracted. It is not
needed to run a server.

Back to [Code Documentation](Code-Documentation).

## Modes

```
dotnet run --project LandmarkEmulator.Parser -- triage <captures>
dotnet run -c Release --project LandmarkEmulator.Parser -- parse <capture or folder> [--opcodes 0xED00,0xF900] [--summary-only] [--with-timeline] [--skip-existing] [--skip-session-dupes] [--full-login-only] [--from-triage triage.json]
dotnet run --project LandmarkEmulator.Parser -- timeline <capture> --seconds 60 --markdown <out.md>
dotnet run --project LandmarkEmulator.Parser -- pack-terrain <parsed bins folder>
dotnet run --project LandmarkEmulator.Parser -- merge-cel2 <cell id list>
dotnet run --project LandmarkEmulator.Parser -- extract-creations <bins-dir> --zone <id> [--out <package-dir>] [--prop-defs <145-PropBase>] [--ready-only]
```

- **triage**: which captures hold a usable session (login and keys found).
- **parse**: decrypts and decodes a session; dumps chosen opcodes as binary files with a JSON description, a summary and
  an index (`summary.json`, `dumps-index.csv`), optionally a zone-entry timeline.
- **timeline**: the zone-entry sequence with timings, as CSV or markdown.
- **pack-terrain** / **merge-cel2**: build or extend a terrain cell store (CEL2) from captured CellsUpdate packets
  ([Format: CEL2](Format-CEL2)).
- **extract-creations**: from parsed bins, identify ClaimsBase AOI sites, check sufficiency (intersecting CellsUpdate
  cells and/or resolved props), and write an offline creation package (`manifest.json`, `report.json`, per-claim
  claims/props/cells). `--zone` is required (claim records do not carry zone). Apply later with WorldServer
  `importcreation` ([Commands: Building](Commands-Building)). Codecs live in `LandmarkEmulator.CaptureImport`.

## Main files

| Path | What |
|---|---|
| `Parser.cs` | Command line and modes |
| `PcapUdpReader.cs`, `IPHeader.cs`, `UdpHeader.cs` | Reading captures |
| `CaptureSessionParser.cs`, `DataStreamParsing.cs`, `CaptureReading.cs` | Rebuilding the SOE sessions and the game packet stream (using the server's own Shared and Gateway code) |
| `CaptureTriage.cs`, `CaptureEnrichment.cs`, `CaptureModels.cs` | Triage and decoding the login side |
| `ZoneTimelineMarkdown.cs` | Timelines |
| `TerrainCel2Packer.cs` | CEL2 terrain stores |
| `CreationExtractRunner.cs` | Creation package extract from bins |

Captures are not part of the repository.

## Batch pipeline (triage → parse → extract → import prep)

[`tools/creation-pipeline.ps1`](../tools/creation-pipeline.ps1) runs each step (or all of them). Default opcodes match creation extract (`0xF900`, `0x260062`, `0xEC000500`, `0xED00`). Parse is one Release folder pass (not one process per pcap). `all` reuses existing `triage.json` unless `-ForceTriage`. Timeline markdown is opt-in (`-WithTimeline`).

```
.\tools\creation-pipeline.ps1 all -Zone 406 -SkipSessionDupes -SkipExisting -ReadyOnly
.\tools\creation-pipeline.ps1 triage
.\tools\creation-pipeline.ps1 parse -SkipSessionDupes -SkipExisting
.\tools\creation-pipeline.ps1 extract -Zone 406 -ReadyOnly
.\tools\creation-pipeline.ps1 status
.\tools\creation-pipeline.ps1 import
```

| Step | Output |
| --- | --- |
| triage | `NetworkCaptures/_index/triage.json` |
| parse | `NetworkCaptures/_parsed/<stem>/bins/` |
| extract | `.../<stem>/creation-package/` (+ `report.json`) |
| import | `.../_parsed/_index/import-commands.txt` (paste into WorldServer `dev_commands.txt`) |

`tools/parse-captures.ps1` remains a thin wrapper around `parse`.
