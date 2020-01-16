echo off

if not errorlevel 0 goto p
pylib\python\python.exe pylib\export.py datasrc ..\..\CrocodileGamePj\Assets\Resources\Jsons\

:p
goto:eof

