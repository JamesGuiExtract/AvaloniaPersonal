
#include "stdafx.h"
#include "ScansoftOCRCfg.h"
#include "resource.h"

#include <RegistryPersistenceMgr.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <RegConstants.h>

const string& gstrROOT_REGISTRY_FOLDER = gstrREG_ROOT_KEY + "\\OCREngine\\SSOCR";
const string& gstrROOT_OCR_ENGINE_REGISTRY_FOLDER = gstrREG_ROOT_KEY + "\\OCREngine";
const string& gstrTRADEOFF = "Tradeoff";
const string& gstrPRIMARY_DECOMPOSITION_METHOD = "DefaultDecompositionMethod";
const string& gstrCLEANING_PASS = "CleaningPass";
const string& gstrTHIRD_RECOGNITION_PASS = "ThirdRecognitionPass";
const string& gstrSTR_DISPLAY_FILTER_CHARS = "DisplayFilterChars";
const string& gstrSHOW_PROGRESS_DIALOG = "ShowProgressDlg";
const string& gstrTIMEOUT = "Timeout";
const string& gstrZONE_ORDERING = "ZoneOrdering";
const string& gstrSKIP_PAGE_ON_FAILURE = "SkipPageOnFailure";
const string& gstrMAX_OCR_PAGE_FAILURE_PERCENTAGE = "MaxOcrPageFailurePercentage";
const string& gstrMAX_OCR_PAGE_FAILURE_NUMBER = "MaxOcrPageFailureNumber";

const string& gstrDEFAULT_TRADEOFF = asString((long)TO_ACCURATE);
const string& gstrDEFAULT_PRIMARY_DECOMPOSITION_METHOD = asString(kAutoDecomposition);
const string& gstrDEFAULT_CLEANING_PASS = "1";
const string& gstrDEFAULT_THIRD_RECOGNITION_PASS = "1";
const string& gstrDEFAULT_STR_DISPLAY_FILTER_CHARS = "0";
const string& gstrDEFAULT_SHOW_PROGRESS_DIALOG = "1";
const string& gstrDEFAULT_TIMEOUT = "120000";
const string& gstrDEFAULT_ZONE_ORDERING = "1";
const string& gstrDEFAULT_SKIP_PAGE_ON_FAILURE = "0";
const string& gstrDEFAULT_MAX_OCR_PAGE_FAILURE_PERCENTAGE = "25";
const string& gstrDEFAULT_MAX_OCR_PAGE_FAILURE_NUMBER = "10";

