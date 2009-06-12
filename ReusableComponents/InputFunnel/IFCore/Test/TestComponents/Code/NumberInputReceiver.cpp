// NumberInputReceiver.cpp : Implementation of CNumberInputReceiver
#include "stdafx.h"
#include "TestComponents.h"
#include "NumberInputReceiver.h"

#include "NumberInputDlg.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CNumberInputReceiver
CNumberInputReceiver::CNumberInputReceiver()
: m_ipEventHandler(NULL),
  m_pNumberInputDlg(NULL),
  m_bWindowShown(false),
  m_bInputEnabled(false)
{
}

CNumberInputReceiver::~CNumberInputReceiver()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (m_pNumberInputDlg)
	{
		delete m_pNumberInputDlg;
	}
}

// IInputReceiver
STDMETHODIMP CNumberInputReceiver::get_WindowShown(VARIANT_BOOL * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = m_bWindowShown ? VARIANT_TRUE : VARIANT_FALSE;
		
	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::get_InputIsEnabled(VARIANT_BOOL * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = m_bInputEnabled ? VARIANT_TRUE : VARIANT_FALSE;
		
	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::get_HasWindow(VARIANT_BOOL * pVal)
{		
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = VARIANT_TRUE;

	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::get_WindowHandle(LONG * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = long(getNumberInputDlg()->m_hWnd);
	
	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::EnableInput(BSTR strInputType, BSTR strPrompt)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	getNumberInputDlg()->m_ctrlInput.EnableWindow(TRUE);
	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::DisableInput()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	getNumberInputDlg()->m_ctrlInput.EnableWindow(FALSE);
	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::SetEventHandler(IIREventHandler * pEventHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_ipEventHandler = pEventHandler;

	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::ShowWindow(VARIANT_BOOL bShow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	getNumberInputDlg()->ShowWindow(bShow==VARIANT_TRUE ? TRUE : FALSE);
	return S_OK;
}

// IInputEntityManager
STDMETHODIMP CNumberInputReceiver::Delete(BSTR strID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	getNumberInputDlg()->m_ctrlInput.SetWindowText("");

	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::SetText(BSTR strID, BSTR strText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// set the actual text at id of strID
	getNumberInputDlg()->m_ctrlInput.SetWindowText(CString(strText));

	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::GetText(BSTR strID, BSTR * pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// Get actual text at id of strID
	*pstrText = CComBSTR(getNumberInputDlg()->m_editContent);

	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::CanBeMarkedAsUsed(BSTR strID, VARIANT_BOOL * pbCanBeMarkedAsUsed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbCanBeMarkedAsUsed == NULL)
		return E_POINTER;

	*pbCanBeMarkedAsUsed = VARIANT_FALSE;
		
	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::MarkAsUsed(BSTR strID, VARIANT_BOOL bValue)
{
	return E_NOTIMPL;
}

STDMETHODIMP CNumberInputReceiver::IsMarkedAsUsed(BSTR strID, VARIANT_BOOL * pbIsMarkedAsUsed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbIsMarkedAsUsed == NULL)
		return E_POINTER;
		
	*pbIsMarkedAsUsed = VARIANT_FALSE;

	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::IsFromPersistentSource(BSTR strID, VARIANT_BOOL * pbIsFromPersistentSource)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbIsFromPersistentSource == NULL)
		return E_POINTER;
		
	*pbIsFromPersistentSource = VARIANT_FALSE;

	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::GetPersistentSourceName(BSTR strID, BSTR * pstrSourceName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pstrSourceName == NULL)
		return E_POINTER;
	
	return E_NOTIMPL;
}

STDMETHODIMP CNumberInputReceiver::HasBeenOCRed(BSTR strID, VARIANT_BOOL * pbHasBeenOCRed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbHasBeenOCRed == NULL)
		return E_POINTER;
	
	*pbHasBeenOCRed = VARIANT_FALSE;

	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::CreateNewInputReceiver(IInputReceiver *ipInputReceiver)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (m_ipEventHandler)
	{
		m_ipEventHandler->OnNewInputReceiverCreated(ipInputReceiver);
	}

	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::OnInputReceived(BSTR bstrTextInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	ITextInputPtr ipTextInput(__uuidof(TextInput));
	IInputEntityPtr ipInputEntity(__uuidof(InputEntity));
	ipInputEntity->InitInputEntity(this, CComBSTR(""));
	ipTextInput->InitTextInput(ipInputEntity, bstrTextInput);

	if (m_ipEventHandler)
	{
		m_ipEventHandler->OnInputReceived(ipTextInput);
	}

	return S_OK;
}

STDMETHODIMP CNumberInputReceiver::OnAboutToDestroy()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (m_ipEventHandler)
	{
		m_ipEventHandler->OnAboutToDestroy(this);
	}

	return S_OK;
}

NumberInputDlg* CNumberInputReceiver::getNumberInputDlg()
{
	if (!m_pNumberInputDlg)
	{
		m_pNumberInputDlg = new NumberInputDlg;
		
		TESTCOMPONENTSLib::INumberInputReceiver *pThis;

		HRESULT hr = QueryInterface(IID_INumberInputReceiver, (void **) &pThis);
		// Do not increment the ref count when connecting to number input dlg.
		// Because number input receiver owns number input dlg, number input dlg 
		// has a reference to the input receiver in order to make some calls from
		// within input receiver, however, input dlg should not increment the ref
		// count so that when input receiver's ref count gets down to 0, input dlg
		// will also be destroyed accordingly. In other words, input receiver and 
		// input dlg should be treated as one object.
		pThis->Release();
		if (SUCCEEDED(hr))
		{
			// initialize the number input dialog
			m_pNumberInputDlg->SetParentCtrl(pThis);
			m_pNumberInputDlg->CreateModeless();
		}
		else
		{
			// ...
			AfxMessageBox("QI did not work!");
		}
	}

	return m_pNumberInputDlg;
}