// LineSegment.cpp ", "Implementation of CLineSegment
#include "stdafx.h"
#include "UCLIDFeatureMgmt.h"
#include "LineSegment.h"

#include <DistanceCore.h>
#include <Bearing.hpp>
#include <Angle.hpp>
#include <TPPoint.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <mathUtil.h>
#include <ValueRestorer.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CLineSegment
//-------------------------------------------------------------------------------------------------
CLineSegment::CLineSegment()
: m_bRequireTangentInDirection(false),
  m_ipParamTypeValuePairs(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CLineSegment::~CLineSegment()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILineSegment,
		&IID_IESSegment,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IESSegment
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::getParameters(IIUnknownVector **ppvecTypeValuePairs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// ensure that a list of parameters is indeed assocaited with this ArcSegment
		if (m_ipParamTypeValuePairs == NULL)
		{
			throw UCLIDException("ELI12017", "There are no parameters associated with this ArcSegment!");
		}

		m_ipParamTypeValuePairs.AddRef();
		*ppvecTypeValuePairs = m_ipParamTypeValuePairs;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12012");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::setParameters(IIUnknownVector *pvecTypeValuePairs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipParamTypeValuePairs = pvecTypeValuePairs;

		// this is the place where m_bRequireTangentInDirection will
		// be set properly
		m_bRequireTangentInDirection = false;
		long nSize = m_ipParamTypeValuePairs->Size();
		for (long n=0; n<nSize; n++)
		{
			UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr 
				ipTypeValuePair(m_ipParamTypeValuePairs->At(n));
			ECurveParameterType eParamType = ipTypeValuePair->eParamType;
			if (eParamType == kLineDeflectionAngle 
				|| eParamType == kLineInternalAngle)
			{
				// set the value and return
				m_bRequireTangentInDirection = true;
				// get out of the loop
				break;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12013");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::requireTangentInDirection(VARIANT_BOOL *pRequire)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// whether or not this line requires tangent in direction,
		// for instance, deflection/internal angle
		*pRequire = m_bRequireTangentInDirection ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12014");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::getTangentOutDirection(BSTR *pstrTangentOut)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// acutally, it's the line direction, which is always 
		// in the format of quadrant bearing
		double dAngle = 0.0, dDistance = 0.0;
		getAngleInRadiansAndDistance(dAngle, dDistance);

		// store the original mode, then set it back once 
		// this method is out of scope
		ReverseModeValueRestorer rmvr;
		// always work in normal mode here
		AbstractMeasurement::workInReverseMode(false);
		
		// convert the angle into quadrant bearing format
		Bearing tempBearing;
		tempBearing.evaluateRadians(dAngle);
		string strBearing = tempBearing.asString();

		*pstrTangentOut = _bstr_t(strBearing.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12015");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::setTangentInDirection(BSTR strTangentInDirection)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// caller of this method shall call requireTangentInDirection()
		// first to make sure this line requires tangent-in
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

			// if this point is reached, then there's no 
			// tangent-in entry in the vector. Create a one
			UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr 
					ipNewTypeValuePair(CLSID_ParameterTypeValuePair);
			ASSERT_RESOURCE_ALLOCATION("ELI12029", ipNewTypeValuePair != NULL);
			ipNewTypeValuePair->eParamType = kArcTangentInBearing;
			ipNewTypeValuePair->strValue = _bstr_t(strTangentInDirection);
			m_ipParamTypeValuePairs->PushBack(ipNewTypeValuePair);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12016");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::getSegmentType(ESegmentType * pSegmentType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (pSegmentType == NULL)
			return E_POINTER;
		
		// return "kLine" as the segment type associated with this object.
		*pSegmentType = kLine;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01648");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::getSegmentLengthString(BSTR* pstrLength)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		long nSize = m_ipParamTypeValuePairs->Size();
		for (long n=0; n<nSize; n++)
		{
			UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr ipParamTypeValuePair
							= m_ipParamTypeValuePairs->At(n);
			if (ipParamTypeValuePair->eParamType == kLineDistance)
			{
				// distance is always stored in the form of feet
				*pstrLength = ipParamTypeValuePair->strValue;
				break;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12494");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILineSegment
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::setParamsFromCoords(ICartographicPoint *pStart, ICartographicPoint *pEnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		m_bRequireTangentInDirection = false;

		ICartographicPointPtr ptrStart(pStart);
		ICartographicPointPtr ptrEnd(pEnd);

		// get the starting and ending point coords
		double dX1, dY1;
		ptrStart->GetPointInXY(&dX1, &dY1);
		double dX2, dY2;
		ptrEnd->GetPointInXY(&dX2, &dY2);

		// compute the distance from the coordinates
		TPPoint p1(dX1, dY1), p2(dX2, dY2);
		// get the distance in between these two points
		// The distance value shall be in current distance unit
		double dDistanceInBetween = p1.distanceTo(p2);
		static DistanceCore distanceCore;
		CString cstrDistance("");
		// get current unit for distance, shall not be unknown unit
		string strUnit(distanceCore.getStringFromUnit(distanceCore.getCurrentDistanceUnit()));
		cstrDistance.Format("%.15f %s", dDistanceInBetween, strUnit.c_str());

		string strDistance = (LPCTSTR)cstrDistance;

		// get the bearing
		string strBearing("");
		{
			// store the original mode, then set it back once 
			// this method is out of scope
			ReverseModeValueRestorer rmvr;
			// always work in normal mode here
			AbstractMeasurement::workInReverseMode(false);
			
			Bearing tempBearing;
			tempBearing.evaluate(p1, p2);
			strBearing = tempBearing.asString();
		}

		if (m_ipParamTypeValuePairs == NULL)
		{
			// create a new variant collection object and associate it with this object
			// create the end point object
			m_ipParamTypeValuePairs.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI12018", m_ipParamTypeValuePairs != NULL);
		}

		// clear the vector
		m_ipParamTypeValuePairs->Clear();

		// add the various parameters to the vector
		m_ipParamTypeValuePairs->PushBack(
			getNewParameterTypeValuePair(kLineBearing, strBearing));
		m_ipParamTypeValuePairs->PushBack(
			getNewParameterTypeValuePair(kLineDistance, strDistance));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01646");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::getCoordsFromParams(ICartographicPoint *pStart, 
											   ICartographicPoint **pEnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();
		
		ICartographicPointPtr ptrStart(pStart);
		
		double dAngle = 0.0, dDistance = 0.0;
		// get line angle and distance
		getAngleInRadiansAndDistance(dAngle, dDistance);

		// get the start point coords
		double dX1, dY1;
		ptrStart->GetPointInXY(&dX1, &dY1);

		// calculate the end point based upon the start point,
		// the bearing, and the distance
		double dX2, dY2;
		dX2 = dX1 + dDistance * cos(dAngle);
		dY2 = dY1 + dDistance * sin(dAngle);

		// create the end point object
		ICartographicPointPtr ptrEnd(CLSID_CartographicPoint);
		ASSERT_RESOURCE_ALLOCATION("ELI01497", ptrEnd != NULL);

		// populate the end point object and return to caller
		ptrEnd->InitPointInXY(dX2, dY2);
		
		// detach the object from the smart pointer and return it
		*pEnd = ptrEnd.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01647");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::valueIsEqualTo(ILineSegment *pSegment, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		UCLID_FEATUREMGMTLib::ILineSegmentPtr ipLineSegment(pSegment);

		// unless ALL necessary conditions are met, the values are not
		// considered to be equal.
		*pbValue = VARIANT_FALSE;
		
		// get the parameters of the line segment
		UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment(ipLineSegment);
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01649");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLineSegment::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helper function
//-------------------------------------------------------------------------------------------------
double CLineSegment::getAngleWithinFirstPeriodInRadians(double dInAngle)
{
	double dRet = dInAngle;
	// make sure the angle is in between 0 and 2*PI
	double dMult = floor( dRet / (MathVars::PI * 2) );
	dRet = dRet - dMult * (MathVars::PI * 2);
	
	return dRet;
}
//-------------------------------------------------------------------------------------------------
void CLineSegment::getAngleInRadiansAndDistance(double& dAngle, double& dDistance)
{
	// store the original mode, then set it back once 
	// this method is out of scope
	ReverseModeValueRestorer rmvr;
	// always work in normal mode here
	AbstractMeasurement::workInReverseMode(false);

	// look up the type value pair
	if (m_ipParamTypeValuePairs == NULL)
	{
		throw UCLIDException("ELI12023", "This line segment's parameters are not set yet.");
	}

	// if the line is formed by deflection angle or internal angle
	bool bIsDeflectionAngle = true;
	double dDeflectionAngle = -1.0;
	double dInternalAngle = -1.0;
	double dTangentInAngle = -1.0;
	// whether or not the angle direction (i.e. to the left
	// or right of the previous line) is set
	bool bDeflectionInternalDirectionSet = false;
	// if this line is formed by deflection/internal angle,
	// we need to know whether the angle is to the left or
	// right of the line
	bool bToTheLeft = true;

	// before getting any data, set them all to negative
	dAngle = -1.0;
	dDistance = -1.0;

	Bearing tempBearing;
	Angle tempAngle;
	long nSize = m_ipParamTypeValuePairs->Size();
	for (long n=0; n<nSize; n++)
	{
		UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr 
					ipTypeValuePair(m_ipParamTypeValuePairs->At(n));
		ECurveParameterType eType = ipTypeValuePair->eParamType;
		string strValue = _bstr_t(ipTypeValuePair->strValue);
		switch (eType)
		{
		case kLineDeflectionAngle:
			{
				tempAngle.resetVariables();
				tempAngle.evaluate(strValue.c_str());
				if (!tempAngle.isValid())
				{
					UCLIDException ue("ELI12034", "Invalid deflection angle value.");
					ue.addDebugInfo("Deflection Angle", strValue);
					throw ue;
				}

				dDeflectionAngle = 
					getAngleWithinFirstPeriodInRadians(tempAngle.getRadians());
				bIsDeflectionAngle = true;
			}
			break;
		case kLineInternalAngle:
			{
				tempAngle.resetVariables();
				tempAngle.evaluate(strValue.c_str());
				if (!tempAngle.isValid())
				{
					UCLIDException ue("ELI12035", "Invalid internal angle value.");
					ue.addDebugInfo("Internal Angle", strValue);
					throw ue;
				}

				dInternalAngle = 
					getAngleWithinFirstPeriodInRadians(tempAngle.getRadians());
				bIsDeflectionAngle = false;
			}
			break;
		case kLineBearing:
			{
				tempBearing.resetVariables();
				tempBearing.evaluate(strValue.c_str());
				if (!tempBearing.isValid())
				{
					UCLIDException ue("ELI12024", "Invalid line bearing!");
					ue.addDebugInfo("Bearing", strValue);
					throw ue;
				}

				// get the bearing in radians
				dAngle = getAngleWithinFirstPeriodInRadians(tempBearing.getRadians());
			}
			break;
		case kArcConcaveLeft:
			{
				bDeflectionInternalDirectionSet = true;
				bToTheLeft = strValue == "1";
			}
			break;
		case kArcTangentInBearing:
			{
				tempBearing.resetVariables();
				tempBearing.evaluate(strValue.c_str());
				if (!tempBearing.isValid())
				{
					UCLIDException ue("ELI12028", "Invalid tangent-in bearing!");
					ue.addDebugInfo("Bearing", strValue);
					throw ue;
				}

				// get the bearing in radians
				dTangentInAngle = 
					getAngleWithinFirstPeriodInRadians(tempBearing.getRadians());
			}
			break;
		case kLineDistance:
			{
				DistanceCore dist;
				dist.evaluate(strValue);
				if (!dist.isValid())
				{
					UCLIDException uclidException("ELI01499", "Invalid distance!");
					uclidException.addDebugInfo("Distance string", strValue);
					throw uclidException;
				}

				// get distance in current unit, which is set globally
				dDistance = dist.getDistanceInCurrentUnit();
			}
			break;
		default:
			// any other type will be ignored
			continue;
		}
	}

	if (m_bRequireTangentInDirection)
	{
		// make sure all data are obtained
		if ((dDeflectionAngle < 0 && dInternalAngle < 0) 
			|| dTangentInAngle < 0 || dDistance < 0 
			|| !bDeflectionInternalDirectionSet)
		{
			throw UCLIDException("ELI12026", "Insuffcient data to calculate a line segment.");
		}

		// if this line is formed by deflection/internal angle
		// the actual angle will depend on current angle interpretation
		double dDefIntAngle = 
			bIsDeflectionAngle ? dDeflectionAngle : (MathVars::PI - dInternalAngle);
		double dInAngle = bToTheLeft ? 
			(dTangentInAngle + dDefIntAngle) : (dTangentInAngle - dDefIntAngle);
		dAngle = getAngleWithinFirstPeriodInRadians(dInAngle);
	}

	if (dAngle < 0 || dDistance < 0)
	{
		throw UCLIDException("ELI12027", "Insuffcient data to calculate a line segment.");
	}
}
//-------------------------------------------------------------------------------------------------
UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr CLineSegment::getNewParameterTypeValuePair(
																const ECurveParameterType& eParamType, 
																const string& strValue)
{
	UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr 
		ipParameterTypeValuePair(CLSID_ParameterTypeValuePair);
	ASSERT_RESOURCE_ALLOCATION("ELI12019", ipParameterTypeValuePair != NULL);

	// populate the parameter type value pair object with the given data
	ipParameterTypeValuePair->eParamType = eParamType;
	ipParameterTypeValuePair->strValue = _bstr_t(strValue.c_str());

	return ipParameterTypeValuePair;
}
//-------------------------------------------------------------------------------------------------
void CLineSegment::validateLicense()
{
	static const unsigned long LINE_SEGMENT_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( LINE_SEGMENT_COMPONENT_ID, "ELI02614", "Line Segment" );
}
//-------------------------------------------------------------------------------------------------
