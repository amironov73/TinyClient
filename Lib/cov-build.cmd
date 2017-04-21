@echo off

SET COVERITY=C:\Coverity
SET COVERITY_BIN=%COVERITY%\bin
SET MSBUILD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
SET PARAMS=/consoleloggerparameters:ErrorsOnly /m

%COVERITY_BIN%\cov-build.exe --dir cov-int "%MSBUILD%" TinyClient.net45.csproj /p:Configuration=Debug /p:TargetFrameworkVersion=4.5 %PARAMS% /p:DefineConstants="CLASSIC;DESKTOP;FW4;FW45" /t:Rebuild
