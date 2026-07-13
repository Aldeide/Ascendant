$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Unity.exe"
$projectPath = "C:\Users\Anthony\Desktop\Ascendant"
$resultsPath = "$projectPath\Library\test-results.xml"

# Remove old results
if (Test-Path $resultsPath) { Remove-Item $resultsPath -Force }

Write-Host "Running EditMode tests..."
$process = Start-Process -FilePath $unityPath -ArgumentList "-runTests", "-batchmode", "-projectPath `"$projectPath`"", "-testResults `"$resultsPath`"", "-testPlatform", "editmode", "-quit" -NoNewWindow -PassThru
$process.WaitForExit()

if (Test-Path $resultsPath) {
    [xml]$xml = Get-Content $resultsPath
    $failed = $xml.SelectNodes("//test-case[@result='Failed']")
    if ($failed.Count -gt 0) {
        Write-Host ""
        Write-Host "------------------------------------" -ForegroundColor Red
        Write-Host "TEST RUN FAILED!" -ForegroundColor Red
        Write-Host "------------------------------------" -ForegroundColor Red
        foreach ($f in $failed) {
            Write-Host "Failed: $($f.name)" -ForegroundColor Red
            Write-Host "  Reason: $($f.failure.message)"
        }
        exit 1
    } else {
        $passed = $xml.SelectNodes("//test-case[@result='Passed'] ")
        $passedCount = if ($passed -eq $null) { 0 } else { $passed.Count }
        Write-Host ""
        Write-Host "------------------------------------" -ForegroundColor Green
        Write-Host "ALL TESTS PASSED SUCCESSFULLY! ($passedCount passed)" -ForegroundColor Green
        Write-Host "------------------------------------" -ForegroundColor Green
        exit 0
    }
} else {
    Write-Host "Error: Test results file not found! Unity might have failed to run tests." -ForegroundColor Red
    exit 1
}
