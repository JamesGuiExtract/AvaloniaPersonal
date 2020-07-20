
#include "pch.h"
#include "ScansoftErr.h"


//-------------------------------------------------------------------------------------------------
void addPageSizeDebugInfo(UCLIDException& ue, HIMGFILE hImgFile, int iPageIndex)
{
	try
	{
		// get the image file page information if it is available
		IMG_INFO imgInfo;
		RECERR rc = kRecGetImgFilePageInfo(0, hImgFile, iPageIndex, &imgInfo, NULL);
		if (rc != REC_OK)
		{
			UCLIDException ue2("ELI49960", "Unable to get page size information.");
			loadScansoftRecErrInfo(ue2, rc);
			throw ue;
		}

		// add page size bounds in pixels
		ue.addDebugInfo("X,Y Pixels", asString(imgInfo.Size.cx) + ", " + asString(imgInfo.Size.cy));
	}
	catch (UCLIDException ue2)
	{
		// unable to determine image size info
		ue.addDebugInfo("X,Y Pixels", "Unable to determine.");

		// log the exception thrown by kRecGetImgFilePageInfo
		ue2.log();
	}

	ue.addDebugInfo("X,Y Limits", "8400 x 8400 pixels");
}
//--------------------------------------------------------------------------------------------------
void addPageSizeDebugInfo(UCLIDException& ue, const IMG_INFO& info)
{
	// add page size bounds in pixels
	ue.addDebugInfo("X,Y Pixels", asString(info.Size.cx) + ", " +
		asString(info.Size.cy));
	ue.addDebugInfo("X,Y Limits", "8400 x 8400 pixels");
}
//-------------------------------------------------------------------------------------------------
void loadScansoftRecErrInfo(UCLIDException & ue, RECERR rc)
{
	long lExtendedErrorCode = 0;
	char pszExtendedErrorDescription[] = "";

	// get the extended error code information from the last error
	kRecGetLastError(&lExtendedErrorCode, pszExtendedErrorDescription,
		sizeof(pszExtendedErrorDescription));

	const char* pszSymbolicErrorName;

	// get the symbolic name of the error
	kRecGetErrorInfo(rc, &pszSymbolicErrorName);

	// get length of the error description
	int iErrorDescriptionLength = 0;
	kRecGetErrorUIText(rc, lExtendedErrorCode, pszExtendedErrorDescription,
		__nullptr, &iErrorDescriptionLength);

	// allocate space for the error description
	unique_ptr<char[]> pHelper(new char[iErrorDescriptionLength]);

	// get the error description
	kRecGetErrorUIText(rc, lExtendedErrorCode, pszExtendedErrorDescription,
		pHelper.get(), &iErrorDescriptionLength);

	// add the debug info
	ue.addDebugInfo("Error description", pHelper.get());
	ue.addDebugInfo("Error code", pszSymbolicErrorName);

	// add extended debug information if it is available
	if (lExtendedErrorCode != 0)
	{
		ue.addDebugInfo("Extended error description", pszExtendedErrorDescription);
		ue.addDebugInfo("Extended error code", lExtendedErrorCode);
	}
}
//-------------------------------------------------------------------------------------------------
void throwExceptionIfNotSuccess(RECERR rc, const string & strELICode,
	const string & strErrorDescription, const string & strFileName/* = ""*/, int iPageIndex/* = 0*/)
{
	if (rc != REC_OK)
	{
		UCLIDException ue(strELICode, strErrorDescription);
		loadScansoftRecErrInfo(ue, rc);
		if (!strFileName.empty())
		{
			ue.addDebugInfo("Filename", strFileName);
		}
		if (iPageIndex > 0)
		{
			ue.addDebugInfo("Page", iPageIndex);
		}
		throw ue;
	}
}