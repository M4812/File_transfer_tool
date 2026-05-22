param(
    [Parameter(Mandatory = $true)]
    [string]$PackageDir
)

$ErrorActionPreference = 'Stop'

$resolved = Resolve-Path -LiteralPath $PackageDir
$packagePath = $resolved.Path

$requiredFiles = @(
    'FileTransferTool.App.exe',
    'hostfxr.dll',
    'hostpolicy.dll',
    'coreclr.dll',
    'System.Windows.Forms.dll',
    'Config\linkdata.xml'
)

$missing = @()
foreach ($file in $requiredFiles) {
    $path = Join-Path $packagePath $file
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $missing += $file
    }
}

if ($missing.Count -gt 0) {
    throw "Package is not Windows Server portable. Missing: $($missing -join ', ')"
}

$exe = Join-Path $packagePath 'FileTransferTool.App.exe'
$process = Start-Process -FilePath $exe -WorkingDirectory $packagePath -PassThru -WindowStyle Hidden
try {
    Start-Sleep -Seconds 3
    if ($process.HasExited) {
        throw "Application exited during startup with code $($process.ExitCode)."
    }
}
finally {
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
        $process.WaitForExit()
    }
}

Write-Host "Package verification passed: $packagePath"
