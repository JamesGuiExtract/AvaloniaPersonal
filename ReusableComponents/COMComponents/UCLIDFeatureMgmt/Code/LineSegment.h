// LineSegment.h : Declaration of the CLineSegment

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CLineSegment
class ATL_NO_VTABLE CLineSegment : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLineSegment, &CLSID_LineSegment>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILineSegment, &IID_ILineSegment, &LIBID_UCLID_FEATUREMGMTLib>,
	public IDispatchImpl<IESSegment, &IID_IESSegment, &LIBID_UCLID_FEATUREMGMTLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLineSegment();
	~CLineSegment();

DECLARE_REGISTRY_RESOURCEID(IDR_LINESEGMENT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLineSegment)
	COM_INTERFACE_ENTRY(ILineSegment)
	COM_INTERFACE_ENTRY2(IDispatch, ILineSegment)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IESSegment)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILineSegment
	STDMETHOD(valueIsEqualTo)(/*[in]*/ ILineSegment *pSegment, /*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(getCoordsFromParams)(/*[in]*/ ICartographicPoint *pStart, /*[out]*/ ICartographicPoint **pEnd);
	STDMETHOD(setParamsFromCoords)(/*[in]*/ ICartographicPoint *pStart, /*[in]*/ ICartographicPoint *pEnd);

// IESSegment
	STDMETHOD(getSegmentType)(ESegmentType * pSegmentType);
	STDMETHOD(setParameters)(/*[in]*/ IIUnknownVector* pvecTypeValuePairs);
	STDMETHOD(getParameters)(/*[out, retval]*/ IIUnknownVector** ppvecTypeValuePairs);
	STDMETHOD(setTangentInDirection)(/*[in]*/ BSTR strTangentInDirection);
	STDMETHOD(getTangentOutDirection)(/*[out, retval]*/ BSTR* pstrTangentOut);
	STDMETHOD(requireTangentInDirection)(/*[out, retval]*/ VARIANT_BOOL* pRequire);
	STDMETHOD(getSegmentLengthString)(BSTR* pstrLength);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);


private:
	////////////
	// Methods
	////////////

	// whatever the input angle value is, it will convert the angle
	// value to be in between 0 ~ 2 pi
	double getAngleWithinFirstPeriodInRadians(double dInAngle);

	// get this line direction as angle in radians and line distance
	// in current distance unit, which is set globally
	void getAngleInRadiansAndDistance(double& dAngle, double& dDistance);

	// create a new type value pair object
	UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr getNewParameterTypeValuePair(
							const ECurveParameterType& eParamType, 
							const std::string& strValue);

	// check license
	void validateLicense();

	////////////
	// Variables
	////////////

	// stores a vector of type value pairs
	// Internally, all direction will be stored as quadrant bearing format
	IIUnknownVectorPtr m_ipParamTypeValuePairs;

	// whether or not this arc is using tangent-in as one
	// of the parameters to define the arc
	bool m_bRequireTangentInDirection;
};
