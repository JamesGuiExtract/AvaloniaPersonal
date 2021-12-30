
#include "pch.h"
#include "NuanceUtils.h"
#include "ScansoftErr.h"
#include "MiscNuanceUtils.h"

#include <KernelAPI.h>
#include <UCLIDException.h>


int getNumberOfPagesInImageNuance(const std::string& strImageFileName)
{
	try
	{
		HIMGFILE hFile;
		int nPageCount;
		THROW_UE_ON_ERROR(
			"ELI46742", "Unable to open image file.",
			kRecOpenImgFile(strImageFileName.c_str(), &hFile, IMGF_READ, FF_SIZE));
		
		THROW_UE_ON_ERROR("ELI46743", "Unable to get the page count.",
			kRecGetImgFilePageCount(hFile, &nPageCount));
		
		THROW_UE_ON_ERROR("ELI46744", "Unable to get the page count.",
			kRecCloseImgFile(hFile));

		return nPageCount;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46741");
}