# Bannerlord Commander UI File Watcher
# This script monitors GUI XML files and automatically copies them to the game's mod directory
# when changes are detected, making development faster.

param(
    [string]$GamePath = "",
    [string]$ModuleName = "Bannerlord.Commander"
)

# Auto-detect game path if not specified
if ([string]::IsNullOrEmpty($GamePath)) {
    $possiblePaths = @(
        "${env:ProgramFiles(x86)}\Steam\steamapps\common\Mount & Blade II Bannerlord",
        "$env:ProgramFiles\Steam\steamapps\common\Mount & Blade II Bannerlord"
    )
    
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $GamePath = $path
            break
        }
    }
    
    # If still not found, default to Program Files (x86)
    if ([string]::IsNullOrEmpty($GamePath)) {
        $GamePath = "${env:ProgramFiles(x86)}\Steam\steamapps\common\Mount & Blade II Bannerlord"
    }
}

# Colors for console output
$SuccessColor = "Green"
$ErrorColor = "Red"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "Bannerlord Commander UI File Watcher" -ForegroundColor $InfoColor
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host ""

# Get the script directory and project root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$SourceGUIPath = Join-Path $ProjectRoot "Bannerlord.Commander\_Module\GUI"
$GameModPath = Join-Path $GamePath "Modules\$ModuleName\GUI"

# Validate paths
if (-not (Test-Path $SourceGUIPath)) {
    Write-Host "[ERROR] Source GUI path not found: $SourceGUIPath" -ForegroundColor $ErrorColor
    Write-Host "Please ensure you're running this script from the project root or dev-tools directory." -ForegroundColor $WarningColor
    exit 1
}

if (-not (Test-Path $GameModPath)) {
    Write-Host "[WARNING] Game mod path not found: $GameModPath" -ForegroundColor $WarningColor
    Write-Host "The script will continue, but files won't be copied until the path exists." -ForegroundColor $WarningColor
    Write-Host "Please ensure:" -ForegroundColor $WarningColor
    Write-Host "  1. Bannerlord is installed at: $GamePath" -ForegroundColor $WarningColor
    Write-Host "  2. The mod is installed in the Modules directory" -ForegroundColor $WarningColor
    Write-Host ""
    
    # Create the directory if parent exists
    $ModulesPath = Join-Path $GamePath "Modules"
    if (Test-Path $ModulesPath) {
        Write-Host "[INFO] Creating mod GUI directory..." -ForegroundColor $InfoColor
        New-Item -Path $GameModPath -ItemType Directory -Force | Out-Null
        Write-Host "[SUCCESS] Created: $GameModPath" -ForegroundColor $SuccessColor
    }
}

Write-Host "[INFO] Watching: $SourceGUIPath" -ForegroundColor $InfoColor
Write-Host "[INFO] Target: $GameModPath" -ForegroundColor $InfoColor
Write-Host ""
Write-Host "Press Ctrl+C to stop watching..." -ForegroundColor $WarningColor
Write-Host ""

# Initial sync - copy all existing files
Write-Host "[INFO] Performing initial sync..." -ForegroundColor $InfoColor
if (Test-Path $GameModPath) {
    $files = Get-ChildItem -Path $SourceGUIPath -Recurse -File
    foreach ($file in $files) {
        $relativePath = $file.FullName.Substring($SourceGUIPath.Length + 1)
        $destPath = Join-Path $GameModPath $relativePath
        
        try {
            # Ensure destination directory exists
            $destDir = Split-Path -Parent $destPath
            if (-not (Test-Path $destDir)) {
                New-Item -Path $destDir -ItemType Directory -Force | Out-Null
            }
            
            Copy-Item -Path $file.FullName -Destination $destPath -Force
            Write-Host "  [SYNCED] $relativePath" -ForegroundColor $SuccessColor
        }
        catch {
            Write-Host "  [ERROR] Failed to sync: $relativePath" -ForegroundColor $ErrorColor
        }
    }
    Write-Host "[SUCCESS] Initial sync complete!" -ForegroundColor $SuccessColor
}
else {
    Write-Host "[SKIPPED] Initial sync (game path not accessible)" -ForegroundColor $WarningColor
}
Write-Host ""

# Create file watcher
$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = $SourceGUIPath
$watcher.Filter = "*.xml"
$watcher.IncludeSubdirectories = $true
$watcher.EnableRaisingEvents = $true

