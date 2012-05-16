
ArcGISIcoMapps.dll: dlldata.obj ArcGISIcoMap_p.obj ArcGISIcoMap_i.obj
	link /dll /out:ArcGISIcoMapps.dll /def:ArcGISIcoMapps.def /entry:DllMain dlldata.obj ArcGISIcoMap_p.obj ArcGISIcoMap_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del ArcGISIcoMapps.dll
	@del ArcGISIcoMapps.lib
	@del ArcGISIcoMapps.exp
	@del dlldata.obj
	@del ArcGISIcoMap_p.obj
	@del ArcGISIcoMap_i.obj
