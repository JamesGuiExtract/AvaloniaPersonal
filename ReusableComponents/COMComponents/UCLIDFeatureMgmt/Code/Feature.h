// Feature.h : Declaration of the CFeature

#pragma once

#include "resource.h"       // main symbols

#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CFeature
class ATL_NO_VTABLE CFeature : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFeature, &CLSID_Feature>,
	public ISupportErrorInfo,
	public IDispatchImpl<IUCLDFeature, &IID_IUCLDFeature, &LIBID_UCLID_FEATUREMGMTLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
	
public:
	CFeature();
	~CFeature();

DECLARE_REGISTRY_RESOURCEID(IDR_FEATURE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CFeature)
	COM_INTERFACE_ENTRY(IUCLDFeature)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IUCLDFeature)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IUCLDFeature
public:
	STDMETHOD(valueIsEqualTo)(/*[in]*/ IUCLDFeature *pFeature, /*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(getFeatureType)(/*[out, retval]*/ EFeatureType *pFeatureType);
	STDMETHOD(setFeatureType)(/*[in]*/ EFeatureType eFeatureType);
	STDMETHOD(addPart)(/*[in]*/ IPart *pPart);
	STDMETHOD(getNumParts)(/*[out, retval]*/ long *plNumParts);
	STDMETHOD(getParts)(/*[out, retval]*/ IEnumPart **pEnumPart);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// check license
	void validateLicense();

	/////////////
	// Variables
	/////////////
	EFeatureType m_eFeatureType;
	std::vector<UCLID_FEATUREMGMTLib::IPartPtr> m_vecParts;
};

