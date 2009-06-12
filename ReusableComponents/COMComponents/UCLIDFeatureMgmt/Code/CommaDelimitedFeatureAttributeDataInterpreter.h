// CommaDelimitedFeatureAttributeDataInterpreter.h : Declaration of the CCommaDelimitedFeatureAttributeDataInterpreter

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CCommaDelimitedFeatureAttributeDataInterpreter
class ATL_NO_VTABLE CCommaDelimitedFeatureAttributeDataInterpreter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCommaDelimitedFeatureAttributeDataInterpreter, &CLSID_CommaDelimitedFeatureAttributeDataInterpreter>,
	public ISupportErrorInfo,
	public IDispatchImpl<IFeatureAttributeDataInterpreter, &IID_IFeatureAttributeDataInterpreter, &LIBID_UCLID_FEATUREMGMTLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CCommaDelimitedFeatureAttributeDataInterpreter();
	~CCommaDelimitedFeatureAttributeDataInterpreter();

DECLARE_REGISTRY_RESOURCEID(IDR_COMMADELIMITEDFEATUREATTRIBUTEDATAINTERPRETER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCommaDelimitedFeatureAttributeDataInterpreter)
	COM_INTERFACE_ENTRY(IFeatureAttributeDataInterpreter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IFeatureAttributeDataInterpreter)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICommaDelimitedFeatureAttributeDataInterpreter
	STDMETHOD(getAttributeDataFromFeature)(/*[in]*/ IUCLDFeature *pFeature, /*[out, retval]*/ BSTR *pstrData);
	STDMETHOD(getFeatureFromAttributeData)(/*[in]*/ BSTR strData, /*[out, retval]*/ IUCLDFeature **pFeature);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(/*[out, retval]*/ BSTR *pbstrComponentName);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);


private:
	//////////
	// methods
	//////////

	// make them into data string
	std::string asString(ICartographicPointPtr& ptrPoint);
	std::string asString(UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr& ptrParam);
	std::string asString(UCLID_FEATUREMGMTLib::IESSegmentPtr& ptrSegment);
	std::string asString(UCLID_FEATUREMGMTLib::IPartPtr& ptrPart);
	std::string asString(UCLID_FEATUREMGMTLib::IUCLDFeaturePtr& ptrFeature);

	// make into arc segment
	UCLID_FEATUREMGMTLib::IESSegmentPtr getArcSegment(
		std::vector<std::string>::const_iterator& iter,
		const std::vector<std::string>& vecMainTokens);
	// make into line segment
	UCLID_FEATUREMGMTLib::IESSegmentPtr getLineSegment(
		std::vector<std::string>::const_iterator& iter,
		const std::vector<std::string>& vecMainTokens);
	// make into part
	UCLID_FEATUREMGMTLib::IPartPtr getPart(
		std::vector<std::string>::const_iterator& iter,
		const std::vector<std::string>& vecMainTokens);
	// make into feature
	UCLID_FEATUREMGMTLib::IUCLDFeaturePtr getFeature(
		std::vector<std::string>::const_iterator& iter,
		const std::vector<std::string>& vecMainTokens);

	// check license
	void validateLicense();

	/////////////
	// Constants
	/////////////
	const char m_cMainSeparator;
	const char m_cSubSeparator;
	const int m_iDoubleToStringPrecision;

	///////////
	// Variables
	////////////
	// what's the version number from the data string?
	int m_nVersionNumber;
};
