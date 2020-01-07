#include "pch.h"
#include "OcrMethods.h"
#include "ScansoftErr.h"

#include <iomanip>
#include <UCLIDException.h>

void loadPageFromImageHandle(const string& strImage, HIMGFILE hImage, int iPageIndex, HPAGE* phPage)
{
	RECERR rc = kRecLoadImg(0, hImage, phPage, iPageIndex);
	if (rc != REC_OK && rc != IMF_PASSWORD_WARN && rc != IMG_NOMORE_WARN && rc != IMF_READ_WARN &&
		rc != IMF_COMP_WARN)
	{
		// Determine whether this page was able to be loaded despite errors in the document
		bool bFail = phPage == NULL || *phPage == NULL; 

		// Create a scary or friendly exception based on whether page was loaded
		string strEli; 
		string strMessage;
		if (bFail)
		{
			strEli = "ELI05773";
			strMessage = "Unable to load image file in the OCR engine.";
		}
		else
		{
			strEli = "ELI29877";
			strMessage = "Application trace: Image loaded successfully but contains errors.";
		}

		// Create the exception
		UCLIDException ue(strEli, strMessage);
		loadScansoftRecErrInfo(ue, rc);
		ue.addDebugInfo("Image File", strImage);
		ue.addDebugInfo("Page Number", iPageIndex + 1);

		// try to add page size information (P13 #4603 - JDS)
		if (rc == IMG_SIZE_ERR)
		{
			addPageSizeDebugInfo(ue, hImage, iPageIndex);
		}

		// Throw or log the exception
		if (bFail)
		{
			throw ue;
		}
		else
		{
			ue.log();
		}
	}
}
//-------------------------------------------------------------------------------------------------
string getTemporaryDataFolder(long pid)
{
	string tmpDir;
	getTempDir(tmpDir);

	stringstream ss;
	ss << tmpDir << "\\Extract Systems\\SSOCR2\\" << setfill('0') << setw(6) << pid;

	return ss.str();
}