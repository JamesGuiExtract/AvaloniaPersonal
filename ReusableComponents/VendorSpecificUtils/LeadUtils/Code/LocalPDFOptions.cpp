#include "stdafx.h"
#include "LocalPDFOptions.h"

#include "MiscLeadUtils.h"

CLocalPDFOptions::CLocalPDFOptions(void)
: m_bInitialized(false)
{
	// Get initialized FILEPDFOPTIONS struct
	m_pdfOptionsOriginal = GetLeadToolsSizedStruct<FILEPDFOPTIONS>(0);

	int nSize = sizeof(m_pdfOptionsOriginal);
	throwExceptionIfNotSuccess(
		L_GetPDFOptions(&m_pdfOptionsOriginal, m_pdfOptionsOriginal.uStructSize),
		"ELI37111", "Failed to get PDF options.");

	memcpy(&m_pdfOptions, &m_pdfOptionsOriginal, sizeof(m_pdfOptionsOriginal));

	m_bInitialized = true;
}
//-------------------------------------------------------------------------------------------------
CLocalPDFOptions::~CLocalPDFOptions(void)
{
	try
	{
		if (memcmp(&m_pdfOptionsOriginal, &m_pdfOptions, m_pdfOptionsOriginal.uStructSize) != 0)
		{
			throwExceptionIfNotSuccess(
				L_SetPDFOptions(&m_pdfOptionsOriginal),
				"ELI37112", "Failed to restore PDF options.");
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37110");
}
//-------------------------------------------------------------------------------------------------
void CLocalPDFOptions::ApplyPDFOptions(const string &strELICode, const string &strErrorDescription)
{
	throwExceptionIfNotSuccess(L_SetPDFOptions(&m_pdfOptions), strELICode, strErrorDescription);
}
//-------------------------------------------------------------------------------------------------