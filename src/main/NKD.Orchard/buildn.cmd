if "%~1"=="" build Naked
msbuild /t:%~1 NKD.proj

