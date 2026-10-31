param(
    [Parameter(Mandatory = $true)]
    [string] $UnityEditorPath
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $UnityEditorPath -PathType Leaf)) {
    throw "Unity editor does not exist: $UnityEditorPath"
}
$systemRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path ([System.IO.Path]::GetTempPath()) ("WaveSystemValidation-" + [Guid]::NewGuid().ToString('N'))
try {
    $assets = New-Item -ItemType Directory -Path (Join-Path $project 'Assets')
    $editor = New-Item -ItemType Directory -Path (Join-Path $assets.FullName 'Editor')
    foreach ($folder in @('Definition', 'Generation', 'Execution')) {
        Copy-Item -LiteralPath (Join-Path $systemRoot $folder) -Destination $assets.FullName -Recurse
    }
    $poolScripts = Join-Path (Split-Path -Parent (Split-Path -Parent $systemRoot)) 'CCLBStudio\ScriptablePooling\Scripts'
    if (-not (Test-Path -LiteralPath (Join-Path $poolScripts 'ScriptablePool.cs'))) {
        throw "ScriptablePooling submodule is missing. Run: git submodule update --init"
    }
    Copy-Item -LiteralPath $poolScripts -Destination (Join-Path $assets.FullName 'ScriptablePooling') -Recurse
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'WaveSystemValidation.cs') -Destination $editor.FullName
    $packages = New-Item -ItemType Directory -Path (Join-Path $project 'Packages')
    Set-Content -LiteralPath (Join-Path $packages.FullName 'manifest.json') -Value '{"dependencies":{}}'
    $log = Join-Path $project 'validation.log'
    $process = Start-Process -FilePath $UnityEditorPath -ArgumentList @(
        '-batchmode', '-nographics', '-projectPath', "`"$project`"",
        '-executeMethod', 'WaveSystemValidation.Run', '-logFile', "`"$log`""
    ) -PassThru
    if (-not $process.WaitForExit(240000)) {
        Stop-Process -Id $process.Id -Force
        $process.WaitForExit()
        throw "Unity validation timed out after 240 seconds."
    }
    $output = Get-Content -LiteralPath $log -Raw
    if ($process.ExitCode -ne 0 -or $output -notmatch 'WAVE_SYSTEM_VALIDATION_PASSED') {
        Write-Output $output
        throw "Wave System validation failed (Unity exit code $($process.ExitCode))."
    }
    $output -split "`n" | Where-Object { $_ -match 'PASS:|WAVE_SYSTEM_VALIDATION_PASSED' }
}
finally {
    if (Test-Path -LiteralPath $project) {
        Remove-Item -LiteralPath $project -Recurse -Force
    }
}
