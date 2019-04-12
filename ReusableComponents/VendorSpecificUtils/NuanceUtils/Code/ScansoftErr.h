#pragma once

#include "NuanceUtils.h"

#include <KernelAPI.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// Macros
//-------------------------------------------------------------------------------------------------

// the following macro is used to simplify the process of throwing exception 
// when a RecAPI call has failed
#define THROW_UE(strELICode, strExceptionText, rc) \
	{ \
		UCLIDException ue(strELICode, strExceptionText); \
		loadScansoftRecErrInfo(ue, rc); \
		throw ue; \
	}

// the following macro is used to simplify the process of checking the return code
// from the RecApi calls and throwing exception if the return code is not REC_OK
#define THROW_UE_ON_ERROR(strELICode, strExceptionText, RecAPICall) \
	{ \
		RECERR rc = ##RecAPICall; \
		if (rc != REC_OK) \
		{ \
			THROW_UE(strELICode, strExceptionText, rc); \
		} \
	}

//--------------------------------------------------------------------------------------------------
// PURPOSE: Adds page size debug information to the specified UCLIDException using information 
//          retrieved from hImgFile about the page at iPageIndex, a 0-based page number.
// NOTE:    Only adds information related to the dimensions of the specified page, it does not
//          add the image filename or page number to the UCLIDException.
NUANCEUTILS_API void addPageSizeDebugInfo(UCLIDException& ue, HIMGFILE hImgFile, int iPageIndex);
NUANCEUTILS_API void addPageSizeDebugInfo(UCLIDException& ue, const IMG_INFO& info);
//-------------------------------------------------------------------------------------------------
// PROMISE: Adds debug information to the specified UCLIDException about the last RecAPI error, 
//          which returned the value of rc.
NUANCEUTILS_API void loadScansoftRecErrInfo(UCLIDException& ue, RECERR rc);
//-------------------------------------------------------------------------------------------------
// PROMISE: If rc != REC_OK, and exception with the specified strELICode and strErrorDescription is
//			created and thrown. If strImageFileName or iPageIndex are specified, they are added as
//			debug info.
NUANCEUTILS_API void throwExceptionIfNotSuccess(RECERR rc, const string& strELICode,
	const string& strErrorDescription, const string& strFileName = "", int iPageIndex = 0);
//-------------------------------------------------------------------------------------------------