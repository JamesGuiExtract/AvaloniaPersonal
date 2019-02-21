#include "stdafx.h"
#include "LocalPDFOptions.h"

#include "MiscLeadUtils.h"
#include "LeadToolsLicenseRestrictor.h"

CLocalPDFOptions::CLocalPDFOptions(void)
: m_bInitialized(false)
{
	InitLeadToolsLicense();

	// Get initialized FILEPDFOPTIONS struct
	m_pdfOptionsOriginal = GetLeadToolsSizedStruct<FILEPDFOPTIONS>(0);
	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;
		throwExceptionIfNotSuccess(
			L_GetPDFOptions(&m_pdfOptionsOriginal, m_pdfOptionsOriginal.uStructSize),
			"ELI37111", "Failed to get PDF options.");
	}

	memcpy(&m_pdfOptions, &m_pdfOptionsOriginal, sizeof(m_pdfOptionsOriginal));

	m_pdfRasterizeDocOptionsOriginal = GetLeadToolsSizedStruct<RASTERIZEDOCOPTIONS>(0);
	
	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;
		throwExceptionIfNotSuccess(
			L_GetRasterizeDocOptions(&m_pdfRasterizeDocOptionsOriginal, m_pdfRasterizeDocOptionsOriginal.uStructSize),
			"ELI41715", "Failed to get PDF rasterize options");
	}

	memcpy(&m_pdfRasterizeDocOptions, &m_pdfRasterizeDocOptionsOriginal, sizeof(m_pdfRasterizeDocOptions));

	
	m_bInitialized = true;
}
//-------------------------------------------------------------------------------------------------
CLocalPDFOptions::~CLocalPDFOptions(void)
{
	try
	{
		if (memcmp(&m_pdfOptionsOriginal, &m_pdfOptions, m_pdfOptionsOriginal.uStructSize) != 0)
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			throwExceptionIfNotSuccess(
				L_SetPDFOptions(&m_pdfOptionsOriginal),
				"ELI37112", "Failed to restore PDF options.");
		}

		if (memcmp(&m_pdfRasterizeDocOptionsOriginal, &m_pdfRasterizeDocOptions, m_pdfRasterizeDocOptionsOriginal.uStructSize) != 0)
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			throwExceptionIfNotSuccess(
				L_SetRasterizeDocOptions(&m_pdfRasterizeDocOptionsOriginal),
				"ELI41716", "Failed to restore PDF rasterize options.");
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37110");
}
//-------------------------------------------------------------------------------------------------
void CLocalPDFOptions::ApplyPDFOptions(const string &strELICode, const string &strErrorDescription)
{
	LeadToolsLicenseRestrictor leadToolsLicenseGuard;
	throwExceptionIfNotSuccess(L_SetPDFOptions(&m_pdfOptions), strELICode, strErrorDescription);
	throwExceptionIfNotSuccess(L_SetRasterizeDocOptions(&m_pdfRasterizeDocOptions), strELICode, strErrorDescription);

}
//-------------------------------------------------------------------------------------------------