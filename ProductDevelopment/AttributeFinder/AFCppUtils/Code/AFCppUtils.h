#ifndef AFCPP_UTILS_H
#define AFCPP_UTILS_H

#ifdef EXPORT_AFCPPUTILS_DLL
#define EXPORT_AFCppUtils __declspec(dllexport)
#define EXPIMP_TEMPLATE_AFCPPUTILS
#elif NO_EXPORT_IMPORT
#define EXPORT_AFCppUtils
#else
#define EXPORT_AFCppUtils __declspec(dllimport)
#define EXPIMP_TEMPLATE_AFCPPUTILS extern
#endif

class UCLIDException;

//--------------------------------------------------------------------------------------------------
// PURPOSE: Adds the name of the currently executing rule file to the debug info of the specified
//			UCLIDException.
EXPORT_AFCppUtils void addCurrentRSDFileToDebugInfo(UCLIDException &ue);
//--------------------------------------------------------------------------------------------------

#endif
