
UCLIDTestingFrameworkps.dll: dlldata.obj UCLIDTestingFramework_p.obj UCLIDTestingFramework_i.obj
	link /dll /out:UCLIDTestingFrameworkps.dll /def:UCLIDTestingFrameworkps.def /entry:DllMain dlldata.obj UCLIDTestingFramework_p.obj UCLIDTestingFramework_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del UCLIDTestingFrameworkps.dll
	@del UCLIDTestingFrameworkps.lib
	@del UCLIDTestingFrameworkps.exp
	@del dlldata.obj
	@del UCLIDTestingFramework_p.obj
	@del UCLIDTestingFramework_i.obj
