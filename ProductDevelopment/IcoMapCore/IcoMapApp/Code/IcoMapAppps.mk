
IcoMapAppps.dll: dlldata.obj IcoMapApp_p.obj IcoMapApp_i.obj
	link /dll /out:IcoMapAppps.dll /def:IcoMapAppps.def /entry:DllMain dlldata.obj IcoMapApp_p.obj IcoMapApp_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del IcoMapAppps.dll
	@del IcoMapAppps.lib
	@del IcoMapAppps.exp
	@del dlldata.obj
	@del IcoMapApp_p.obj
	@del IcoMapApp_i.obj
