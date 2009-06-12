// ObjectSelectorUI.h : Declaration of the CObjectSelectorUI

#pragma once

#include "resource.h"       // main symbols
#include "ObjSelectDlg.h"

#include <string>
#include <memory>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CObjectSelectorUI
class ATL_NO_VTABLE CObjectSelectorUI : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CObjectSelectorUI, &CLSID_ObjectSelectorUI>,
	public ISupportErrorInfo,
	public IDispatchImpl<IObjectSelectorUI, &IID_IObjectSelectorUI, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IPrivateLicensedComponent, &IID_IPrivateLicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CObjectSelectorUI();
	~CObjectSelectorUI();

DECLARE_REGISTRY_RESOURCEID(IDR_OBJECTSELECTORUI)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CObjectSelectorUI)
	COM_INTERFACE_ENTRY(IObjectSelectorUI)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IObjectSelectorUI)
	COM_INTERFACE_ENTRY(IPrivateLicensedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IObjectSelectorUI
	STDMETHOD(ShowUI1)(BSTR strTitleAfterSelect, BSTR strPrompt1, BSTR strPrompt2, 
		BSTR strCategory, IObjectWithDescription *pObj, VARIANT_BOOL vbAllowNone, VARIANT_BOOL *pbOK);
	STDMETHOD(ShowUI2)(BSTR strTitleAfterSelect, BSTR strPrompt1, BSTR strPrompt2, 
		BSTR strCategory, IObjectWithDescription *pObj, VARIANT_BOOL vbAllowNone, 
		long nNumRequiredIIDs, IID pRequiredIIDs[], VARIANT_BOOL *pbOK);
	STDMETHOD(ShowUINoDescription)(BSTR bstrTitleAfterSelect, BSTR btrPrompt, BSTR bstrCategory, 
		IObjectWithDescription *pObj, long nNumRequiredIIDs, IID pRequiredIIDs[], 
		VARIANT_BOOL *pbOK);

// IPrivateLicensedComponent
	STDMETHOD(raw_InitPrivateLicense)(/*[in]*/ BSTR strPrivateLicenseKey);
	STDMETHOD(raw_IsPrivateLicensed)(/*[out, retval]*/ VARIANT_BOOL *pbIsLicensed);

private:
	//////////////////////
	//  Variables
	//////////////////////

	// Dialog title
	std::string	m_strDlgTitle;

	// Dialog prompt for object description
	std::string	m_strDlgPrompt1;

	// Dialog prompt for object selection
	std::string	m_strDlgPrompt2;

	// Category name for items in combo box
	std::string	m_strCategoryName;

	// NOTE: This object can be instantiated only by software products
	// written by UCLID because this object automatically unlocks all
	// privately-licensed UCLID components (such as the OCR engine).
	// A customer who wants to access the OCR engine (SSOCR) could
	// create an instance of this object, allow the user to select
	// the OCR engine (or do that selection programmatically), and then
	// get the reference to an instantiated SSOCR object, which 
	// they can then fully operate.  To prevent our customers or the
	// end users from being able to do that, this object also requires
	// a private license.  The private license ensures that only
	// UCLID's products can instantiate this object.
	bool m_bPrivateLicenseInitialized;

	/////////////
	// Methods
	/////////////

	// Check license
	void validateLicense();

	// Helper function to show ObjSelectDlg given specified parameters from COM call
	void showObjSelectDlg(string strTitleAfterSelect,
		string strPrompt1, string strPrompt2, string strCategory, 
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObj, bool bAllowNone, long nNumRequiredIIDs,
		IID pRequiredIIDs[], CWnd* pParent, bool bConfigureDescription, VARIANT_BOOL *pbOK);
};
