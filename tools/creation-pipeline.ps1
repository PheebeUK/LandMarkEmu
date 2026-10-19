<#
.SYNOPSIS
  Capture → creation inject pipeline (triage, parse/decrypt, extract, import prep).

.DESCRIPTION
  Steps (run from repo root):

    triage   — classify pcaps (Auth/Gateway), write NetworkCaptures/_index/triage.*
    parse    — decrypt full-login captures → _parsed/<stem>/bins (creation opcodes)
    extract  — bins → creation-package (+ report.json); needs -Zone
    status   — summarise parsed bins / packages / ready counts
    import   — write importcreation lines for WorldServer (does not talk to the server)
    all      — triage (if missing) → parse → extract → import (prep)

  Parse is a single Release folder pass (not one process per pcap).

.EXAMPLE
  .\tools\creation-pipeline.ps1 all -Zone 406 -SkipSessionDupes -SkipExisting -ReadyOnly

.EXAMPLE
  .\tools\creation-pipeline.ps1 parse -SkipSessionDupes -SkipExisting
  .\tools\creation-pipeline.ps1 extract -Zone 406
  .\tools\creation-pipeline.ps1 status
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet("triage", "parse", "extract", "status", "import", "all", "help")]
    [string] $Step = "help",

    [string] $CapturesDir = "NetworkCaptures",
    [string] $Path,
    [string] $OutDir,
    [string] $Opcodes = "0xF900,0x260062,0xEC000500,0xED00",
    [uint32] $Zone = 0,
    [string] $PropDefs,
    [switch] $SkipSessionDupes,
    [switch] $SkipExisting,
    [switch] $ReadyOnly,
    [switch] $Force,
    [switch] $ForceTriage,
    [switch] $WithTimeline,
    [switch] $WhatIf
)

$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    $root = Split-Path -Parent $PSScriptRoot
    if (-not (Test-Path (Join-Path $root "LandmarkEmulator.Parser"))) {
        $root = (Get-Location).Path
    }
    return $root
}

function Resolve-UnderRoot([string] $root, [string] $p) {
    if ([string]::IsNullOrWhiteSpace($p)) { return $null }
    if ([System.IO.Path]::IsPathRooted($p)) { return $p }
    return Join-Path $root $p
}

function Invoke-Parser([string[]] $ParserArgs) {
    Write-Host ("  > dotnet run -c Release --project LandmarkEmulator.Parser -- " + ($ParserArgs -join " ")) -ForegroundColor DarkGray
    if ($WhatIf) { return 0 }
    # Pipe to Out-Host so Parser stdout is not captured as the function's return value
    # (otherwise $code = Invoke-Parser ... becomes the whole log + exit code).
    & dotnet run -c Release --project LandmarkEmulator.Parser -- @ParserArgs 2>&1 | Out-Host
    $exit = $LASTEXITCODE
    if ($null -eq $exit) { return 0 }
    return [int]$exit
}

function Get-Stem([string] $fileName) {
    $stem = [IO.Path]::GetFileNameWithoutExtension($fileName)
    if ($stem.EndsWith(".pcap", [StringComparison]::OrdinalIgnoreCase)) {
        $stem = [IO.Path]::GetFileNameWithoutExtension($stem)
    }
    foreach ($c in [IO.Path]::GetInvalidFileNameChars()) {
        $stem = $stem.Replace($c, "_")
    }
    return $stem.Trim()
}

function Show-Help {
    Write-Host @'


creation-pipeline - Landmark capture -> creation inject

  .\tools\creation-pipeline.ps1 <step> [options]

Steps:
  triage    Classify captures (Auth / Gateway / full login)
  parse     Decrypt + dump bins (claims / props / cells opcodes)
  extract   Build creation packages from each .../bins (--Zone required)
  status    List bins / packages / ready claim counts
  import    Write importcreation command list for WorldServer
  all       triage (if missing) -> parse -> extract -> import

Common options:
  -CapturesDir NetworkCaptures
  -Zone 406                 (required for extract / all)
  -SkipSessionDupes         Prefer foo.pcapng over session-foo.pcapng
  -SkipExisting             Skip stems that already have bins / packages
  -ReadyOnly                extract: only write ready claim payloads
  -Force                    parse gateway-only / no-Auth anyway
  -ForceTriage              all: re-run triage even if triage.json exists
  -WithTimeline             parse: also write zone-entry timeline markdown
  -Path <file>              Single capture (parse)
  -WhatIf                   Print commands only

Examples:
  .\tools\creation-pipeline.ps1 all -Zone 406 -SkipSessionDupes -SkipExisting -ReadyOnly
  .\tools\creation-pipeline.ps1 parse -SkipSessionDupes -SkipExisting
  .\tools\creation-pipeline.ps1 extract -Zone 406 -ReadyOnly
  .\tools\creation-pipeline.ps1 status

Apply packages in-game (WorldServer running):
  importcreation NetworkCaptures/_parsed/<stem>/creation-package --dry-run
  importcreation NetworkCaptures/_parsed/<stem>/creation-package --owner me

'@
}

