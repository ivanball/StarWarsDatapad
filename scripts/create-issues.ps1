<#
.SYNOPSIS
    Creates the GitHub issues in docs/issues, in file order, on the current repo.

.DESCRIPTION
    Commits reference issues as #1..#N, matching the file prefixes. Run this once on a FRESH repository
    (no issues or PRs yet) so GitHub assigns the same numbers. Each file is:
        # <title>
        Labels: <comma-separated labels>
        <body...>
    Requires the GitHub CLI (gh) authenticated against the target repo.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$IssuesPath = (Join-Path $PSScriptRoot '..\docs\issues')
)

$ErrorActionPreference = 'Stop'

$files = Get-ChildItem -Path $IssuesPath -Filter '*.md' | Sort-Object Name
foreach ($file in $files) {
    $lines = Get-Content -Path $file.FullName
    $title = $lines[0] -replace '^#\s*', ''
    $labels = ($lines[1] -replace '^Labels:\s*', '').Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ }
    $body = ($lines | Select-Object -Skip 2) -join "`n"

    foreach ($label in $labels) {
        if ($PSCmdlet.ShouldProcess($label, 'gh label create')) {
            gh label create $label --force | Out-Null
        }
    }

    if ($PSCmdlet.ShouldProcess($title, 'gh issue create')) {
        $labelArgs = $labels | ForEach-Object { '--label', $_ }
        $body | gh issue create --title $title --body-file - @labelArgs
    }
}
