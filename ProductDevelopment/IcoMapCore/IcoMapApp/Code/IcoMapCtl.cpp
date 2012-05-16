//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapCtl.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Arvind Ganesan (Aug 2001 to present)
//			John Hurd (till July 2001)
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "IcoMapApp.h"
#include "IcoMapCtl.h"

#include "IcoMapDlg.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>

#include "IcoMapOptions.h"

#include <SafeNetLicenseCfg.h>
#include <SafeNetLicenseMgr.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern HINSTANCE gModuleResource;

using namespace std;

//-------------------------------------------------------------------------------------------------
// CIcoMapCtl
//-------------------------------------------------------------------------------------------------
CIcoMapCtl::CIcoMapCtl() :
	m_pIcoMapDlg(NULL),
	m_bDisplayAdapterSet(false),
	m_bAttributeManagerSet(false),
	m_bInputEnabled(false)
{
}
//-------------------------------------------------------------------------------------------------
CIcoMapCtl::~CIcoMapCtl()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		// Since this uses the validateIcoMapLicensed function any concurrent license
		// should be released
		IcoMapOptions::sGetInstance().releaseConcurrentIcoMapLicense();
		destroyIcoMapDlg();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12514");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IIcoMapApplication,
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent,
		&IID_IInputReceiver,
		&IID_IInputTarget
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IIcoMapApplication
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::SetDisplayAdapter(IDisplayAdapter * ipDisplayAdapter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		getIcoMapDlg()->setDisplayAdapter(ipDisplayAdapter);
		m_bDisplayAdapterSet = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03093");


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::SetAttributeManager(IAttributeManager * ipAttributeManager)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		getIcoMapDlg()->setAttributeManager(ipAttributeManager);
		m_bAttributeManagerSet = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03094");


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::SetPoint(DOUBLE dX, DOUBLE dY)
{
	if (!m_pIcoMapDlg)
	{
		return E_UNEXPECTED;
	}

	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		CString cstrPoint("");
		cstrPoint.Format("%f,%f", dX, dY); 
		m_pIcoMapDlg->setPoint((LPCTSTR)cstrPoint);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03095");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::SetText(BSTR text)
{
	if (!m_pIcoMapDlg)
	{
		return E_UNEXPECTED;
	}

	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		CString zText(text);
		string strText = (LPCTSTR)zText;
		m_pIcoMapDlg->setText(strText);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03096");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::ShowIcoMapWindow(VARIANT_BOOL bShow)
{
	if (!m_bDisplayAdapterSet &&
		!m_bAttributeManagerSet)
	{
		return E_UNEXPECTED;
	}

	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (bShow == VARIANT_TRUE)
		{
			try
			{
				validateLicense();
			}
			catch(UCLIDException &ue)
			{
				getIcoMapDlg()->ShowWindow(SW_HIDE);
				throw ue;
			}
			getIcoMapDlg()->ShowWindow(SW_SHOW);
		}
		else
		{
			getIcoMapDlg()->ShowWindow(SW_HIDE);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03097");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::DestroyWindows()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		destroyIcoMapDlg();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03098");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::get_EnableFeatureSelection(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// TODO: this method is just a kludge
		if (m_pIcoMapDlg)
		{
			*pVal = m_pIcoMapDlg->isFeatureSelectionEnabled() ? VARIANT_TRUE : VARIANT_FALSE;
		}
		else
		{
			*pVal = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03099");


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::get_Initialized(VARIANT_BOOL * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		throw UCLIDException("ELI03155", "Initialized() is not implemented in CIcoMapCtl!");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03156")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::EnableFeatureCreation(VARIANT_BOOL bEnable)
{
	// current icomap ctrl is diabled
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		try
		{
			validateLicense();
		}
		catch(...)
		{
			bEnable = VARIANT_FALSE;
		}

		if (m_pIcoMapDlg)
		{
			// enable feature creation if not enabled yet
			m_pIcoMapDlg->enableFeatureCreation(bEnable == VARIANT_TRUE? true : false);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03100");


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::Reset()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_pIcoMapDlg)
		{
			m_pIcoMapDlg->reset();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03101");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::NotifySketchModified(long nActualNumOfSegments)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// notify IcoMapDlg about the modification of num of segments for current sketch
		if (m_pIcoMapDlg)
		{
			m_pIcoMapDlg->notifySketchModified(nActualNumOfSegments);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03102");


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::ProcessKeyDown(long lKeyCode, long lShiftKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_pIcoMapDlg)
		{
			m_pIcoMapDlg->processKeyDown(lKeyCode, lShiftKey);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03103");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::ProcessKeyUp(long lKeyCode, long lShiftKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_pIcoMapDlg)
		{
			m_pIcoMapDlg->processKeyUp(lKeyCode, lShiftKey);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03104");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::OnFeatureSelected(VARIANT_BOOL bReadOnly)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_pIcoMapDlg)
		{
			m_pIcoMapDlg->onFeatureSelected(bReadOnly == VARIANT_TRUE ? true : false);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03105");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::SetIcoMapAsCurrentTool(VARIANT_BOOL bIsCurrent)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		try
		{
			validateLicense();
		}
		catch(...)
		{	
			if ( m_pIcoMapDlg )
			{
				m_pIcoMapDlg->setIcoMapAsCurrentTool(false);
			}
			throw;
		}

		if (m_pIcoMapDlg)
		{
			m_pIcoMapDlg->setIcoMapAsCurrentTool(bIsCurrent == VARIANT_TRUE ? true : false);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03106");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbstrComponentDescription = _bstr_t("IcoMap Command").copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03107");
		
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputReceiver 
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_get_ParentWndHandle(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		throw UCLIDException("ELI03164", "get_ParentWndHandle() is not implemented in CIcoMapCtl!");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03165")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_put_ParentWndHandle(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		throw UCLIDException("ELI03166", "put_ParentWndHandle() is not implemented in CIcoMapCtl!");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03167")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_get_WindowShown(VARIANT_BOOL * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		throw UCLIDException("ELI03157", "WindowShown() is not implemented in CIcoMapCtl!");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03158")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_get_InputIsEnabled(VARIANT_BOOL * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();

		*pVal = m_bInputEnabled ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03108");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_get_HasWindow(VARIANT_BOOL * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();

		// in this perticular case, IcoMap dlg command is the
		// actual input receiver. Since the command belongs to 
		// the dialog, it's pointless to deal with command
		// window seperately.
		*pVal = VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03159");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_get_WindowHandle(LONG * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		throw UCLIDException ("ELI03160", "WindowHandle() is not implemented in CIcoMapCtl!");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03161");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_EnableInput(BSTR strInputType, BSTR strPrompt)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		bool bLicensed = true;
		try
		{
			validateLicense();
		}
		catch(...)
		{
			bLicensed = false;
		}

		m_bInputEnabled = bLicensed;
		// enable icomap command
		getIcoMapDlg()->enableCommandInputReceiver(m_bInputEnabled);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03109");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_DisableInput()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bInputEnabled = false;
		
		// disable icomap command
		getIcoMapDlg()->enableCommandInputReceiver(m_bInputEnabled);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03110");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_SetEventHandler(IIREventHandler * pEventHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// delegate the call to icomap dlg
		getIcoMapDlg()->setIREventHandler(pEventHandler);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03111");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_ShowWindow(VARIANT_BOOL bShow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		throw UCLIDException ("ELI03162", "ShowWindow() is not implemented in CIcoMapCtl!");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03163");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_get_UsesOCR(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		*pVal = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03468")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_SetOCRFilter(IOCRFilter *pFilter)
{
	try
	{
		// Confirm that component is licensed
		validateLicense();

		throw UCLIDException("ELI03463", "The IcoMapCtl does not use OCR technologies!");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03469")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_SetOCREngine(IOCREngine *pEngine)
{
	try
	{
		// Confirm that component is licensed
		validateLicense();

		throw UCLIDException("ELI03596", "The IcoMapCtl does not use OCR technologies!");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03597")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
//	IInputTarget
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_SetApplicationHook(IUnknown * pHook)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_IsVisible(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		if (m_pIcoMapDlg != NULL && ::IsWindow(m_pIcoMapDlg->m_hWnd))
		{
			// Check Traverse Window object
			*pbValue = getIcoMapDlg()->isInputTargetWindowVisible()? VARIANT_TRUE : VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04042")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_Activate()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (m_pIcoMapDlg)
		{
			// move the highlight window on top of icomap dlg
			getIcoMapDlg()->activateInputTarget();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04043")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCtl::raw_Deactivate()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (m_pIcoMapDlg)
		{
			getIcoMapDlg()->deactivateInputTarget();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04044")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// PRIVATE Methods
//-------------------------------------------------------------------------------------------------
IcoMapDlg* CIcoMapCtl::getIcoMapDlg()
{
	try
	{
		// lazy creation of the IcoMapDlg object, if necessary
		if (!m_pIcoMapDlg)
		{
			m_pIcoMapDlg = IcoMapDlg::sGetInstance();
			ASSERT_RESOURCE_ALLOCATION("ELI03847", m_pIcoMapDlg != NULL); 
			m_pIcoMapDlg->createModelessDlg();
			
			// add this as input receiver to the input manager
			m_pIcoMapDlg->connectCommandInputReceiver(this);
			
			// TODO: clean this during the IcoMapCore phase
			// setup this as the SpotRecognitionWindow's event handler so that
			// we can decrement appropriate license counters when paragraph text
			// recognition is performed.
//			m_pIcoMapDlg->setSRWEventHandler(this);

			m_pIcoMapDlg->setInputTarget(this);
			// add this input target to the inputtarget manager
			IInputTargetManagerPtr ipInputTargetMgr(CLSID_InputTargetManager);
			if (ipInputTargetMgr)
			{
				ipInputTargetMgr->AddInputTarget(this);
			}
		}
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI03845")

	return m_pIcoMapDlg;
}
//-------------------------------------------------------------------------------------------------
void CIcoMapCtl::destroyIcoMapDlg(void)
{
	if (m_pIcoMapDlg)
	{
		// before destroying icomap dlg, disconnect from input manager
//		m_pIcoMapDlg->disconnectCommandInputReceiver();

		m_pIcoMapDlg->DestroyWindow();

		delete m_pIcoMapDlg;
		m_pIcoMapDlg = NULL;
	}
}
//-------------------------------------------------------------------------------------------------
void CIcoMapCtl::validateLicense()
{
	try
	{
		IcoMapOptions::sGetInstance().validateIcoMapLicensed();
	}
	catch (...)
	{
		// If this is not done the ctrl works normally except exceptions are displayed saying
		// the component is not licensed
		if ( m_pIcoMapDlg )
		{
			m_pIcoMapDlg->ShowWindow(SW_HIDE);
			m_pIcoMapDlg->enableFeatureCreation(false);
		}
		throw;
	}
}
//-------------------------------------------------------------------------------------------------
