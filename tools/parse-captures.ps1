<#
.SYNOPSIS
  Decrypt/parse captures. Thin wrapper around creation-pipeline.ps1 parse.

.EXAMPLE
  .\tools\parse-captures.ps1 -Triage -SkipSessionDupes -SkipExisting
#>
[CmdletBinding()]
param(
    [string] $CapturesDir = "NetworkCaptures",
    [string] $Path,
    [string] $OutDir,
    [string] $Opcodes = "0xF900,0x260062,0xEC000500,0xED00",
    [switch] $Triage,
    [switch] $SkipSessionDupes,
    [switch] $SkipExisting,
    [switch] $Force,
    [switch] $WithTimeline,
    [switch] $WhatIf
)

$pipeline = Join-Path $PSScriptRoot "creation-pipeline.ps1"

function Invoke-Step([string] $step) {
    $cmd = @($pipeline, $step, "-CapturesDir", $CapturesDir, "-Opcodes", $Opcodes)
    if ($Path) { $cmd += @("-Path", $Path) }
    if ($OutDir) { $cmd += @("-OutDir", $OutDir) }
    if ($SkipSessionDupes) { $cmd += "-SkipSessionDupes" }
    if ($SkipExisting) { $cmd += "-SkipExisting" }
    if ($Force) { $cmd += "-Force" }
    if ($WithTimeline) { $cmd += "-WithTimeline" }
    if ($WhatIf) { $cmd += "-WhatIf" }
    & $cmd[0] @($cmd[1..($cmd.Length - 1)])
}

if ($Triage) { Invoke-Step "triage" }
Invoke-Step "parse"
