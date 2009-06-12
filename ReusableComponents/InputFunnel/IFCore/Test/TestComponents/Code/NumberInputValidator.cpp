// NumberInputValidator.cpp : Implementation of CNumberInputValidator
#include "stdafx.h"
#include "TestComponents.h"
#include "NumberInputValidator.h"

#include "InputCorrectionDlg.h"

#include <TemporaryResourceOverride.h>

extern HINSTANCE g_Resource;

/////////////////////////////////////////////////////////////////////////////
// CNumberInputValidator
CNumberInputValidator::CNumberInputValidator()
{
}

CNumberInputValidator::~CNumberInputValidator()
{
}

// IInputValidator
STDMETHODIMP CNumberInputValidator::ValidateInput(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( g_Resource );

	ITextInputPtr ipTextInput(pTextInput);
	BSTR bstrText;
	ipTextInput->GetText(&bstrText);

	CString originInput(bstrText);
	CString tempInput(originInput);

	while (true)
	{
		char* pszError;
		// convert the string to long
		long lTemp = strtol(tempInput, &pszError, 10);
		if (pszError[0] != 0)
		{
			InputCorrectionDlg dlg;
			dlg.m_editCorrection = tempInput;
			int res = dlg.DoModal();
			if (res == IDCANCEL)
			{
				IInputEntityPtr ipInputEntity;
				ipTextInput->GetInputEntity(&ipInputEntity);
				if (ipInputEntity)
				{
					// delete the input entity
					ipInputEntity->Delete();
				}
				*pbSuccessful = VARIANT_FALSE;
				break;	
			}

			// if it's OK, validate the input
			tempInput = dlg.m_editCorrection;
		}
		else
		{
			// check whether the input string has been changed
			if (strcmpi(originInput, tempInput) != 0)
			{
				// set the text input
				ipTextInput->SetText(CComBSTR(tempInput));
			}

			*pbSuccessful = VARIANT_TRUE;
			break;
		}
	}
		
	return S_OK;
}

STDMETHODIMP CNumberInputValidator::GetInputType(BSTR * pstrInputType)
{
	return getComponentDescription(pstrInputType);
}
