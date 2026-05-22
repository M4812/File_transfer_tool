param(
    [string]$PackageDir = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'

$resolvedPackage = Resolve-Path -LiteralPath $PackageDir
$packagePath = $resolvedPackage.Path
$exePath = Join-Path $packagePath 'FileTransferTool.App.exe'

Write-Host "Package: $packagePath"
Write-Host "OS: $([Environment]::OSVersion.VersionString)"
Write-Host "64-bit OS: $([Environment]::Is64BitOperatingSystem)"
Write-Host "64-bit process: $([Environment]::Is64BitProcess)"

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
    if (-not (Test-Path -LiteralPath (Join-Path $packagePath $file) -PathType Leaf)) {
        $missing += $file
    }
}

if ($missing.Count -gt 0) {
    throw "Missing package files: $($missing -join ', '). Use artifacts\publish\FileTransferTool.App\win-x64 or the self-contained zip."
}

if (-not [Environment]::UserInteractive) {
    Write-Warning "Current session is not interactive. This WinForms app needs Windows Server Desktop Experience or an interactive logged-in desktop session."
}

$major = [Environment]::OSVersion.Version.Major
$minor = [Environment]::OSVersion.Version.Minor
if ($major -eq 6 -and ($minor -eq 2 -or $minor -eq 3)) {
    Write-Warning "Windows Server 2012/2012 R2 may require Microsoft Visual C++ 2015-2019 Redistributable."
}

$process = Start-Process -FilePath $exePath -WorkingDirectory $packagePath -PassThru
try {
    Start-Sleep -Seconds 3
    if ($process.HasExited) {
        throw "Application exited during startup with code $($process.ExitCode)."
    }

    Write-Host "Startup check passed: application stayed open for 3 seconds."
}
finally {
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
        $process.WaitForExit()
    }
}
