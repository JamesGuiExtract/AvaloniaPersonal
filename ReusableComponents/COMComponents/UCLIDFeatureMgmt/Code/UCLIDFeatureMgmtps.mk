
UCLIDFeatureMgmtps.dll: dlldata.obj UCLIDFeatureMgmt_p.obj UCLIDFeatureMgmt_i.obj
	link /dll /out:UCLIDFeatureMgmtps.dll /def:UCLIDFeatureMgmtps.def /entry:DllMain dlldata.obj UCLIDFeatureMgmt_p.obj UCLIDFeatureMgmt_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del UCLIDFeatureMgmtps.dll
	@del UCLIDFeatureMgmtps.lib
	@del UCLIDFeatureMgmtps.exp
	@del dlldata.obj
	@del UCLIDFeatureMgmt_p.obj
	@del UCLIDFeatureMgmt_i.obj
