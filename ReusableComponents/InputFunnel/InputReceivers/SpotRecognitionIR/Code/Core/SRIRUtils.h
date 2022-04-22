// SRIRUtils.h : Declaration of the CSRIRUtils

#pragma once

#include "resource.h"       // main symbols
#include <IConfigurationSettingsPersistenceMgr.h>

/////////////////////////////////////////////////////////////////////////////
// CSRIRUtils
class ATL_NO_VTABLE CSRIRUtils : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSRIRUtils, &CLSID_SRIRUtils>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISRIRUtils, &IID_ISRIRUtils, &LIBID_UCLID_SPOTRECOGNITIONIRLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CSRIRUtils();
	~CSRIRUtils();

DECLARE_REGISTRY_RESOURCEID(IDR_SRIRUTILS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSRIRUtils)
	COM_INTERFACE_ENTRY(ISRIRUtils)
	COM_INTERFACE_ENTRY2(IDispatch,ISRIRUtils)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ISRIRUtils
	STDMETHOD(GetSRIRWithImage)(
		/*[in]*/ BSTR strImageFileName, 
		/*[in]*/ IInputManager* pInputManager, 
		/*[in]*/ VARIANT_BOOL bAutoCreate, 
		/*[out, retval]*/ ISpotRecognitionWindow** ppSRIR);
	STDMETHOD(IsExactlyOneImageOpen)(
		/*[in]*/ IInputManager* pInputMgr, 
		/*[in, out]*/ VARIANT_BOOL* pbExactOneFileOpen,
		/*[in, out]*/ BSTR* pstrCurrentOpenFileName,
		/*[in, out]*/ ISpotRecognitionWindow **ppSRIR);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:

	std::unique_ptr<IConfigurationSettingsPersistenceMgr> m_apCfgMgr;

	///////////
	// Methods
	///////////
	void validateLicense();

	// return true if the registry setting is configured to reuse the window
	bool reuseWindowForNewFile() const;
};