//-------------------------------------------------------------------------------------------------
ScansoftOCRCfg::ScansoftOCRCfg()
{
	try
	{
		// create an instance of the registry persistence manager
		m_pCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, ""));
		ASSERT_RESOURCE_ALLOCATION("ELI10629", m_pCfgMgr.get() != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10628")
}
//-------------------------------------------------------------------------------------------------
ScansoftOCRCfg::~ScansoftOCRCfg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16488");
}
//-------------------------------------------------------------------------------------------------
RMTRADEOFF ScansoftOCRCfg::getTradeoff()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrTRADEOFF))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTRADEOFF, gstrDEFAULT_TRADEOFF);
	}

	return (RMTRADEOFF)asLong(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTRADEOFF,
		gstrDEFAULT_TRADEOFF));
}
//-------------------------------------------------------------------------------------------------
bool ScansoftOCRCfg::getPerformCleaningPass()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrCLEANING_PASS))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrCLEANING_PASS, gstrDEFAULT_CLEANING_PASS);
	}

	return asCppBool(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrCLEANING_PASS,
		gstrDEFAULT_CLEANING_PASS));
}
//-------------------------------------------------------------------------------------------------
bool ScansoftOCRCfg::getPerformThirdRecognitionPass()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrTHIRD_RECOGNITION_PASS))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTHIRD_RECOGNITION_PASS,
			gstrDEFAULT_THIRD_RECOGNITION_PASS);
	}

	return asCppBool(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTHIRD_RECOGNITION_PASS,
		gstrDEFAULT_THIRD_RECOGNITION_PASS));
}
//-------------------------------------------------------------------------------------------------
CScansoftOCR2::EDisplayCharsType ScansoftOCRCfg::getDisplayFilterChars()
{
	// determine the value of m_bDisplayFilterCharsMsg
	// this variable is used to determine whether a msgbox is to be displayed whenever
	// the current filter character set is changed.
	// Check for existence of width
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrSTR_DISPLAY_FILTER_CHARS))
	{
		// Not found, just default to 330
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrSTR_DISPLAY_FILTER_CHARS,
			gstrDEFAULT_STR_DISPLAY_FILTER_CHARS);
		return CScansoftOCR2::kDisplayCharsTypeNone;
	}
	else
	{
		string strValue = m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrSTR_DISPLAY_FILTER_CHARS,
			gstrDEFAULT_STR_DISPLAY_FILTER_CHARS);
		if (strValue == "0")
		{
			return CScansoftOCR2::kDisplayCharsTypeNone;
		}
		else if (strValue == "1")
		{
			return CScansoftOCR2::kDisplayCharsTypeOnChange;
		}
		else if (strValue == "2")
		{
			return CScansoftOCR2::kDisplayCharsTypeAlways;
		}
		else
		{
			m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrSTR_DISPLAY_FILTER_CHARS, "0");
			return CScansoftOCR2::kDisplayCharsTypeNone;
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool ScansoftOCRCfg::getShowProgress()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_OCR_ENGINE_REGISTRY_FOLDER, gstrSHOW_PROGRESS_DIALOG))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_OCR_ENGINE_REGISTRY_FOLDER, gstrSHOW_PROGRESS_DIALOG,
			gstrDEFAULT_SHOW_PROGRESS_DIALOG);
	}

	return asCppBool(m_pCfgMgr->getKeyValue(gstrROOT_OCR_ENGINE_REGISTRY_FOLDER,
		gstrSHOW_PROGRESS_DIALOG, gstrDEFAULT_SHOW_PROGRESS_DIALOG));
}
//-------------------------------------------------------------------------------------------------
long ScansoftOCRCfg::getTimeoutLength()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrTIMEOUT))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTIMEOUT, gstrDEFAULT_TIMEOUT);
	}

	return asLong(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTIMEOUT, gstrDEFAULT_TIMEOUT));
}
//-------------------------------------------------------------------------------------------------
UCLID_SSOCR2Lib::EPageDecompositionMethod ScansoftOCRCfg::getPrimaryDecompositionMethod()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrPRIMARY_DECOMPOSITION_METHOD))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrPRIMARY_DECOMPOSITION_METHOD, 
			gstrDEFAULT_PRIMARY_DECOMPOSITION_METHOD);
	}

	return (UCLID_SSOCR2Lib::EPageDecompositionMethod)
		asLong(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrPRIMARY_DECOMPOSITION_METHOD,
		gstrDEFAULT_PRIMARY_DECOMPOSITION_METHOD));
}
//-------------------------------------------------------------------------------------------------
bool ScansoftOCRCfg::getZoneOrdering()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrZONE_ORDERING))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrZONE_ORDERING, gstrDEFAULT_ZONE_ORDERING);
	}

	return asCppBool(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrZONE_ORDERING,
		gstrDEFAULT_ZONE_ORDERING));
}
//-------------------------------------------------------------------------------------------------
bool ScansoftOCRCfg::getSkipPageOnFailure()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrSKIP_PAGE_ON_FAILURE))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrSKIP_PAGE_ON_FAILURE,
			gstrDEFAULT_SKIP_PAGE_ON_FAILURE);
	}

	return asCppBool(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrSKIP_PAGE_ON_FAILURE,
		gstrDEFAULT_SKIP_PAGE_ON_FAILURE));
}
//-------------------------------------------------------------------------------------------------
unsigned long ScansoftOCRCfg::getMaxOcrPageFailurePercentage()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrMAX_OCR_PAGE_FAILURE_PERCENTAGE))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrMAX_OCR_PAGE_FAILURE_PERCENTAGE,
			gstrDEFAULT_MAX_OCR_PAGE_FAILURE_PERCENTAGE);
	}

	return asUnsignedLong(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER,
		gstrMAX_OCR_PAGE_FAILURE_PERCENTAGE, gstrDEFAULT_MAX_OCR_PAGE_FAILURE_PERCENTAGE));
}
//-------------------------------------------------------------------------------------------------
unsigned long ScansoftOCRCfg::getMaxOcrPageFailureNumber()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrMAX_OCR_PAGE_FAILURE_NUMBER))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrMAX_OCR_PAGE_FAILURE_NUMBER,
			gstrDEFAULT_MAX_OCR_PAGE_FAILURE_NUMBER);
	}

	return asUnsignedLong(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER,
		gstrMAX_OCR_PAGE_FAILURE_NUMBER, gstrDEFAULT_MAX_OCR_PAGE_FAILURE_NUMBER));
}
//-------------------------------------------------------------------------------------------------