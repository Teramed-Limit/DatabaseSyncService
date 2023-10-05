@echo off

set ServiceName="Report Signoff Action Service"
set ServiceExePath="%~dp0ReportSignoffActionService.exe"

echo Creating Windows Service...
sc create %ServiceName% binPath= %ServiceExePath% start= auto
echo Windows Service created successfully.

echo Starting Windows Service...
sc start %ServiceName%
echo Windows Service started successfully.

pause