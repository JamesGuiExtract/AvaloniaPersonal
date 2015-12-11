#pragma once

#ifdef _DEBUG

// https://extract.atlassian.net/browse/ISSUE-13516
// Commenting in the below include statement enables reporting of the location and call stacks of
// memory leaks in c++ code via Visual Leak Detector. Use of VLD does seem to prevent COM
// registration from working, so make sure all code is up-to-date, built and registered before
// turning on leak detection. The subsequent build will end up re-building most everything, but
// despite the COM registration failures the registrations from the previous (non-VLD) build should
// still be intact.
// Also, I have found that as of 12/2015, in Visual Studio 2015, v2.4rc2 of VLD seem to cause VS to
// crash. VLD version v2.3 does not cause this crash (though COM registrations still fail).
// 
// Before enabling, ensure:
// 1) Visual Leak Detector is installed (https://vld.codeplex.com) (v2.3 recommended)
// 2) The VLD bin path is added to the Windows path
//		Probably: C:\Program Files (x86)\Visual Leak Detector\bin\Win32
// 3) The VLD include and library paths are registered with visual studio)
//		In VS 2010, via: [UserLocalAppData]\Microsoft\MSBuild\v4.0\Microsoft.Cpp.Win32.user.props
//		Add to IncludePath and LibraryPath, such as:
//		<IncludePath>$(VCInstallDir)PlatformSDK\include;$(IncludePath);C:\Program Files (x86)\Visual Leak Detector\include</IncludePath>
//		<LibraryPath>$(LibraryPath);C:\Program Files (x86)\Visual Leak Detector\lib\Win32</LibraryPath>
// 3) The entire solution is properly built and registered before enabling VLD.
//#include <vld.h> 

#endif