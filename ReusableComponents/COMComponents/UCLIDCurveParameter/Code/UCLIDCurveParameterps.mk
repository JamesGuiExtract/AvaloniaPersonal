
UCLIDCurveParameterps.dll: dlldata.obj UCLIDCurveParameter_p.obj UCLIDCurveParameter_i.obj
	link /dll /out:UCLIDCurveParameterps.dll /def:UCLIDCurveParameterps.def /entry:DllMain dlldata.obj UCLIDCurveParameter_p.obj UCLIDCurveParameter_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del UCLIDCurveParameterps.dll
	@del UCLIDCurveParameterps.lib
	@del UCLIDCurveParameterps.exp
	@del dlldata.obj
	@del UCLIDCurveParameter_p.obj
	@del UCLIDCurveParameter_i.obj
