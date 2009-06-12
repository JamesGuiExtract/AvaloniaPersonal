// ArcSegment.h : Declaration of the CArcSegment

#pragma once

#include "resource.h"       // main symbols

#include <CurveCalculationEngineImpl.h>

#include <string>


/////////////////////////////////////////////////////////////////////////////
// CArcSegment
class ATL_NO_VTABLE CArcSegment : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CArcSegment, &CLSID_ArcSegment>,
	public IDispatchImpl<IArcSegment, &IID_IArcSegment, &LIBID_UCLID_FEATUREMGMTLib>,
	public ISupportErrorInfo,
	public IDispatchImpl<IESSegment, &IID_IESSegment, &LIBID_UCLID_FEATUREMGMTLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CArcSegment();
	~CArcSegment();

DECLARE_REGISTRY_RESOURCEID(IDR_ARCSEGMENT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CArcSegment)
	COM_INTERFACE_ENTRY(IArcSegment)
	COM_INTERFACE_ENTRY2(IDispatch, IArcSegment)
	COM_INTERFACE_ENTRY(IESSegment)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IArcSegment
	STDMETHOD(valueIsEqualTo)(/*[in]*/ IArcSegment *pArc, /*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(getCoordsFromParams)(/*[in]*/ ICartographicPoint *pStart, /*[out]*/ ICartographicPoint **pMid, /*[out]*/ ICartographicPoint **pEnd);
	STDMETHOD(setDefaultParamsFromCoords)(/*[in]*/ ICartographicPoint *pStart, /*[in]*/ ICartographicPoint *pMid, /*[in]*/ ICartographicPoint *pEnd);

// IESSegment
	STDMETHOD(getSegmentType)(ESegmentType * pSegmentType);
	STDMETHOD(setTangentInDirection)(/*[in]*/ BSTR strTangentInDirection);
	STDMETHOD(getTangentOutDirection)(/*[out, retval]*/ BSTR* pstrTangentOut);
	STDMETHOD(requireTangentInDirection)(/*[out, retval]*/ VARIANT_BOOL* pRequire);
	STDMETHOD(setParameters)(/*[in]*/ IIUnknownVector* pvecTypeValuePairs);
	STDMETHOD(getParameters)(/*[out, retval]*/ IIUnknownVector** ppvecTypeValuePairs);
	STDMETHOD(getSegmentLengthString)(BSTR* pstrLength);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// check license
	void validateLicense();

	void setCCEParameter(CurveCalculationEngineImpl *pCCE, 
						 ECurveParameterType eParamType,
						 const std::string& strValue);
	
	UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr getNewParameterTypeValuePair(
							const ECurveParameterType& eParamType, 
							const std::string& strValue);

	////////////
	// Variables
	////////////
	// stores a vector of type value pairs
	// Internally, all direction will be stored as quadrant bearing format
	IIUnknownVectorPtr m_ipParamTypeValuePairs;

	// whether or not this arc is using tangent-in as one
	// of the parameters to define the arc
	bool m_bRequireTangentInDirection;

	// calculation engine
	CurveCalculationEngineImpl m_CCE;
};

