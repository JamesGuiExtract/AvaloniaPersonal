#pragma once

// Default target to Windows XP
#ifndef WINVER
#define WINVER 0x0600
#endif


#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0600
#endif

// This was added for https://extract.atlassian.net/browse/ISSUE-14581
// the Multi byte character support was deprecated for VS2013 but in has been un deprecated for VS2017
// https://devblogs.microsoft.com/cppblog/mfc-support-for-mbcs-deprecated-in-visual-studio-2013/
#ifndef NO_WARN_MBCS_MFC_DEPRECATION
#define NO_WARN_MBCS_MFC_DEPRECATION
#endif