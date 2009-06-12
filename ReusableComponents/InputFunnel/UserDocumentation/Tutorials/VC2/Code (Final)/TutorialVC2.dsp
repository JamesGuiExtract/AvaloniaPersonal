# Microsoft Developer Studio Project File - Name="TutorialVC2" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Application" 0x0101

CFG=TutorialVC2 - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "TutorialVC2.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "TutorialVC2.mak" CFG="TutorialVC2 - Win32 Debug"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "TutorialVC2 - Win32 Release" (based on "Win32 (x86) Application")
!MESSAGE "TutorialVC2 - Win32 Debug" (based on "Win32 (x86) Application")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
CPP=cl.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "TutorialVC2 - Win32 Release"

# PROP BASE Use_MFC 6
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "Release"
# PROP BASE Intermediate_Dir "Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 6
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "Release"
# PROP Intermediate_Dir "Release"
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MD /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_AFXDLL" /Yu"stdafx.h" /FD /c
# ADD CPP /nologo /MD /W3 /GX /O2 /I "..\..\..\Bin" /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_AFXDLL" /D "_MBCS" /Yu"stdafx.h" /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "NDEBUG" /d "_AFXDLL"
# ADD RSC /l 0x409 /d "NDEBUG" /d "_AFXDLL"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 /nologo /subsystem:windows /machine:I386
# ADD LINK32 /nologo /subsystem:windows /machine:I386

!ELSEIF  "$(CFG)" == "TutorialVC2 - Win32 Debug"

# PROP BASE Use_MFC 6
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "Debug"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 6
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "Debug"
# PROP Intermediate_Dir "Debug"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MDd /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /D "_AFXDLL" /Yu"stdafx.h" /FD /GZ /c
# ADD CPP /nologo /MDd /W3 /Gm /GX /ZI /Od /I "..\..\..\Bin" /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /D "_AFXDLL" /D "_MBCS" /Yu"stdafx.h" /FD /GZ /c
# ADD BASE MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "_DEBUG" /d "_AFXDLL"
# ADD RSC /l 0x409 /d "_DEBUG" /d "_AFXDLL"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 /nologo /subsystem:windows /debug /machine:I386 /pdbtype:sept
# ADD LINK32 /nologo /subsystem:windows /debug /machine:I386 /pdbtype:sept

!ENDIF 

# Begin Target

# Name "TutorialVC2 - Win32 Release"
# Name "TutorialVC2 - Win32 Debug"
# Begin Group "Source Files"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;idl;hpj;bat"
# Begin Source File

SOURCE=.\inputmanager.cpp
# End Source File
# Begin Source File

SOURCE=.\inputreceiver.cpp
# End Source File
# Begin Source File

SOURCE=.\iunknownvector.cpp
# End Source File
# Begin Source File

SOURCE=.\ocrfiltermgr.cpp
# End Source File
# Begin Source File

SOURCE=.\StdAfx.cpp
# ADD CPP /Yc"stdafx.h"
# End Source File
# Begin Source File

SOURCE=.\TutorialVC2.cpp
# End Source File
# Begin Source File

SOURCE=.\TutorialVC2.rc
# End Source File
# Begin Source File

SOURCE=.\TutorialVC2Dlg.cpp
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hpp;hxx;hm;inl"
# Begin Source File

SOURCE=.\inputmanager.h
# End Source File
# Begin Source File

SOURCE=.\inputreceiver.h
# End Source File
# Begin Source File

SOURCE=.\iunknownvector.h
# End Source File
# Begin Source File

SOURCE=.\ocrfiltermgr.h
# End Source File
# Begin Source File

SOURCE=.\Resource.h
# End Source File
# Begin Source File

SOURCE=.\StdAfx.h
# End Source File
# Begin Source File

SOURCE=.\TutorialVC2.h
# End Source File
# Begin Source File

SOURCE=.\TutorialVC2Dlg.h
# End Source File
# End Group
# Begin Group "Resource Files"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe"
# Begin Source File

SOURCE=.\res\TutorialVC2.ico
# End Source File
# Begin Source File

SOURCE=.\res\TutorialVC2.rc2
# End Source File
# End Group
# Begin Source File

SOURCE=.\ReadMe.txt
# End Source File
# End Target
# End Project
# Section TutorialVC2 : {1AA123A1-1FD6-4620-8CD2-31C3F9E7CAB2}
# 	2:5:Class:CIUnknownVector
# 	2:10:HeaderFile:iunknownvector.h
# 	2:8:ImplFile:iunknownvector.cpp
# End Section
# Section TutorialVC2 : {9D45CAE9-7052-4AE1-BD83-68C213E8BA22}
# 	2:5:Class:CIUnknownVector
# 	2:10:HeaderFile:iunknownvector.h
# 	2:8:ImplFile:iunknownvector.cpp
# End Section
# Section TutorialVC2 : {775ACCAD-32AC-11D6-8259-0050DAD4FF55}
# 	2:21:DefaultSinkHeaderFile:inputmanager.h
# 	2:16:DefaultSinkClass:CInputManager
# End Section
# Section TutorialVC2 : {775ACCAC-32AC-11D6-8259-0050DAD4FF55}
# 	2:5:Class:CInputManager
# 	2:10:HeaderFile:inputmanager.h
# 	2:8:ImplFile:inputmanager.cpp
# End Section
# Section TutorialVC2 : {775ACCA8-32AC-11D6-8259-0050DAD4FF55}
# 	2:5:Class:CInputReceiver
# 	2:10:HeaderFile:inputreceiver.h
# 	2:8:ImplFile:inputreceiver.cpp
# End Section
# Section TutorialVC2 : {963C8A62-B21F-4FB3-BB3E-10BE3463BFE2}
# 	2:5:Class:COCRFilterMgr
# 	2:10:HeaderFile:ocrfiltermgr.h
# 	2:8:ImplFile:ocrfiltermgr.cpp
# End Section
