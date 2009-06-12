
SpotRecIRAutoTestps.dll: dlldata.obj SpotRecIRAutoTest_p.obj SpotRecIRAutoTest_i.obj
	link /dll /out:SpotRecIRAutoTestps.dll /def:SpotRecIRAutoTestps.def /entry:DllMain dlldata.obj SpotRecIRAutoTest_p.obj SpotRecIRAutoTest_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del SpotRecIRAutoTestps.dll
	@del SpotRecIRAutoTestps.lib
	@del SpotRecIRAutoTestps.exp
	@del dlldata.obj
	@del SpotRecIRAutoTest_p.obj
	@del SpotRecIRAutoTest_i.obj