# --- steps ---

function Step-Triage($ctx) {
    Write-Host ('[1/triage] {0}' -f $ctx.Captures) -ForegroundColor Yellow
    $code = Invoke-Parser @("triage", $ctx.Captures)
    if ($code -ne 0) { throw "triage failed (exit $code)" }
    Write-Host ('[triage] ok -> {0}' -f $ctx.TriageJson)
}

function Ensure-Triage($ctx) {
    if ((Test-Path $ctx.TriageJson) -and -not $ForceTriage) {
        Write-Host ('[triage] reuse {0} (use -ForceTriage to refresh)' -f $ctx.TriageJson)
        return
    }
    Step-Triage $ctx
}

function Step-Parse($ctx) {
    Write-Host ('[2/parse] decrypt -> {0}' -f $ctx.ParsedRoot) -ForegroundColor Yellow
    if (-not (Test-Path $ctx.TriageJson) -and -not $Force) {
        Write-Host '[parse] no triage.json - running triage first'
        Step-Triage $ctx
    }

    $target = if ($Path) { Resolve-UnderRoot $ctx.Root $Path } else { $ctx.Captures }
    if (-not (Test-Path $target)) { throw "Capture path not found: $target" }

    $args = @("parse", $target, "--opcodes", $Opcodes, "--summary-only", "--out", $ctx.ParsedRoot)
    if ($WithTimeline) { $args += "--with-timeline" }
    if ($Force) { $args += "--force" }
    if (Test-Path $ctx.TriageJson) {
        $args += @("--from-triage", $ctx.TriageJson)
        if (-not $Force) { $args += "--full-login-only" }
    }
    if ($SkipSessionDupes) { $args += "--skip-session-dupes" }
    if ($SkipExisting) { $args += "--skip-existing" }

    Write-Host '[parse] one Release folder pass (Parser applies skip filters)'
    $code = Invoke-Parser $args
    if ($code -ne 0) { throw "parse failed (exit $code)" }
    Write-Host '[parse] done'
}

function Get-BinsDirs($ctx) {
    if (-not (Test-Path $ctx.ParsedRoot)) { return @() }
    return @(Get-ChildItem -Path $ctx.ParsedRoot -Directory | Where-Object {
        $_.Name -ne "_index" -and (Test-Path (Join-Path $_.FullName "bins"))
    } | Sort-Object Name)
}

function Step-Extract($ctx) {
    if ($Zone -eq 0) { throw 'extract requires -Zone <id> (e.g. -Zone 406)' }
    Write-Host ('[3/extract] zone={0} -> creation-package under each session' -f $Zone) -ForegroundColor Yellow

    $sessions = Get-BinsDirs $ctx
    if ($sessions.Count -eq 0) {
        Write-Host ('[extract] no .../bins folders under {0}' -f $ctx.ParsedRoot)
        return
    }

    $ok = 0; $fail = 0; $skip = 0; $i = 0
    foreach ($session in $sessions) {
        $i++
        $bins = Join-Path $session.FullName "bins"
        $pkg = Join-Path $session.FullName "creation-package"
        if ($SkipExisting -and (Test-Path (Join-Path $pkg "report.json"))) {
            Write-Host ('[extract] skip existing {0}' -f $session.Name)
            $skip++
            continue
        }
        Write-Host ('[extract] ({0}/{1}) {2}' -f $i, $sessions.Count, $session.Name) -ForegroundColor Green
        $args = @("extract-creations", $bins, "--zone", "$Zone", "--out", $pkg)
        if ($ReadyOnly) { $args += "--ready-only" }
        if ($PropDefs) { $args += @("--prop-defs", (Resolve-UnderRoot $ctx.Root $PropDefs)) }
        $code = Invoke-Parser $args
        # exit 1 when ready=0 is expected for empty sessions - count as soft fail
        if ($code -eq 0) { $ok++ }
        elseif (Test-Path (Join-Path $pkg "report.json")) { $ok++; Write-Host '[extract] package written (no ready claims)' -ForegroundColor DarkYellow }
        else { $fail++; Write-Host ('[extract] FAILED exit={0}' -f $code) -ForegroundColor Red }
    }
    Write-Host ('[extract] done ok={0} skip={1} fail={2}' -f $ok, $skip, $fail)
    if ($fail -gt 0) { throw "extract had $fail failure(s)" }
}