# Define event handlers with inline logic (to work properly in background jobs)
$onChange = {
    param($sender, $e)
    
    $sourcePath = $e.FullPath
    $changeType = $e.ChangeType
    $sourceGUIPath = $Event.MessageData.SourceGUIPath
    $gameModPath = $Event.MessageData.GameModPath
    
    $relativePath = $sourcePath.Substring($sourceGUIPath.Length + 1)
    $destPath = Join-Path $gameModPath $relativePath
    $timestamp = Get-Date -Format "HH:mm:ss"
    
    if (Test-Path $gameModPath) {
        try {
            # Ensure destination directory exists
            $destDir = Split-Path -Parent $destPath
            if (-not (Test-Path $destDir)) {
                New-Item -Path $destDir -ItemType Directory -Force | Out-Null
            }
            
            # Small delay to ensure file is not locked
            Start-Sleep -Milliseconds 100
            Copy-Item -Path $sourcePath -Destination $destPath -Force -ErrorAction Stop
            
            Write-Host "[$timestamp] [COPIED] $relativePath" -ForegroundColor Green
            Write-Host "           Use 'ui.reload' in game console to see changes" -ForegroundColor Cyan
        }
        catch {
            Write-Host "[$timestamp] [ERROR] Failed to copy: $relativePath - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "[$timestamp] [SKIPPED] $relativePath (game path not accessible)" -ForegroundColor Yellow
    }
}

$onDelete = {
    param($sender, $e)
    
    $sourcePath = $e.FullPath
    $sourceGUIPath = $Event.MessageData.SourceGUIPath
    $gameModPath = $Event.MessageData.GameModPath
    
    $relativePath = $sourcePath.Substring($sourceGUIPath.Length + 1)
    $destPath = Join-Path $gameModPath $relativePath
    $timestamp = Get-Date -Format "HH:mm:ss"
    
    if (Test-Path $destPath) {
        try {
            Remove-Item $destPath -Force
            Write-Host "[$timestamp] [DELETED] $relativePath" -ForegroundColor Yellow
        }
        catch {
            Write-Host "[$timestamp] [ERROR] Failed to delete: $relativePath" -ForegroundColor Red
        }
    }
}

$onRename = {
    param($sender, $e)
    
    $sourceGUIPath = $Event.MessageData.SourceGUIPath
    $gameModPath = $Event.MessageData.GameModPath
    $timestamp = Get-Date -Format "HH:mm:ss"
    
    $oldRelativePath = $e.OldFullPath.Substring($sourceGUIPath.Length + 1)
    $newRelativePath = $e.FullPath.Substring($sourceGUIPath.Length + 1)
    
    Write-Host "[$timestamp] [RENAMED] $oldRelativePath -> $newRelativePath" -ForegroundColor Cyan
    
    # Delete old file
    $oldDestPath = Join-Path $gameModPath $oldRelativePath
    if (Test-Path $oldDestPath) {
        Remove-Item $oldDestPath -Force -ErrorAction SilentlyContinue
    }
    
    # Copy new file
    $newDestPath = Join-Path $gameModPath $newRelativePath
    if (Test-Path $gameModPath) {
        try {
            $destDir = Split-Path -Parent $newDestPath
            if (-not (Test-Path $destDir)) {
                New-Item -Path $destDir -ItemType Directory -Force | Out-Null
            }
            
            Start-Sleep -Milliseconds 100
            Copy-Item -Path $e.FullPath -Destination $newDestPath -Force
            Write-Host "[$timestamp] [COPIED] $newRelativePath" -ForegroundColor Green
        }
        catch {
            Write-Host "[$timestamp] [ERROR] Failed to copy renamed file: $newRelativePath" -ForegroundColor Red
        }
    }
}

# Prepare message data to pass to event handlers
$messageData = @{
    SourceGUIPath = $SourceGUIPath
    GameModPath = $GameModPath
}

# Register event handlers with MessageData
Register-ObjectEvent $watcher "Changed" -Action $onChange -MessageData $messageData | Out-Null
Register-ObjectEvent $watcher "Created" -Action $onChange -MessageData $messageData | Out-Null
Register-ObjectEvent $watcher "Deleted" -Action $onDelete -MessageData $messageData | Out-Null
Register-ObjectEvent $watcher "Renamed" -Action $onRename -MessageData $messageData | Out-Null

# Keep script running
try {
    while ($true) {
        Start-Sleep -Seconds 1
    }
}
finally {
    # Cleanup
    $watcher.EnableRaisingEvents = $false
    $watcher.Dispose()
    Get-EventSubscriber | Unregister-Event
    Write-Host ""
    Write-Host "[INFO] File watcher stopped." -ForegroundColor $InfoColor
}
