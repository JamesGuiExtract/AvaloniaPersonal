
UCLIDTestingFrameworkCoreps.dll: dlldata.obj UCLIDTestingFrameworkCore_p.obj UCLIDTestingFrameworkCore_i.obj
	link /dll /out:UCLIDTestingFrameworkCoreps.dll /def:UCLIDTestingFrameworkCoreps.def /entry:DllMain dlldata.obj UCLIDTestingFrameworkCore_p.obj UCLIDTestingFrameworkCore_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del UCLIDTestingFrameworkCoreps.dll
	@del UCLIDTestingFrameworkCoreps.lib
	@del UCLIDTestingFrameworkCoreps.exp
	@del dlldata.obj
	@del UCLIDTestingFrameworkCore_p.obj
	@del UCLIDTestingFrameworkCore_i.obj