function Step-Status($ctx) {
    Write-Host ('[status] {0}' -f $ctx.ParsedRoot) -ForegroundColor Yellow
    $rows = @()
    foreach ($session in (Get-BinsDirs $ctx)) {
        $bins = Join-Path $session.FullName "bins"
        $pkg = Join-Path $session.FullName "creation-package"
        $binCount = @(Get-ChildItem $bins -Filter "*.bin" -ErrorAction SilentlyContinue).Count
        $ready = "-"; $claims = "-"; $hasPkg = Test-Path (Join-Path $pkg "report.json")
        if ($hasPkg) {
            $report = Get-Content -Raw (Join-Path $pkg "report.json") | ConvertFrom-Json
            $claims = @($report.claims).Count
            $ready = @($report.claims | Where-Object { $_.ready }).Count
        }
        $rows += [pscustomobject]@{
            Session = $session.Name
            Bins    = $binCount
            Package = $(if ($hasPkg) { "yes" } else { "no" })
            Claims  = $claims
            Ready   = $ready
        }
    }
    if ($rows.Count -eq 0) {
        Write-Host '  (no parsed sessions yet - run parse)'
        return
    }
    $rows | Format-Table -AutoSize | Out-String | Write-Host
    $readyTotal = ($rows | Where-Object { $_.Ready -match '^\d+$' } | ForEach-Object { [int]$_.Ready } | Measure-Object -Sum).Sum
    Write-Host ('Sessions={0} ReadyClaims={1}' -f $rows.Count, $readyTotal)
}

function Step-Import($ctx) {
    Write-Host '[4/import-prep] write WorldServer command list' -ForegroundColor Yellow
    $indexDir = Join-Path $ctx.ParsedRoot "_index"
    if (-not $WhatIf) { New-Item -ItemType Directory -Force -Path $indexDir | Out-Null }
    $outFile = Join-Path $indexDir "import-commands.txt"
    $lines = @()
    $lines += "# Paste lines into WorldServer dev_commands.txt (or admin Any command)."
    $lines += "# Prefer --dry-run first. Owner defaults to targeted player with --owner me."
    $lines += ""

    $n = 0
    foreach ($session in (Get-BinsDirs $ctx)) {
        $pkg = Join-Path $session.FullName "creation-package"
        $reportPath = Join-Path $pkg "report.json"
        if (-not (Test-Path $reportPath)) { continue }
        $report = Get-Content -Raw $reportPath | ConvertFrom-Json
        $ready = @($report.claims | Where-Object { $_.ready }).Count
        if ($ready -eq 0) { continue }
        $rel = $pkg
        if ($rel.StartsWith($ctx.Root, [StringComparison]::OrdinalIgnoreCase)) {
            $rel = $pkg.Substring($ctx.Root.Length).TrimStart("\", "/")
        }
        $lines += ("# {0} - {1} ready claim(s)" -f $session.Name, $ready)
        $lines += "importcreation $rel --dry-run"
        $lines += "importcreation $rel --owner me"
        $lines += ""
        $n++
    }

    if ($WhatIf) {
        $lines | ForEach-Object { Write-Host $_ }
        return
    }
    Set-Content -Path $outFile -Value $lines -Encoding utf8
    Write-Host ('[import] {0} package(s) with ready claims -> {1}' -f $n, $outFile)
    if ($n -eq 0) {
        Write-Host '[import] nothing ready - check status / extract -Zone'
    }
}

# --- main ---

$root = Get-RepoRoot
Set-Location $root
$captures = Resolve-UnderRoot $root $CapturesDir
if (-not (Test-Path $captures) -and $Step -ne "help") {
    throw "Captures folder not found: $captures"
}
$parsedRoot = if ($OutDir) { Resolve-UnderRoot $root $OutDir } else { Join-Path $captures "_parsed" }
$ctx = @{
    Root       = $root
    Captures   = $captures
    ParsedRoot = $parsedRoot
    TriageJson = Join-Path $captures "_index\triage.json"
}

switch ($Step) {
    "help"    { Show-Help }
    "triage"  { Step-Triage $ctx }
    "parse"   { Step-Parse $ctx }
    "extract" { Step-Extract $ctx; Step-Status $ctx }
    "status"  { Step-Status $ctx }
    "import"  { Step-Import $ctx }
    "all"     {
        Ensure-Triage $ctx
        Step-Parse $ctx
        Step-Extract $ctx
        Step-Import $ctx
        Step-Status $ctx
    }
}
