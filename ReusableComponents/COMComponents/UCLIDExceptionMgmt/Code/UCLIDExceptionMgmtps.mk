
UCLIDExceptionMgmtps.dll: dlldata.obj UCLIDExceptionMgmt_p.obj UCLIDExceptionMgmt_i.obj
	link /dll /out:UCLIDExceptionMgmtps.dll /def:UCLIDExceptionMgmtps.def /entry:DllMain dlldata.obj UCLIDExceptionMgmt_p.obj UCLIDExceptionMgmt_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del UCLIDExceptionMgmtps.dll
	@del UCLIDExceptionMgmtps.lib
	@del UCLIDExceptionMgmtps.exp
	@del dlldata.obj
	@del UCLIDExceptionMgmt_p.obj
	@del UCLIDExceptionMgmt_i.obj
