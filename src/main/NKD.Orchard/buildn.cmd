if "%~1"=="" build Build
msbuild /t:%~1 NKD.proj
