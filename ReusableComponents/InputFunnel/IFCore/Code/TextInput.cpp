// TextInput.cpp : Implementation of CTextInput
#include "stdafx.h"
#include "IFCore.h"
#include "TextInput.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CTextInput
//-------------------------------------------------------------------------------------------------
CTextInput::CTextInput()
: m_cbstrText(""),
  m_cipValidatedInput(NULL),
  m_ipInputEntity(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CTextInput::~CTextInput()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16464");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInput::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITextInput
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ITextInput
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInput::InitTextInput(IInputEntity *pEntity, BSTR strText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		// set input entity
		m_ipInputEntity = pEntity;
		// set input text
		m_cbstrText = strText;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02459")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInput::SetText(BSTR strText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// set input text
		m_cbstrText = strText;
		
		// if input entity exists
		if (m_ipInputEntity)
		{
			m_ipInputEntity->SetText(get_bstr_t(strText));
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02462")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInput::GetText(BSTR *pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		// return input text
		m_cbstrText.CopyTo(pstrText);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02465")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInput::GetValidatedInput(IUnknown **pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_cipValidatedInput)
		{
			m_cipValidatedInput.CopyTo(pObj);
		}
		else
		{
			*pObj = NULL;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02468")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInput::SetValidatedInput(IUnknown *pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// set the validated input
		m_cipValidatedInput = pObj;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02471")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInput::GetInputEntity(IInputEntity **pEntity)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipInputEntity)
		{
			// copy m_ipInputEntity.
			UCLID_INPUTFUNNELLib::IInputEntityPtr ipShallowCopy = m_ipInputEntity;
			*pEntity = (IInputEntity*) ipShallowCopy.Detach();
		}
		else
		{
			*pEntity = NULL;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02474")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInput::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CTextInput::validateLicense()
{
	static const unsigned long TEXT_INPUT_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( TEXT_INPUT_COMPONENT_ID, 
		"ELI02604", "Text Input" );
}
//-------------------------------------------------------------------------------------------------

