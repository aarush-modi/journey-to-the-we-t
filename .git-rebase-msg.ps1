param([string]$Path)
$raw = [System.IO.File]::ReadAllText($Path)
# Strip UTF-8 BOM if present
if ($raw.StartsWith([char]0xFEFF)) { $raw = $raw.Substring(1) }
$lines = $raw -split "`r?`n", -1
$filtered = foreach ($line in $lines) {
    if ($line -notmatch '^Made-with: Cursor\s*$') { $line }
}
# Trim trailing blank lines
while ($filtered.Count -gt 0 -and [string]::IsNullOrWhiteSpace($filtered[-1])) {
    $filtered = $filtered[0..($filtered.Count - 2)]
}
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllLines($Path, $filtered, $utf8NoBom)
