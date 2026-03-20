param(
    [Parameter(Mandatory = $true)]
    [string]$ExecutablePath,

    [Parameter(Mandatory = $true)]
    [string]$SharedLabUser
)

if (-not (Test-Path $ExecutablePath)) {
    throw "Executable not found: $ExecutablePath"
}

$action = New-ScheduledTaskAction -Execute $ExecutablePath
$trigger = New-ScheduledTaskTrigger -AtLogOn -User $SharedLabUser
$principal = New-ScheduledTaskPrincipal -UserId $SharedLabUser -LogonType Interactive -RunLevel Limited
$settings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -MultipleInstances IgnoreNew `
    -RestartCount 3 `
    -RestartInterval (New-TimeSpan -Minutes 1)

Register-ScheduledTask `
    -TaskName "PCLab Client Startup" `
    -Description "Starts the PC Lab lock/check-in overlay when the shared lab account signs in." `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Settings $settings `
    -Force
