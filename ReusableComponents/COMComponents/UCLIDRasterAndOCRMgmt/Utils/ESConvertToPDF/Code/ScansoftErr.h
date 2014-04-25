#pragma once

#include <KernelAPI.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// PURPOSE: Adds page size debug information to the specified UCLIDException using information 
//          retrieved from hImgFile about the page at iPageIndex, a 0-based page number.
// NOTE:    Only adds information related to the dimensions of the specified page, it does not
//          add the image filename or page number to the UCLIDException.
void addPageSizeDebugInfo(UCLIDException& ue, HIMGFILE hImgFile, int iPageIndex);
//-------------------------------------------------------------------------------------------------
// PROMISE: Adds debug information to the specified UCLIDException about the last RecAPI error, 
//          which returned the value of rc.
void loadScansoftRecErrInfo(UCLIDException& ue, RECERR rc);
//-------------------------------------------------------------------------------------------------
// PROMISE: If rc != REC_OK, and exception with the specified strELICode and strErrorDescription is
//			created and thrown. If strImageFileName or iPageIndex are specified, they are added as
//			debug info.
void throwExceptionIfNotSuccess(RECERR rc, const string& strELICode,
	const string& strErrorDescription, const string& strFileName = "", int iPageIndex = 0);
//-------------------------------------------------------------------------------------------------