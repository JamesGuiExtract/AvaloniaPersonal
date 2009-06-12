
LineTextEvaluatorsps.dll: dlldata.obj LineTextEvaluators_p.obj LineTextEvaluators_i.obj
	link /dll /out:LineTextEvaluatorsps.dll /def:LineTextEvaluatorsps.def /entry:DllMain dlldata.obj LineTextEvaluators_p.obj LineTextEvaluators_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del LineTextEvaluatorsps.dll
	@del LineTextEvaluatorsps.lib
	@del LineTextEvaluatorsps.exp
	@del dlldata.obj
	@del LineTextEvaluators_p.obj
	@del LineTextEvaluators_i.obj
