#include "stdafx.h"
#include "UCLIDFeatureMgmt.h"
#include "ArcSegment.h"
#include "ParameterTypeValuePair.h"

#include <cpputil.h>
#include <Bearing.hpp>
#include <DistanceCore.h>
#include <Angle.hpp>
#include <StringTokenizer.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ValueRestorer.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CArcSegment
//-------------------------------------------------------------------------------------------------
CArcSegment::CArcSegment()
: m_bRequireTangentInDirection(false),
  m_ipParamTypeValuePairs(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CArcSegment::~CArcSegment()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IArcSegment,
		&IID_IESSegment,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IESSegment
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::setParameters(IIUnknownVector *pvecTypeValuePairs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// store the internal pointer
		m_ipParamTypeValuePairs = pvecTypeValuePairs;

		// this is the place where m_bRequireTangentInDirection will
		// be set properly
		m_bRequireTangentInDirection = false;

//		TODO: The following block of code can be used in the future
//		where each tangent-in curve will always depend on its previous
//		segment's tangent-out.
/*		long nSize = m_ipParamTypeValuePairs->Size();
		for (long n=0; n<nSize; n++)
		{
			UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr 
				ipTypeValuePair(m_ipParamTypeValuePairs->At(n));
			if (ipTypeValuePair->eParamType == kArcTangentInBearing)
			{
				// set the value and return
				m_bRequireTangentInDirection = true;
				// get out of the loop
				break;
			}
		}*/
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01633");

	return S_OK;
} 
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::getParameters(IIUnknownVector **ppvecTypeValuePairs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// ensure that a list of parameters is indeed assocaited with this ArcSegment
		if (m_ipParamTypeValuePairs == NULL)
		{
			throw UCLIDException("ELI01500", "There are no parameters associated with this ArcSegment!");
		}

		// increment reference count & return a reference to our internally 
		// stored parameter-value-type-pair 
		m_ipParamTypeValuePairs.AddRef();
		*ppvecTypeValuePairs = m_ipParamTypeValuePairs;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01634");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::requireTangentInDirection(VARIANT_BOOL *pRequire)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		// whether or not one of the parameters that define this arc segment
		// is tangent-in direction
		*pRequire = m_bRequireTangentInDirection ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12031");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::getTangentOutDirection(BSTR *pstrTangentOut)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// caculate the tangent-out direction, which is always in the
		// format of quadrant bearing
		// recycle CCE
		m_CCE.reset();

		// set each of the parameters in the CCE to their appropriate value.
		long nSize = m_ipParamTypeValuePairs->Size();
		for (long n = 0; n < nSize; n++)
		{
			UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr ptrParamTypeValuePair
							= m_ipParamTypeValuePairs->At(n);

			// get the type of the parameter and the value
			ECurveParameterType eParamType = ptrParamTypeValuePair->eParamType;
			string strValue = _bstr_t(ptrParamTypeValuePair->strValue);

			// add the parameter-type-value-pair to the CCE
			setCCEParameter(&m_CCE, eParamType, strValue);
		}

		// ensure that the arc mid point and the arc-end point are
		// calculable
		if (!m_CCE.canCalculateParameter(kArcTangentOutBearing))
		{
			throw UCLIDException("ELI12033", "Specified parameters are insufficient to calculate Tangent-out direction.");
		}

		// get the tangent out value as polar angle in radians
		double dTout = asDouble(m_CCE.getCurveParameter(kArcTangentOutBearing));
		string strTangentOutBearing("");
		{
			// convert to quadrant bearing format
			// always work in normal mode here
			ReverseModeValueRestorer rmvr;

			AbstractMeasurement::workInReverseMode(false);
			
			Bearing bearing;
			bearing.evaluateRadians(dTout);
			if (!bearing.isValid())
			{
				UCLIDException ue("ELI12332", "Invalid tangent out direction");
				ue.addDebugInfo("Tangent out value", dTout);
				throw ue;
			}

			// get the string format
			strTangentOutBearing = bearing.asString();
		}

		*pstrTangentOut = _bstr_t(strTangentOutBearing.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12032");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::setTangentInDirection(BSTR strTangentInDirection)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// caller of this method shall call requireTangentInDirection()
		// first to make sure this arc requires tangent-in
		// Note: arc is different from line. If there's no tangent-in
		// direction parameter found in the vector, then no need to 
		// add a new pair for the tangent-in
		if (m_bRequireTangentInDirection)
		{
			// find tangent-in in vector
			long nSize = m_ipParamTypeValuePairs->Size();
			for (long n=0; n<nSize; n++)
			{
				UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr 
							ipTypeValuePair(m_ipParamTypeValuePairs->At(n));
				if (ipTypeValuePair->eParamType == kArcTangentInBearing)
				{
					// set the value and return
					ipTypeValuePair->strValue = _bstr_t(strTangentInDirection);
					return S_OK;
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12030");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::getSegmentType(ESegmentType *pSegmentType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		if (pSegmentType == NULL)
		{
			return E_POINTER;
		}
		
		// return kArc as the segment type associated with this CoClass
		*pSegmentType = kArc;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01637");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::getSegmentLengthString(BSTR* pstrLength)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// caculate the tangent-out direction, which is always in the
		// format of quadrant bearing
		// recycle CCE
		m_CCE.reset();

		// set each of the parameters in the CCE to their appropriate value.
		long nSize = m_ipParamTypeValuePairs->Size();
		for (long n = 0; n < nSize; n++)
		{
			UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr ptrParamTypeValuePair
							= m_ipParamTypeValuePairs->At(n);

			// get the type of the parameter and the value
			ECurveParameterType eParamType = ptrParamTypeValuePair->eParamType;
			string strValue = _bstr_t(ptrParamTypeValuePair->strValue);

			// add the parameter-type-value-pair to the CCE
			setCCEParameter(&m_CCE, eParamType, strValue);
		}

		// ensure that the arc mid point and the arc-end point are
		// calculable
		if (!m_CCE.canCalculateParameter(kArcLength))
		{
			throw UCLIDException("ELI12495", "Specified parameters are insufficient to calculate Arc Length.");
		}

		// get arc length in the form of feet
		string strArcLen = m_CCE.getCurveParameter(kArcLength);
		*pstrLength = _bstr_t(strArcLen.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12493");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IArcSegment
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::getCoordsFromParams(ICartographicPoint *pStart, 
											  ICartographicPoint **pMid, 
											  ICartographicPoint **pEnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		ICartographicPointPtr ptrStart(pStart);

		// get the start point
		double dX1, dY1;
		pStart->GetPointInXY(&dX1, &dY1);

		// recycle CCE
		m_CCE.reset();

		// set the starting point in the CCE
		m_CCE.setCurvePointParameter(kArcStartingPoint, dX1, dY1);

		// set each of the parameters in the CCE to their appropriate value.
		long lSize = m_ipParamTypeValuePairs->Size();
		for (int i = 0; i < lSize; i++)
		{
			UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr ptrParamTypeValuePair
							= m_ipParamTypeValuePairs->At(i);

			// get the type of the parameter and the value
			ECurveParameterType eParamType = ptrParamTypeValuePair->eParamType;
			string strValue = _bstr_t(ptrParamTypeValuePair->strValue);

			// add the parameter-type-value-pair to the CCE
			setCCEParameter(&m_CCE, eParamType, strValue);
		}

		// ensure that the arc mid point and the arc-end point are
		// calculable
		if (!m_CCE.canCalculateParameter(kArcMidPoint))
		{
			throw UCLIDException("ELI01589", "Specified parameters are insufficient to calculate Arc mid point coordinate.");
		}
		else if (!m_CCE.canCalculateParameter(kArcEndingPoint))
		{
			throw UCLIDException("ELI01596", "Specified parameters are insufficient to calculate Arc end point coordinate.");
		}

		// calculate the mid point and end point coodinates using the CCE 
		double dMidX = 0.0, dMidY = 0.0, dEndX = 0.0, dEndY = 0.0;
		string strTemp;
		vector<string> vecTokens;
		StringTokenizer st;
		// get the mid point coordinate in the format of xxx.xx,xxx.xx
		strTemp = m_CCE.getCurveParameter(kArcMidPoint);
		// parse the mid point string into two doubles
		st.parse(strTemp, vecTokens);
		if (vecTokens.size() != 2)
		{
			throw UCLIDException("ELI01590", "Internal error!");
		}
		else
		{
			dMidX = asDouble( vecTokens[0] );
			dMidY = asDouble( vecTokens[1] );
		}

		strTemp = m_CCE.getCurveParameter(kArcEndingPoint);
		st.parse(strTemp, vecTokens);
		if (vecTokens.size() != 2)
		{
			throw UCLIDException("ELI01591", "Internal error!");
		}
		else
		{
			dEndX = asDouble( vecTokens[0] );
			dEndY = asDouble( vecTokens[1] );
		}

		// create the mid point and end point coordinate objects and 
		// return to the coordinate objects to the caller
		ICartographicPointPtr ptrMid(CLSID_CartographicPoint);
		ASSERT_RESOURCE_ALLOCATION("ELI01587", ptrMid != NULL);
		ICartographicPointPtr ptrEnd(CLSID_CartographicPoint);
		ASSERT_RESOURCE_ALLOCATION("ELI01588", ptrEnd != NULL);

		ptrMid->InitPointInXY(dMidX, dMidY);
		ptrEnd->InitPointInXY(dEndX, dEndY);
		*pMid = ptrMid.Detach();
		*pEnd = ptrEnd.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01636");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::valueIsEqualTo(IArcSegment *pArc, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();
		
		UCLID_FEATUREMGMTLib::IArcSegmentPtr ipArc(pArc);
		ASSERT_ARGUMENT("ELI12002", ipArc != NULL);

		// unless ALL necessary conditions are met, the values are not
		// considered to be equal.
		*pbValue = VARIANT_FALSE;
		
		// get the parameters of the arc
		UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment(ipArc);
		IIUnknownVectorPtr ipParams = ipSegment->getParameters();
		
		// ensure that the vectors have the same size
		long lExpectedSize = m_ipParamTypeValuePairs->Size();
		if (ipParams->Size() == lExpectedSize)
		{
			for (int i = 0; i < lExpectedSize; i++)
			{
				// get the expected parameter-type-value-pair
				UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr ptrExpected 
								= m_ipParamTypeValuePairs->At(i);

				// get the parameter-type-value-pair to compare to
				UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr ptrActual 
								= ipParams->At(i);

				// compare the objects
				VARIANT_BOOL bSame = ptrActual->valueIsEqualTo(ptrExpected);
				
				// if we are comparing the last item and if bSame is TRUE, then
				// we can return TRUE to the caller
				if (i == lExpectedSize - 1 && bSame == VARIANT_TRUE)
				{
					*pbValue = VARIANT_TRUE;
				}
				else if (bSame == VARIANT_FALSE)
				{
					// we have determined that the current parameter-type-value-pairs
					// are not the same...so we can continue checking and return false.
					break;
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01638");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::setDefaultParamsFromCoords(ICartographicPoint *pStart, 
													 ICartographicPoint *pMid, 
													 ICartographicPoint *pEnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();
		
		// since the default parameters are set
		// based on three points, we shall not
		// set this arc segment as requiring tangent-in direction
		m_bRequireTangentInDirection = false;

		// get the start, mid, and end points
		double dX1, dY1, dX2, dY2, dX3, dY3;
		pStart->GetPointInXY(&dX1, &dY1);
		pMid->GetPointInXY(&dX2, &dY2);
		pEnd->GetPointInXY(&dX3, &dY3);

		// recycle the CCE
		m_CCE.reset();
		
		// set the start, mid, and end points in the engine
		m_CCE.setCurvePointParameter(kArcStartingPoint, dX1, dY1);
		m_CCE.setCurvePointParameter(kArcMidPoint, dX2, dY2);
		m_CCE.setCurvePointParameter(kArcEndingPoint, dX3, dY3);

		// ensure that all required parameters are calculatable
		if (!m_CCE.canCalculateParameter(kArcTangentInBearing) ||
			!m_CCE.canCalculateParameter(kArcRadius) ||
			!m_CCE.canCalculateParameter(kArcDelta) ||
			!m_CCE.canCalculateParameter(kArcConcaveLeft))
		{
			throw UCLIDException("ELI01502", "Internal error - unable to use CCE as expected.");
		}

		// calculate the default parameters for the curve
		// the default parameters to calculate for the curve are", "tangent-in, radius, delta, and 
		// concavity
		string strTanInBearing = m_CCE.getCurveParameter(kArcTangentInBearing);

		string strRadius = m_CCE.getCurveParameter(kArcRadius);
		// the radius shall have distance unit in the string
		CString cstrRadius("");
		static DistanceCore dist;
		string strUnit(dist.getStringFromUnit(dist.getCurrentDistanceUnit()));
		cstrRadius.Format("%s %s", strRadius.c_str(), strUnit.c_str());
		strRadius = (string)cstrRadius;

		string strDelta = m_CCE.getCurveParameter(kArcDelta);
		string strConcaveToTheLeft = m_CCE.getCurveParameter(kArcConcaveLeft);

		// ensure that a variant collection object is associated with
		// this object
		if (m_ipParamTypeValuePairs == NULL)
		{
			// create a new variant collection object and associate it with this object
			// create the end point object
			m_ipParamTypeValuePairs.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI01503", m_ipParamTypeValuePairs != NULL);
		}

		// clear the vector
		m_ipParamTypeValuePairs->Clear();

		// add the various parameters to the variant collection
		m_ipParamTypeValuePairs->PushBack(
			getNewParameterTypeValuePair(kArcTangentInBearing, strTanInBearing));
		m_ipParamTypeValuePairs->PushBack(
			getNewParameterTypeValuePair(kArcRadius, strRadius));
		m_ipParamTypeValuePairs->PushBack(
			getNewParameterTypeValuePair(kArcDelta, strDelta));
		m_ipParamTypeValuePairs->PushBack(
			getNewParameterTypeValuePair(kArcConcaveLeft, strConcaveToTheLeft));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01635");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcSegment::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr CArcSegment::getNewParameterTypeValuePair(
																const ECurveParameterType& eParamType, 
																const string& strValue)
{
	UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr 
		ipParameterTypeValuePair(CLSID_ParameterTypeValuePair);
	ASSERT_RESOURCE_ALLOCATION("ELI01504", ipParameterTypeValuePair != NULL);

	// populate the parameter type value pair object with the given data
	ipParameterTypeValuePair->eParamType = eParamType;
	ipParameterTypeValuePair->strValue = _bstr_t(strValue.c_str());

	return ipParameterTypeValuePair;
}
//-------------------------------------------------------------------------------------------------
void CArcSegment::setCCEParameter(CurveCalculationEngineImpl *pCCE, 
								  ECurveParameterType eParamType,
								  const string& strValue)
{
	// depending upon what the type of the parameter is, parse the
	// value accordingly
	switch (eParamType)
	{
	// distance parameter types
	case kArcRadius:
	case kArcLength:
	case kArcChordLength:
	case kArcExternalDistance:
	case kArcMiddleOrdinate:
	case kArcTangentDistance:
	{
		// use the distance class to parse the value
		DistanceCore dist;
		dist.evaluate(strValue);
		if (dist.isValid())
		{
			// distance value must be presented in current unit
			double dDist = dist.getDistanceInCurrentUnit();
			pCCE->setCurveDistanceParameter(eParamType, dDist);
		}
		else
		{
			UCLIDException ue("ELI01511", "Distance parameter value specified in invalid format!");
			ue.addDebugInfo( "Input distance", strValue );
			throw ue;
		}
		break;
	}

	// bearing parameter types
	case kArcTangentInBearing:
	case kArcTangentOutBearing:
	case kArcChordBearing:
	case kArcRadialInBearing:
	case kArcRadialOutBearing:
	{
		// store the original mode, then set it back once 
		// this method is out of scope
		ReverseModeValueRestorer rmvr;
		// always work in normal mode here
		AbstractMeasurement::workInReverseMode(false);

		Bearing bearing;
		bearing.evaluate(strValue.c_str());
		if (bearing.isValid())
		{
			double dRadians = bearing.getRadians();
			pCCE->setCurveAngleOrBearingParameter(eParamType, dRadians);
		}
		else
		{
			UCLIDException ue("ELI01510", "Bearing parameter value specified in invalid format!");
			ue.addDebugInfo("Bearing", strValue);
			throw ue;
		}
		break;
	}
	
	// angle parameter types
	case kArcDegreeOfCurveChordDef:
	case kArcDegreeOfCurveArcDef:
	case kArcDelta:
	case kArcStartAngle:
	case kArcEndAngle:
	{
		Angle angle;
		angle.evaluate(strValue.c_str());
		if (angle.isValid())
		{
			double dRadians = angle.getRadians();
			pCCE->setCurveAngleOrBearingParameter(eParamType, dRadians);
		}
		else
		{
			UCLIDException ue("ELI01509", "Angle parameter value specified in invalid format!");
			ue.addDebugInfo( "Input angle", strValue );
			throw ue;
		}
		break;
	}
	
	// point parameter types
	case kArcStartingPoint:
	case kArcMidPoint:
	case kArcEndingPoint:
	case kArcCenter:
	case kArcExternalPoint:
	case kArcChordMidPoint:
	{
		vector<string> vecTokens;
		StringTokenizer st;
		st.parse(strValue, vecTokens);
		if (vecTokens.size() == 2)
		{
			double dX = asDouble( vecTokens[0] );
			double dY = asDouble( vecTokens[1] );
			pCCE->setCurvePointParameter(eParamType, dX, dY);
		}
		else
		{
			UCLIDException ue("ELI01507", "Point parameter value specified in invalid format!");
			ue.addDebugInfo( "Input point", strValue );
			throw ue;
		}
		break;
	}
	
	// boolean parameter types
	case kArcConcaveLeft:
	case kArcDeltaGreaterThan180Degrees:
	{
		if (strValue == "0")
		{
			pCCE->setCurveBooleanParameter(eParamType, false);
		}
		else if (strValue == "1")
		{
			pCCE->setCurveBooleanParameter(eParamType, true);
		}
		else
		{
			UCLIDException ue("ELI01508", "Boolean parameter value specified in invalid format!");
			ue.addDebugInfo( "Input parameter", strValue );
			throw ue;
		}
		break;
	}

	// unknown parameter types
	default:
		UCLIDException ue("ELI01506", "Unexpected parameter stored in parameters collection!");
		ue.addDebugInfo( "Input parameter type", eParamType );
		ue.addDebugInfo( "Input parameter", strValue );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CArcSegment::validateLicense()
{
	static const unsigned long ARC_SEGMENT_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE(ARC_SEGMENT_COMPONENT_ID, "ELI02608", "Arc Segment");
}
//-------------------------------------------------------------------------------------------------
