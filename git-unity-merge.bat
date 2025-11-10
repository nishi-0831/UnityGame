@echo off
rem git-unity-merge.bat
rem Usage: git-unity-merge.bat [path_to_UnityYAMLMerge.exe]
rem If no argument is given, edit the DEFAULT_UNITYYAML path below.

setlocal enabledelayedexpansion

rem ====== CONFIG: change DEFAULT_UNITYYAML to your Unity path if needed ======
set "DEFAULT_UNITYYAML=C:\Program Files\Unity\Hub\Editor\6000.0.46f1\Editor\Data\Tools\UnityYAMLMerge.exe"
rem ========================================================================

if "%~1"=="" (
  set "UNITYYAML=%DEFAULT_UNITYYAML%"
) else (
  set "UNITYYAML=%~1"
)

rem Extensions to auto-merge (case-insensitive)
rem Note: extension includes leading dot (e.g. .unity)
set "EXT1=.unity"
set "EXT2=.prefab"
set "EXT3=.mat"
set "EXT4=.anim"
set "EXT5=.asset"

set /a COUNTER=0

rem Get list of conflicted files
for /f "usebackq delims=" %%F in (`git diff --name-only --diff-filter=U`) do (
  set "file=%%F"
  set "ext=%%~xF"
  rem Compare case-insensitive using delayed expansion
  if /I "!ext!"=="!EXT1!" (
    call :processFile "%%F"
  ) else if /I "!ext!"=="!EXT2!" (
    call :processFile "%%F"
  ) else if /I "!ext!"=="!EXT3!" (
    call :processFile "%%F"
  ) else if /I "!ext!"=="!EXT4!" (
    call :processFile "%%F"
  ) else if /I "!ext!"=="!EXT5!" (
    call :processFile "%%F"
  ) else (
    echo Skipping non-Unity asset: %%F
  )
)

echo Done.
endlocal
exit /b 0

:processFile
set "file=%~1"
set /a COUNTER+=1
set "rnd=%COUNTER%_%RANDOM%"

rem make a safe base name for temp files
set "safe=%file:/=_%"
set "safe=%safe:\=_%"
set "BASE_TMP=%TEMP%\uym_%safe%_%rnd%_base"
set "OURS_TMP=%TEMP%\uym_%safe%_%rnd%_ours"
set "THEIRS_TMP=%TEMP%\uym_%safe%_%rnd%_theirs"
set "MERGED_TMP=%TEMP%\uym_%safe%_%rnd%_merged"

rem initialize
set "BASE_SHA="
set "OURS_SHA="
set "THEIRS_SHA="

for /f "tokens=1,2,3,4" %%A in ('git ls-files -u "%file%" 2^>nul') do (
  rem output format: mode SHA stage\tpath
  rem tokens are mode, SHA, stage, path (may vary but this works usually)
  set "tok1=%%A"
  set "tok2=%%B"
  set "tok3=%%C"
  set "tok4=%%D"
  rem We actually need SHA and stage; find stage 1/2/3 by reading fields
)

rem Alternative robust approach: read ls-files -u lines and parse by splitting
for /f "usebackq tokens=2,3" %%A in (`git ls-files -u "%file%" 2^>nul`) do (
  set "sha=%%A"
  set "stage=%%B"
  if "%%B"=="1" set "BASE_SHA=%%A"
  if "%%B"=="2" set "OURS_SHA=%%A"
  if "%%B"=="3" set "THEIRS_SHA=%%A"
)

if "%BASE_SHA%"=="" (
  echo ERROR: could not find base blob for %file%. Skipping.
  goto :cleanup_and_return
)

git show %BASE_SHA% > "%BASE_TMP%" 2>nul
git show %OURS_SHA% > "%OURS_TMP%" 2>nul
git show %THEIRS_SHA% > "%THEIRS_TMP%" 2>nul

echo Merging %file% with UnityYAMLMerge...
"%UNITYYAML%" merge --fallback none -p --force "%BASE_TMP%" "%THEIRS_TMP%" "%OURS_TMP%" "%MERGED_TMP%"
if errorlevel 1 (
  echo UnityYAMLMerge failed for %file%. Merge left unresolved.
  goto :cleanup_and_return
)

copy /y "%MERGED_TMP%" "%file%" >nul
if errorlevel 1 (
  echo Failed to copy merged result back to %file%.
  goto :cleanup_and_return
)
git add "%file%"
if errorlevel 1 (
  echo git add failed for %file%.
) else (
  echo Merged and staged: %file%
)

:cleanup_and_return
if exist "%BASE_TMP%" del /f /q "%BASE_TMP%"
if exist "%OURS_TMP%" del /f /q "%OURS_TMP%"
if exist "%THEIRS_TMP%" del /f /q "%THEIRS_TMP%"
if exist "%MERGED_TMP%" del /f /q "%MERGED_TMP%"
exit /b 0