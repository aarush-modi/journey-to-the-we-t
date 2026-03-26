param([string]$Path)
$lines = Get-Content -LiteralPath $Path
$lines | ForEach-Object {
    if ($_ -match '^pick ') { $_ -replace '^pick', 'reword' } else { $_ }
} | Set-Content -LiteralPath $Path
