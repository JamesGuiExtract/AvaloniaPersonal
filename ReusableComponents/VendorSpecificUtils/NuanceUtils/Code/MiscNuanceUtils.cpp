
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
		THROW_UE_ON_ERROR("ELI46742", "Unable to open image file.",
			kRecOpenImgFile(strImageFileName.c_str(), &hFile, IMGF_READ, FF_SIZE));

		// Close the image when this goes out of scope
		std::shared_ptr<void> deleteAllocatedMemory(__nullptr, [&](void*)
			{
				RECERR rc = kRecCloseImgFile(hFile);

				// log any errors
				if (rc < REC_OK)
				{
					UCLIDException ue("ELI53264", "Application trace: Unable to close image file. Possible memory leak.");
					loadScansoftRecErrInfo(ue, rc);
					ue.log();
				}
			});

		THROW_UE_ON_ERROR("ELI46743", "Unable to get the page count.",
			kRecGetImgFilePageCount(hFile, &nPageCount));

		return nPageCount;
	}
	catch (...)
	{
		auto ue = uex::fromCurrent("ELI46741");
		ue.addDebugInfo("Image file", strImageFileName);
		throw ue;
	}
}