set ProjectName="WebApiPlugin"
set TargetPath=.\bin\Debug\net472

if exist "%ParatextInstallDir%\plugins\%ProjectName%"\ (
  del /F /Q "%ParatextInstallDir%\plugins\%ProjectName%"\*.*
) else (
  mkdir "%ParatextInstallDir%\plugins\%ProjectName%"
)

@echo Copying files to %ParatextInstallDir%\plugins\%ProjectName%
xcopy "%TargetPath%\*.dll" "%ParatextInstallDir%\plugins\%ProjectName%" /y /i
xcopy "%TargetPath%\*.pdb" "%ParatextInstallDir%\plugins\%ProjectName%" /y /i
xcopy "%TargetPath%\*.config" "%ParatextInstallDir%\plugins\%ProjectName%" /y /i
xcopy "%TargetPath%\Plugin.bmp" "%ParatextInstallDir%\plugins\%ProjectName%" /y /i

rename "%ParatextInstallDir%\plugins\%ProjectName%\WebApiPlugin.dll" "WebApiPlugin.ptxplg"


rem @echo Copying Named Pipes dll files to %ParatextInstallDir%\plugins\%ProjectName%
rem xcopy "%NamedPipesPath%\*.dll" "%ParatextInstallDir%\plugins\%ProjectName%" /y /i
