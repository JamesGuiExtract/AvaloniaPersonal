// CommaDelimitedFeatureAttributeDataInterpreter.cpp ", "Implementation of CCommaDelimitedFeatureAttributeDataInterpreter
#include "stdafx.h"
#include "UCLIDFeatureMgmt.h"
#include "CommaDelimitedFeatureAttributeDataInterpreter.h"

#include <strstream>
#include <stdio.h>
#include <StringTokenizer.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

using namespace std;

// current version string to be added to the beginning of the data string
// It will be a pair of strings separated by sub separator
// for instance, Version:2
const static string VERSION_STRING = "Version";
const static int CURRENT_VERSION_NUMBER = 2;

//-------------------------------------------------------------------------------------------------
// CCommaDelimitedFeatureAttributeDataInterpreter
//-------------------------------------------------------------------------------------------------
CCommaDelimitedFeatureAttributeDataInterpreter::CCommaDelimitedFeatureAttributeDataInterpreter()
:m_cMainSeparator('|'),
 m_cSubSeparator (':'),
 m_iDoubleToStringPrecision(10),
 m_nVersionNumber(1)
{
}
//-------------------------------------------------------------------------------------------------
CCommaDelimitedFeatureAttributeDataInterpreter::~CCommaDelimitedFeatureAttributeDataInterpreter()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCommaDelimitedFeatureAttributeDataInterpreter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFeatureAttributeDataInterpreter,
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent
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
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCommaDelimitedFeatureAttributeDataInterpreter::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		*pbstrComponentDescription = _bstr_t("Store text delimited with pipe & colon characters").copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02617");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCommaDelimitedFeatureAttributeDataInterpreter::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICommaDelimitedFeatureAttributeDataInterpreter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCommaDelimitedFeatureAttributeDataInterpreter::getAttributeDataFromFeature(
																IUCLDFeature *pFeature, 
																BSTR *pstrData)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		UCLID_FEATUREMGMTLib::IUCLDFeaturePtr ptrFeature(pFeature);
		ASSERT_RESOURCE_ALLOCATION("ELI12036", ptrFeature != NULL);

		// return the stringized feature as a BSTR to the caller
		string strData = asString(ptrFeature);
		*pstrData = _bstr_t(strData.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01632");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCommaDelimitedFeatureAttributeDataInterpreter::getFeatureFromAttributeData(
																BSTR strData, 
																IUCLDFeature **pFeature)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// parse the input into tokens
		string strInput = _bstr_t(strData);
		StringTokenizer stMain(m_cMainSeparator);
		

		// get the tokens as a vector of strings
		vector<string> vecMainTokens;
		stMain.parse(strInput, vecMainTokens);


		// create an iterator to walk through them
		vector<string>::const_iterator iter = vecMainTokens.begin();

		UCLID_FEATUREMGMTLib::IUCLDFeaturePtr ipFeature
			= getFeature(iter, vecMainTokens);

		// return the feature object to the caller
		*pFeature = (IUCLDFeature *)ipFeature.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01631");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
