
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
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTRADEOFF, asString((long)TO_ACCURATE));
	}

	return (RMTRADEOFF)asLong(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTRADEOFF));
}
//-------------------------------------------------------------------------------------------------
bool ScansoftOCRCfg::getPerformCleaningPass()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrCLEANING_PASS))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrCLEANING_PASS, "1");
	}

	return m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrCLEANING_PASS) != "0";
}
//-------------------------------------------------------------------------------------------------
bool ScansoftOCRCfg::getPerformThirdRecognitionPass()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrTHIRD_RECOGNITION_PASS))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTHIRD_RECOGNITION_PASS, "1");
	}

	return m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTHIRD_RECOGNITION_PASS) != "0";
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
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrSTR_DISPLAY_FILTER_CHARS, "0");
		return CScansoftOCR2::kDisplayCharsTypeNone;
	}
	else
	{
		string strValue = m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrSTR_DISPLAY_FILTER_CHARS);
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
		m_pCfgMgr->setKeyValue(gstrROOT_OCR_ENGINE_REGISTRY_FOLDER, gstrSHOW_PROGRESS_DIALOG, "1");
	}

	return m_pCfgMgr->getKeyValue(gstrROOT_OCR_ENGINE_REGISTRY_FOLDER, gstrSHOW_PROGRESS_DIALOG) != "0";
}
//-------------------------------------------------------------------------------------------------
long ScansoftOCRCfg::getTimeoutLength()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrTIMEOUT))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTIMEOUT, "120000");
	}

	return asLong(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrTIMEOUT));
}
//-------------------------------------------------------------------------------------------------
UCLID_SSOCR2Lib::EPageDecompositionMethod ScansoftOCRCfg::getPrimaryDecompositionMethod()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrPRIMARY_DECOMPOSITION_METHOD))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrPRIMARY_DECOMPOSITION_METHOD, 
			asString(kAutoDecomposition));
	}

	return (UCLID_SSOCR2Lib::EPageDecompositionMethod)
		asLong(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrPRIMARY_DECOMPOSITION_METHOD));
}
//-------------------------------------------------------------------------------------------------
bool ScansoftOCRCfg::getZoneOrdering()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrZONE_ORDERING))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrZONE_ORDERING, "1");
	}

	return m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrZONE_ORDERING) != "0";
}
//-------------------------------------------------------------------------------------------------
bool ScansoftOCRCfg::getSkipPageOnFailure()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrSKIP_PAGE_ON_FAILURE))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrSKIP_PAGE_ON_FAILURE, "0");
	}

	return m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrSKIP_PAGE_ON_FAILURE) != "0";
}
//-------------------------------------------------------------------------------------------------
unsigned long ScansoftOCRCfg::getMaxOcrPageFailurePercentage()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrMAX_OCR_PAGE_FAILURE_PERCENTAGE))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrMAX_OCR_PAGE_FAILURE_PERCENTAGE, "25");
	}

	return asUnsignedLong(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrMAX_OCR_PAGE_FAILURE_PERCENTAGE));
}
//-------------------------------------------------------------------------------------------------
unsigned long ScansoftOCRCfg::getMaxOcrPageFailureNumber()
{
	if (!m_pCfgMgr->keyExists(gstrROOT_REGISTRY_FOLDER, gstrMAX_OCR_PAGE_FAILURE_NUMBER))
	{
		m_pCfgMgr->setKeyValue(gstrROOT_REGISTRY_FOLDER, gstrMAX_OCR_PAGE_FAILURE_NUMBER, "10");
	}

	return asUnsignedLong(m_pCfgMgr->getKeyValue(gstrROOT_REGISTRY_FOLDER, gstrMAX_OCR_PAGE_FAILURE_NUMBER));
}
//-------------------------------------------------------------------------------------------------