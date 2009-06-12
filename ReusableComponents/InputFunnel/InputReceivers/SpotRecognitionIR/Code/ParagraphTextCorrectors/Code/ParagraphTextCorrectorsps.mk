
ParagraphTextCorrectorsps.dll: dlldata.obj ParagraphTextCorrectors_p.obj ParagraphTextCorrectors_i.obj
	link /dll /out:ParagraphTextCorrectorsps.dll /def:ParagraphTextCorrectorsps.def /entry:DllMain dlldata.obj ParagraphTextCorrectors_p.obj ParagraphTextCorrectors_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del ParagraphTextCorrectorsps.dll
	@del ParagraphTextCorrectorsps.lib
	@del ParagraphTextCorrectorsps.exp
	@del dlldata.obj
	@del ParagraphTextCorrectors_p.obj
	@del ParagraphTextCorrectors_i.obj
