@echo off

set ServiceName="Report Signoff Action Service"

echo Stopping Windows Service...
net stop %ServiceName%
echo Windows Service stopped successfully.

echo Deleting Windows Service...
sc delete %ServiceName%
echo Windows Service deleted successfully.

pause
