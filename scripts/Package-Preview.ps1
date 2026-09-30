param([string]$Stamp = (Get-Date -Format 'yyyyMMdd-HHmm'))
$ErrorActionPreference = 'Stop'
if ($Stamp -notmatch '^\d{8}-\d{4}$') { throw 'Stamp must be yyyyMMdd-HHmm' }
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outputDir = Join-Path $projectRoot 'Builds/Handoff'
$playerRoot = Join-Path $projectRoot 'Builds/DungeonPreview'
foreach ($required in @('LunaEclipseLabyrinth.exe','UnityPlayer.dll','LunaEclipseLabyrinth_Data/globalgamemanagers')) {
    if (!(Test-Path -LiteralPath (Join-Path $playerRoot $required))) { throw "Missing build file: $required" }
}
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
function Write-Zip([string]$Destination, [string]$Base, [string[]]$RelativePaths, [string]$Prefix) {
    if (Test-Path -LiteralPath $Destination) { throw "Refusing to overwrite $Destination" }
    $stream = [IO.File]::Open($Destination, [IO.FileMode]::CreateNew)
    $zip = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($relative in $RelativePaths) {
            $path = Join-Path $Base $relative
            if (!(Test-Path -LiteralPath $path)) { throw "Missing package input: $relative" }
            $entry = Get-Item -LiteralPath $path
            $files = if ($entry.PSIsContainer) { Get-ChildItem -LiteralPath $path -File -Recurse } else { @($entry) }
            foreach ($file in $files) {
                if ($file.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Unexpected link: $($file.FullName)" }
                $name = [IO.Path]::GetRelativePath($Base, $file.FullName).Replace('\','/')
                [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, "$Prefix/$name", [IO.Compression.CompressionLevel]::Optimal) | Out-Null
            }
        }
    } finally { $zip.Dispose(); $stream.Dispose() }
}
$sourceZip = Join-Path $outputDir "luna-unity-source-$Stamp.zip"
$playerZip = Join-Path $outputDir "luna-windows-playable-$Stamp.zip"
$sourceInputs = @('Assets','Packages','ProjectSettings','docs','Screenshots','scripts','README.md','DUNGEON_REPORT.md','VISUAL_REVIEW.md','.gitignore')
Write-Zip $sourceZip $projectRoot $sourceInputs 'LunaEclipseLabyrinth'
# A fixed allowlist deliberately excludes QA, logs, symbols and DoNotShip folders.
$playerInputs = @('LunaEclipseLabyrinth.exe','UnityPlayer.dll','UnityCrashHandler64.exe','LunaEclipseLabyrinth_Data','MonoBleedingEdge','D3D12','dstorage.dll','dstoragecore.dll')
Write-Zip $playerZip $playerRoot $playerInputs 'LunaEclipseLabyrinth'
$archive = [IO.Compression.ZipFile]::Open($playerZip, [IO.Compression.ZipArchiveMode]::Update)
try {
    [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, (Join-Path $projectRoot 'docs/production/PLAYTEST_GUIDE.md'), 'LunaEclipseLabyrinth/PLAY_GUIDE.md', [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, (Join-Path $projectRoot 'docs/production/HANDOFF.md'), 'LunaEclipseLabyrinth/START_HERE.md', [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    foreach ($relative in @('README.md','VISUAL_REVIEW.md','DUNGEON_REPORT.md','Assets/Resources/Fonts/OFL.txt','docs/production/PLAN.md','docs/production/OPEN_ITEMS.md','docs/production/music.md','docs/production/art.md','docs/production/terrain-art.md','docs/production/stair-art.md','docs/production/asset-provenance.md')) {
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, (Join-Path $projectRoot $relative), "LunaEclipseLabyrinth/Documentation/$relative", [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
} finally { $archive.Dispose() }
$manifest = foreach ($path in @($sourceZip,$playerZip)) {
    $archive = [IO.Compression.ZipFile]::OpenRead($path)
    try {
        if ($archive.Entries.Count -eq 0) { throw 'Empty package' }
        if ($archive.Entries.FullName -match '^LunaEclipseLabyrinth/(Library|Logs|UserSettings|QA|SourceArchive)(/|$)' -or $archive.Entries.FullName -match 'DoNotShip') { throw 'Excluded directory leaked into archive' }
        [pscustomobject]@{ file = [IO.Path]::GetFileName($path); bytes = (Get-Item $path).Length; entries = $archive.Entries.Count; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }
    } finally { $archive.Dispose() }
}
$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $outputDir "manifest-$Stamp.json") -Encoding utf8
$manifest | Format-Table -AutoSize
# Local user handoff only. This script never uploads or publishes files.
