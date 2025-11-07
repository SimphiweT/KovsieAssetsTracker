# ============================================
# Pull_AllBranches.ps1
# Description: Pulls latest changes from all
# KovsieAssetsTracker branches into local folders
# ============================================

# --- CONFIGURATION ---
$repoUrl = "https://github.com/SimphiweT/KovsieAssetsTracker.git"
$basePath = "C:\KovsieAssetTrackerUFS"

# --- BRANCHES AND FOLDER MAPPING ---
$branches = @{
    "KovsieAssetsTracker_Web"     = "$basePath\KovsieAssetsTracker"
    "KovsieAssetsTracker_Android" = "$basePath\KovsieAssetsTrackerAndroid"
    "KovsieAssetsTracker_Extras"  = "$basePath\Extras"
}

# --- FUNCTION TO PULL BRANCH ---
function Pull-Branch {
    param(
        [string]$branchName,
        [string]$folderPath
    )

    Write-Host "`n=== Processing branch: $branchName ===" -ForegroundColor Cyan
    Write-Host "Folder: $folderPath"

    # Ensure folder exists
    if (!(Test-Path $folderPath)) {
        Write-Host "❌ Folder not found: $folderPath" -ForegroundColor Red
        return
    }

    Set-Location $folderPath

    # Initialize repo if not already
    if (!(Test-Path "$folderPath\.git")) {
        Write-Host "Initializing new Git repo..." -ForegroundColor Yellow
        git init
        git remote add origin $repoUrl
        git fetch origin
        git checkout -b $branchName origin/$branchName
    } else {
        Write-Host "Pulling latest changes..." -ForegroundColor Green
        git fetch origin
        git checkout $branchName
        git pull origin $branchName
    }

    Write-Host "✅ Done updating $branchName" -ForegroundColor Green
}

# --- MAIN LOOP ---
foreach ($branch in $branches.Keys) {
    Pull-Branch -branchName $branch -folderPath $branches[$branch]
}

Write-Host "`nAll branches updated successfully ✅" -ForegroundColor Cyan
