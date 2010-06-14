
#include "stdafx.h"
#include "ScansoftErr.h"

#include <UCLIDException.h>
#include <cpputil.h>

//--------------------------------------------------------------------------------------------------
void loadScansoftRecErrInfo(UCLIDException& ue, RECERR rc)
{
	long lExtendedErrorCode = 0;
	char pszExtendedErrorDescription[] = "";

	// get the extended error code information from the last error
	kRecGetLastError(&lExtendedErrorCode, pszExtendedErrorDescription, 
		sizeof(pszExtendedErrorDescription));

	const char *pszSymbolicErrorName;

	// get the symbolic name of the error
	kRecGetErrorInfo(rc, &pszSymbolicErrorName);

	char *pszErrorDescription = NULL;
	int iErrorDescriptionLength = 0;

	// get length of the error description
	kRecGetErrorUIText(rc, lExtendedErrorCode, pszExtendedErrorDescription, 
		pszErrorDescription, &iErrorDescriptionLength);

	// allocate space for the error description
	pszErrorDescription = new char[iErrorDescriptionLength];

	// get the error description
	kRecGetErrorUIText(rc, lExtendedErrorCode, pszExtendedErrorDescription, 
		pszErrorDescription, &iErrorDescriptionLength);

	// add the debug info
	ue.addDebugInfo("Error description", pszErrorDescription);
	ue.addDebugInfo("Error code", pszSymbolicErrorName);
	
	// add extended debug information if it is available
	if (lExtendedErrorCode != 0)
	{
		ue.addDebugInfo("Extended error description", pszExtendedErrorDescription);
		ue.addDebugInfo("Extended error code", lExtendedErrorCode);
	}

	// free the allocated space
	delete [] pszErrorDescription;
}
//--------------------------------------------------------------------------------------------------
void addPageSizeDebugInfo(UCLIDException& ue, const IMG_INFO& info)
{
	// add page size bounds in pixels
	ue.addDebugInfo( "X,Y Pixels", asString(info.Size.cx) + ", " + 
		asString(info.Size.cy) );
	ue.addDebugInfo( "X,Y Limits", "8400 x 8400 pixels" );
}
//--------------------------------------------------------------------------------------------------
void addPageSizeDebugInfo(UCLIDException& ue, HIMGFILE hImgFile, int iPageIndex)
{
	try
	{
		// get the image file page information if it is available
		IMG_INFO imgInfo;
		RECERR rc = kRecGetImgFilePageInfo(0, hImgFile, iPageIndex, &imgInfo, NULL);
		if(rc != REC_OK)
		{
			UCLIDException ue2("ELI29880", "Unable to get page size information.");
			loadScansoftRecErrInfo(ue2, rc);
			throw ue;
		}

		// add page size bounds in pixels
		ue.addDebugInfo("X,Y Pixels", asString(imgInfo.Size.cx) + ", " + asString(imgInfo.Size.cy));
	}
	catch(UCLIDException ue2)
	{
		// unable to determine image size info
		ue.addDebugInfo("X,Y Pixels", "Unable to determine.");
		
		// log the exception thrown by kRecGetImgFilePageInfo
		ue2.log();
	}

	ue.addDebugInfo("X,Y Max Limits", "8400 x 8400 pixels");
	ue.addDebugInfo("X,Y Min Limits", "16 x 16 pixels");
}

