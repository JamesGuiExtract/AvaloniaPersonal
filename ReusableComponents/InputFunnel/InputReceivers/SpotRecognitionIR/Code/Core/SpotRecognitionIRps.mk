
SpotRecognitionIRps.dll: dlldata.obj SpotRecognitionIR_p.obj SpotRecognitionIR_i.obj
	link /dll /out:SpotRecognitionIRps.dll /def:SpotRecognitionIRps.def /entry:DllMain dlldata.obj SpotRecognitionIR_p.obj SpotRecognitionIR_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del SpotRecognitionIRps.dll
	@del SpotRecognitionIRps.lib
	@del SpotRecognitionIRps.exp
	@del dlldata.obj
	@del SpotRecognitionIR_p.obj
	@del SpotRecognitionIR_i.obj
