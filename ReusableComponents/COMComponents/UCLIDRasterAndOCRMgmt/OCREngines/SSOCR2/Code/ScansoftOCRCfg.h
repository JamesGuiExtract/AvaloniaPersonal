#pragma once

#include "SSOCR2.h"
#include "ScansoftOCR2.h"

#include <IConfigurationSettingsPersistenceMgr.h>

#include <memory>
#include <string>

using namespace std;

class ScansoftOCRCfg
{
public:
	//---------------------------------------------------------------------------------------------
	// REQUIRE: pDlg is a pointer to the dialog whose window
	//			position needs to be restored or saved
	//			Also, pDlg should be a valid pointer for the lifetime of this
	//			object.
	ScansoftOCRCfg();
	~ScansoftOCRCfg();
	//---------------------------------------------------------------------------------------------
	RMTRADEOFF getTradeoff();
	//---------------------------------------------------------------------------------------------
	bool getPerformCleaningPass();
	//---------------------------------------------------------------------------------------------
	bool getPerformThirdRecognitionPass();
	//---------------------------------------------------------------------------------------------
	CScansoftOCR2::EDisplayCharsType getDisplayFilterChars();
	//---------------------------------------------------------------------------------------------
	bool getShowProgress();
	//---------------------------------------------------------------------------------------------
	long getTimeoutLength();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the primary decomposition method as set in the registry. If no registry 
	//          key exists, sets it to a default value and returns that value.
	UCLID_SSOCR2Lib::EPageDecompositionMethod getPrimaryDecompositionMethod();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns zone ordering key as set in the registry. If the zone ordering key does not 
	//          exist, one will be created with a default value and that value will be returned.
	bool getZoneOrdering();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the true if pages should be skipped if they fail; false if the whole 
	//          document should fail when a page fails.
	bool getSkipPageOnFailure();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the maximum percentage of pages that can fail without failing the document.
	unsigned long getMaxOcrPageFailurePercentage();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the maximum number of pages that can fail without failing the document.
	unsigned long getMaxOcrPageFailureNumber();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets a value specifying the despeckling options to use before OCR'ing an image.
	//			Bit 0 (low bit): kRecSetImgDespeckleMode (0 = FALSE, 1 = TRUE)
	//			Bit 1: 1 = call kRecForceDespeckleImg, 0 = don't call.
	//			Bits 2 - 6: DESPECKLE_METHOD for kRecForceDespeckleImg.
	//			Bits 7+: Level for kRecForceDespeckleImg.
	//			Example: 355 =
	//				kRecSetImgDespeckleMode = TRUE
	//				kRecForceDespeckleImg(DESPECKLE_PEPPERANDSALT, 2)
	//			Default = 353 (kRecSetImgDespeckleMode = TRUE)
	unsigned long getDespeckleMode();
	
private:
	// pointer to the persistence manager for registry access
	unique_ptr<IConfigurationSettingsPersistenceMgr> m_pCfgMgr;
};