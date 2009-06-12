
UCLIDDistanceConverterps.dll: dlldata.obj UCLIDDistanceConverter_p.obj UCLIDDistanceConverter_i.obj
	link /dll /out:UCLIDDistanceConverterps.dll /def:UCLIDDistanceConverterps.def /entry:DllMain dlldata.obj UCLIDDistanceConverter_p.obj UCLIDDistanceConverter_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del UCLIDDistanceConverterps.dll
	@del UCLIDDistanceConverterps.lib
	@del UCLIDDistanceConverterps.exp
	@del dlldata.obj
	@del UCLIDDistanceConverter_p.obj
	@del UCLIDDistanceConverter_i.obj