string CCommaDelimitedFeatureAttributeDataInterpreter::asString(
														ICartographicPointPtr& ptrPoint)
{
	// get the coordinates
	double dX, dY;
	ptrPoint->GetPointInXY(&dX, &dY);

	// create a string with the two doubles seprated by a sub separator
	strstream data;
	data.precision(m_iDoubleToStringPrecision);
	data << dX << m_cSubSeparator << dY;
	
	// return the result string to the caller
	string strResult;
	strResult.assign(data.str(), data.pcount());
	free(data.str());
	return strResult;
}
//-------------------------------------------------------------------------------------------------
string CCommaDelimitedFeatureAttributeDataInterpreter::asString(
							UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr& ptrParam)
{
	// get the parameter type and the value
	ECurveParameterType eParamType = ptrParam->eParamType;
	string strValue = ptrParam->strValue;

	strstream data;
	// write parameter type and value, separated by sub separator
	data << (long) eParamType << m_cSubSeparator;
	data << strValue;

	// return the data stream as a string
	string strResult;
	strResult.assign(data.str(), data.pcount());
	free(data.str());
	return strResult;
}
//-------------------------------------------------------------------------------------------------
string CCommaDelimitedFeatureAttributeDataInterpreter::asString(
									UCLID_FEATUREMGMTLib::IESSegmentPtr& ipSegment)
{
	strstream data;

	// write the segment type
	UCLID_FEATUREMGMTLib::ESegmentType eSegmentType = ipSegment->getSegmentType();
	data << (long) eSegmentType << m_cMainSeparator;

	if (eSegmentType == kLine || eSegmentType == kArc)
	{
		// get number of parameters
		IIUnknownVectorPtr ipParams = ipSegment->getParameters();
		
		// write the number of parameters to the data stream
		long lNumParams = ipParams->Size();
		data << lNumParams;
		
		if (lNumParams > 0)
		{
			// write a seperator to seperate the number of parameter-type-value-pairs
			// from the first parameter-type-value-pair
			data << m_cMainSeparator;
			
			// write out each parameter-type-value-pair
			for (int i = 0; i < lNumParams; i++)
			{
				// get the ParameterTypevaluePair object
				UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr ptrParam 
					= ipParams->At(i);
				
				// write out the parameter-type-value-pair object
				data << asString(ptrParam);
				
				// if there are more params to write out, then write
				// out a seperator char to seperate the params
				if (i < lNumParams - 1)
				{
					data << m_cMainSeparator;
				}
			}
		}
	}
	else
	{
		UCLIDException ue("ELI01598", "Invalid segment type!");
		ue.addDebugInfo("SegmentType", (int)eSegmentType);
		throw ue;
	}

	// return the result data stream
	string strResult;
	strResult.assign(data.str(), data.pcount());
	free(data.str());
	return strResult;
}
//-------------------------------------------------------------------------------------------------
string CCommaDelimitedFeatureAttributeDataInterpreter::asString(
										UCLID_FEATUREMGMTLib::IPartPtr& ptrPart)
{
	// get the starting point of the part
	ICartographicPointPtr ipStartPoint = ptrPart->getStartingPoint();

	// write the starting point
	strstream data;
	data << asString(ipStartPoint) << m_cMainSeparator;

	// write the number of segments
	long lNumSegments = ptrPart->getNumSegments();
	data << lNumSegments;

	if (lNumSegments > 0)
	{
		// write a seperator to seperate the number of segments
		// from the first segment
		data << m_cMainSeparator;

		// get the enumsegment object
		UCLID_FEATUREMGMTLib::IEnumSegmentPtr ipEnumSegment = ptrPart->getSegments();

		// write out each segment
		for (int iSegmentNumber = 0; iSegmentNumber < lNumSegments; iSegmentNumber++)
		{
			// get the segment object
			UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment = ipEnumSegment->next();

			data << asString(ipSegment);

			// if there are more segments to write out, then write
			// out a seperator char to seperate the segments
			if (iSegmentNumber < lNumSegments - 1)
			{
				data << m_cMainSeparator;
			}
		}
	}

	// return the result data stream
	string strResult;
	strResult.assign(data.str(), data.pcount());
	free(data.str());
	return strResult;
}
//-------------------------------------------------------------------------------------------------
string CCommaDelimitedFeatureAttributeDataInterpreter::asString(
										UCLID_FEATUREMGMTLib::IUCLDFeaturePtr& ptrFeature)
{
	strstream data;

	// write the version number at the very beginning
	data << VERSION_STRING << m_cSubSeparator << 
		::asString(CURRENT_VERSION_NUMBER) << m_cMainSeparator;

	// write the feature type
	UCLID_FEATUREMGMTLib::EFeatureType eFeatureType = ptrFeature->getFeatureType();
	data << (long) eFeatureType << m_cMainSeparator;

	// write the number of parts
	long lNumParts = ptrFeature->getNumParts();
	data << lNumParts;

	if (lNumParts > 0)
	{
		// write out a seperator char to separate the number of parts
		// from the first part
		data << m_cMainSeparator;

		// get the enumpart object
		UCLID_FEATUREMGMTLib::IEnumPartPtr ipEnumPart = ptrFeature->getParts();

		// write out each part
		for (int iPartNumber = 0; iPartNumber < lNumParts; iPartNumber++)
		{
			// get the part object
			UCLID_FEATUREMGMTLib::IPartPtr ipPart = ipEnumPart->next();

			data << asString(ipPart);

			// if there are more parts to write out, then write
			// out a seperator char to seperate the parts
			if (iPartNumber < lNumParts - 1)
			{
				data << m_cMainSeparator;
			}
		}
	}

	// return the result data stream
	string strResult;
	strResult.assign(data.str(), data.pcount());
	free(data.str());
	return strResult;
}
//-------------------------------------------------------------------------------------------------
UCLID_FEATUREMGMTLib::IESSegmentPtr CCommaDelimitedFeatureAttributeDataInterpreter::getArcSegment(
																vector<string>::const_iterator& iter,
																const vector<string>& vecMainTokens)
{
	StringTokenizer stSub(m_cSubSeparator);

	// get the number of parameters associated with the arc
	if (iter == vecMainTokens.end())
	{
		throw UCLIDException("ELI01609", "Unable to read number of ArcSegment parameters!");
	}
	string strTemp = *iter++;
	long lNumParams = ::asLong(strTemp);
	
	// create a IIunknownVector to keep the parameters
	IIUnknownVectorPtr ptrParams(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI01619", ptrParams != NULL);
	
	// read each parameter and add it to ptrParams
	for (int iParam = 0; iParam < lNumParams; iParam++)
	{
		// get the parameter
		if (iter == vecMainTokens.end())
		{
			throw UCLIDException("ELI01621", "Unable to read ArcSegment object's parameter!");
		}
		strTemp = *iter++;
		
		vector<string> vecSubTokens;
		// parse the parameter
		stSub.parse(strTemp, vecSubTokens);
		
		// get the parameter type
		ECurveParameterType eParamType = (ECurveParameterType)asLong(vecSubTokens[0]);
		
		// get the parameter value
		string strValue = vecSubTokens[1];
		
		// create a parametertypevaluepair object
		UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr 
			ptrParam(CLSID_ParameterTypeValuePair);
		ASSERT_RESOURCE_ALLOCATION("ELI01623", ptrParam != NULL);
		ptrParam->eParamType = eParamType;
		ptrParam->strValue = _bstr_t(strValue.c_str());
		
		ptrParams->PushBack(ptrParam);
	}
	
	// create an arc object
	UCLID_FEATUREMGMTLib::IArcSegmentPtr ptrArc(CLSID_ArcSegment);
	ASSERT_RESOURCE_ALLOCATION("ELI01620", ptrArc != NULL);
	UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment(ptrArc);
	// set the parameters to the arcsegment object
	ipSegment->setParameters(ptrParams);
	
	return ipSegment;
}
//-------------------------------------------------------------------------------------------------
UCLID_FEATUREMGMTLib::IESSegmentPtr CCommaDelimitedFeatureAttributeDataInterpreter::getLineSegment(
																vector<string>::const_iterator& iter,
																const vector<string>& vecMainTokens)
{
	StringTokenizer stSub(m_cSubSeparator);
	
	// get the number of parameters associated with the line
	// NOTE", "we expect that there will be two parameters exactly for the line
	if (iter == vecMainTokens.end())
	{
		throw UCLIDException("ELI01610", "Unable to read number of Linesegment parameters!");
	}
	string strTemp = *iter++;
	long lNumParams = ::asLong(strTemp);

	// for version 1, there are only two parameters, which are
	// bearing and distance
	if (m_nVersionNumber < 2 && lNumParams != 2)
	{
		throw UCLIDException("ELI01611", "Invalid number of parameters for LineSegment object!");
	}

	// create a IIunknownVector to keep the parameters
	IIUnknownVectorPtr ptrParams(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI12038", ptrParams != NULL);
	// read each parameter and add it to ptrParams
	for (int iParam = 0; iParam < lNumParams; iParam++)
	{
		// get the parameter
		if (iter == vecMainTokens.end())
		{
			throw UCLIDException("ELI01612", "Unable to read LineSegment object's parameter!");
		}
		strTemp = *iter++;
		
		vector<string> vecSubTokens;
		// parse the parameter
		stSub.parse(strTemp, vecSubTokens);
		
		// get the parameter type
		ECurveParameterType eParamType = (ECurveParameterType)::asLong(vecSubTokens[0]);
		
		// get the parameter value
		string strValue = vecSubTokens[1];
		
		// create a parametertypevaluepair object
		UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr 
			ptrParam(CLSID_ParameterTypeValuePair);
		ASSERT_RESOURCE_ALLOCATION("ELI12039", ptrParam != NULL);
		ptrParam->eParamType = eParamType;
		ptrParam->strValue = _bstr_t(strValue.c_str());
		
		ptrParams->PushBack(ptrParam);
	}
		
	// create a line segment object
	UCLID_FEATUREMGMTLib::ILineSegmentPtr ptrLine(CLSID_LineSegment);
	ASSERT_RESOURCE_ALLOCATION("ELI01618", ptrLine != NULL);
	UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment(ptrLine);
	ipSegment->setParameters(ptrParams);

	return ipSegment;
}
//-------------------------------------------------------------------------------------------------
UCLID_FEATUREMGMTLib::IPartPtr CCommaDelimitedFeatureAttributeDataInterpreter::getPart(
																vector<string>::const_iterator& iter,
																const vector<string>& vecMainTokens)
{
	StringTokenizer stSub(m_cSubSeparator);

	// create a part object
	UCLID_FEATUREMGMTLib::IPartPtr ptrPart(CLSID_Part);
	ASSERT_RESOURCE_ALLOCATION("ELI01605", ptrPart != NULL);
	
	// get the starting point of the part
	if (iter == vecMainTokens.end())
	{
		throw UCLIDException("ELI01604", "Unable to read Part starting point!");
	}
	string strTemp = *iter++;
	
	vector<string> vecSubTokens;
	// ensure that the starting point is in the format "X:Y"
	stSub.parse(strTemp, vecSubTokens);
	if (vecSubTokens.size() == 2)
	{
		double dX = asDouble( vecSubTokens[0] );
		double dY = asDouble( vecSubTokens[1] );
		ICartographicPointPtr ptrPoint(CLSID_CartographicPoint);
		ASSERT_RESOURCE_ALLOCATION("ELI01603", ptrPoint != NULL);
		
		ptrPoint->InitPointInXY(dX, dY);
		ptrPart->setStartingPoint(ptrPoint);
	}
	else
	{
		throw UCLIDException("ELI01602", "Part starting point not specified in expected format!");
	}
	
	// get the number of segments in the current part
	if (iter == vecMainTokens.end())
	{
		throw UCLIDException("ELI01606", "Unable to read number of segments in current part!");
	}
	strTemp = *iter++;
	long lNumSegments = ::asLong(strTemp);
	
	// add each segment to the part
	for (int iSegment = 0; iSegment < lNumSegments; iSegment++)
	{
		// get the segment type
		if (iter == vecMainTokens.end())
		{
			throw UCLIDException("ELI01607", "Unable to read segment type!");
		}
		strTemp = *iter++;
		ESegmentType eSegmentType = (ESegmentType)::asLong(strTemp);
		
		// depending upon the segment type, process the information further
		UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment(NULL);
		switch (eSegmentType)
		{
		case kArc:
			ipSegment = getArcSegment(iter, vecMainTokens);	
			break;

		case kLine:
			ipSegment = getLineSegment(iter, vecMainTokens);
			break;

		default:
			throw UCLIDException("ELI01608", "Invalid segment type!");
		}
		
		// add it to the part
		ptrPart->addSegment(ipSegment);
	}

	return ptrPart;
}
//-------------------------------------------------------------------------------------------------
UCLID_FEATUREMGMTLib::IUCLDFeaturePtr CCommaDelimitedFeatureAttributeDataInterpreter::getFeature(
																vector<string>::const_iterator& iter,
																const vector<string>& vecMainTokens)
{
	StringTokenizer stSub(m_cSubSeparator);
	
	// create a feature object
	UCLID_FEATUREMGMTLib::IUCLDFeaturePtr ptrFeature(CLSID_Feature);
	ASSERT_RESOURCE_ALLOCATION("ELI01600", ptrFeature != NULL);

	// get the version number
	if (iter == vecMainTokens.end())
	{
		throw UCLIDException("ELI12037", "Unable to determine version number!");
	}

	string strTemp = *iter++;
	// parse this string, see if it contains the version number
	// Note: for version 1, there's no version info added at the 
	// very beginning of the data string
	vector<string> vecVersionTokens;
	stSub.parse(strTemp, vecVersionTokens);
	if (vecVersionTokens.size() == 2 && vecVersionTokens[0] == VERSION_STRING)
	{
		// get current version number
		m_nVersionNumber = ::asLong(vecVersionTokens[1]);

		// get the feature type after the version info
		if (iter == vecMainTokens.end())
		{
			throw UCLIDException("ELI01599", "Unable to determine feature type!");
		}

		strTemp = *iter++;
	}

	EFeatureType eFeatureType = (EFeatureType)::asLong(strTemp);
	ptrFeature->setFeatureType((UCLID_FEATUREMGMTLib::EFeatureType)eFeatureType);

	// get the number of parts
	if (iter == vecMainTokens.end())
	{
		throw UCLIDException("ELI01601", "Unable to read part data!");
	}

	strTemp = *iter++;
	long lNumParts = ::asLong(strTemp);

	// create each part object and add it to the feature object
	for (int iPart = 0; iPart < lNumParts; iPart++)
	{
		UCLID_FEATUREMGMTLib::IPartPtr ipPart
			= getPart(iter, vecMainTokens);

		// add the part object to the feature
		ptrFeature->addPart(ipPart);
	}

	return ptrFeature;
}
//-------------------------------------------------------------------------------------------------
void CCommaDelimitedFeatureAttributeDataInterpreter::validateLicense()
{
	static const unsigned long CDFA_DATA_INTERPRETER_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE(CDFA_DATA_INTERPRETER_COMPONENT_ID, "ELI02610",
					"CDFA Data Interpreter");
}
//-------------------------------------------------------------------------------------------------
