@echo Installing IDShieldForSPClient
@IF  "C:\Program Files (x86)"=="%ProgramFiles(x86)%" (
	COPY "%~dp0*.*" "C:\Program Files (x86)\Extract Systems\CommonComponents"
) ELSE (
	COPY "%~dp0*.*" "C:\Program Files\Extract Systems\CommonComponents"
)

