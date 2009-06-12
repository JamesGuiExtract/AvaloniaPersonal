// Part.cpp ", "Implementation of CPart
#include "stdafx.h"
#include "UCLIDFeatureMgmt.h"
#include "Part.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CPart
//-------------------------------------------------------------------------------------------------
CPart::CPart()
{
}
//-------------------------------------------------------------------------------------------------
CPart::~CPart()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPart::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IPart,
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
// IPart
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPart::getSegments(IEnumSegment **pEnumSegment)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// create an enumsegment object
		UCLID_FEATUREMGMTLib::IEnumSegmentPtr ptrEnumSegment(CLSID_EnumSegment);
		ASSERT_RESOURCE_ALLOCATION("ELI01516", ptrEnumSegment != NULL);

		// Do a QI and get the IEnumSegmentModifier interface
		UCLID_FEATUREMGMTLib::IEnumSegmentModifierPtr ptrEnumSegmentModifier(ptrEnumSegment);
		ASSERT_RESOURCE_ALLOCATION("ELI01517", ptrEnumSegmentModifier != NULL);

		// add the segments to the interface
		vector<UCLID_FEATUREMGMTLib::IESSegmentPtr>::iterator iter;
		for (iter = m_vecSegments.begin(); iter != m_vecSegments.end(); iter++)
		{
			ptrEnumSegmentModifier->addSegment(*iter);
		}
		
		// return the enumsegment to the caller
		*pEnumSegment = (IEnumSegment *)ptrEnumSegment.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01653");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPart::getNumSegments(long *plNumSegments)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// return the size of the vector
		*plNumSegments = m_vecSegments.size();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01654");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPart::addSegment(IESSegment *pSegment)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// add the segment object to the vector
		m_vecSegments.push_back(pSegment);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01655");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPart::setStartingPoint(ICartographicPoint *pStartingPoint)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		m_ptrStartingPoint = pStartingPoint;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01656");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPart::getStartingPoint(ICartographicPoint **pStartingPoint)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		if (m_ptrStartingPoint == NULL)
		{
			throw UCLIDException("ELI01519", "The starting point has not been set!");
		}

		// increment reference count and 
		// return a reference to the starting point object
		m_ptrStartingPoint.AddRef();
		*pStartingPoint = m_ptrStartingPoint;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01657");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPart::getEndingPoint(ICartographicPoint **pEndingPoint)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		if (m_ptrStartingPoint == NULL)
		{
			throw UCLIDException("ELI12491", "Please set starting point before getting ending point");
		}

		// go through all segments in this part
		ICartographicPointPtr ipStartPoint(m_ptrStartingPoint), ipEndPoint(NULL), ipMidPoint(NULL);
		int nNumOfSegments = m_vecSegments.size();
		for (int n=0; n<nNumOfSegments; n++)
		{
			UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment(m_vecSegments[n]);
			UCLID_FEATUREMGMTLib::ESegmentType eSegmentType = ipSegment->getSegmentType();
			if (eSegmentType == UCLID_FEATUREMGMTLib::kLine)
			{
				UCLID_FEATUREMGMTLib::ILineSegmentPtr ipLine(ipSegment);
				ipLine->getCoordsFromParams(ipStartPoint, &ipEndPoint);
			}
			else if (eSegmentType == UCLID_FEATUREMGMTLib::kArc)
			{
				UCLID_FEATUREMGMTLib::IArcSegmentPtr ipArc(ipSegment);
				ipArc->getCoordsFromParams(ipStartPoint, &ipMidPoint, &ipEndPoint);
			}

			ASSERT_RESOURCE_ALLOCATION("ELI12492", ipEndPoint != NULL);

			// if this is the last segment of the part
			if (n == nNumOfSegments-1)
			{
				break;
			}

			// otherwise, set start point to the end point
			ipStartPoint = ipEndPoint;
		}

		*pEndingPoint = ipEndPoint.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12489");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPart::valueIsEqualTo(IPart *pPart, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		UCLID_FEATUREMGMTLib::IPartPtr ptrPart(pPart);

		// unless ALL necessary requirements are met, the values
		// are not considered equal
		*pbValue = VARIANT_FALSE;

		// get the starting point of the specified part
		ICartographicPointPtr ptrStartPoint = ptrPart->getStartingPoint();

		// compare the starting points
		VARIANT_BOOL bSameStartPoint = ptrStartPoint->IsEqual(m_ptrStartingPoint);
		if (bSameStartPoint == VARIANT_TRUE)
		{
			// create a vector to store the segments
			vector<UCLID_FEATUREMGMTLib::IESSegmentPtr> vecSegments;
			UCLID_FEATUREMGMTLib::IEnumSegmentPtr ptrEnumSegment = ptrPart->getSegments();

			UCLID_FEATUREMGMTLib::IESSegmentPtr ptrSegment(NULL);
			do
			{
				ptrSegment = ptrEnumSegment->next();

				if (ptrSegment != NULL)
				{
					vecSegments.push_back(ptrSegment);
				}

			} while (ptrSegment != NULL);


			// compare the size and contents of the two vectors
			long lVecSize = m_vecSegments.size();
			if (vecSegments.size() == lVecSize)
			{
				// for each segment, get the two segments and compare the type, and the contents
				for (int i = 0; i < lVecSize; i++)
				{
					// get the two segments
					UCLID_FEATUREMGMTLib::IESSegmentPtr ptrCurrSegment = vecSegments[i];
					UCLID_FEATUREMGMTLib::IESSegmentPtr ptrActualSegment = m_vecSegments[i];
					UCLID_FEATUREMGMTLib::ESegmentType eCurrSegmentType 
						= ptrCurrSegment->getSegmentType();
					UCLID_FEATUREMGMTLib::ESegmentType eActualSegmentType
						= ptrActualSegment->getSegmentType();

					// compare the segment types
					if (eCurrSegmentType == eActualSegmentType)
					{
						// the segment types are equal.
						// now determine if the values are equal
						if (eCurrSegmentType == kArc)
						{
							// get the IArcSegment interface for the current segment
							UCLID_FEATUREMGMTLib::IArcSegmentPtr ptrCurrArcSegment = ptrCurrSegment;
							// TODO", "throw UCLIDException("ELI01566", "Unable to access IArcSegment interface!");
							
							// get the IArcSegment interface for the actual segment
							UCLID_FEATUREMGMTLib::IArcSegmentPtr ptrActualArcSegment = ptrActualSegment;
							// TODO", "throw UCLIDException("ELI01567", "Unable to access IArcSegment interface!");

							// compare the two arc segments
							VARIANT_BOOL bSame = ptrActualArcSegment->valueIsEqualTo(ptrCurrArcSegment);
							
							// if this is the last segment we are comparing, and if the
							// segments are equal, then the two parts are equal
							if (i == lVecSize - 1 && bSame == VARIANT_TRUE)
							{
								*pbValue = VARIANT_TRUE;
							}
							else if (bSame == VARIANT_FALSE)
							{
								// the two segments are not equal.
								// Therefore, the two parts are not equal
								// no need to do further comparisions
								break;
							}
						}
						else if (eCurrSegmentType == kLine)
						{
							// get the ILineSegment interface for the current segment
							UCLID_FEATUREMGMTLib::ILineSegmentPtr ptrCurrLineSegment = ptrCurrSegment;
							// TODO", "throw UCLIDException("ELI01568", "Unable to access ILineSegment interface!");
							
							// get the ILineSegment interface for the actual segment
							UCLID_FEATUREMGMTLib::ILineSegmentPtr ptrActualLineSegment = ptrActualSegment;
							// TODO", "throw UCLIDException("ELI01569", "Unable to access ILineSegment interface!");

							// compare the two Line segments
							VARIANT_BOOL bSame = ptrActualLineSegment->valueIsEqualTo(ptrCurrLineSegment);
							
							// if this is the last segment we are comparing, and if the
							// segments are equal, then the two parts are equal
							if (i == lVecSize - 1 && bSame == VARIANT_TRUE)
							{
								*pbValue = TRUE;
							}
							else if (bSame == VARIANT_FALSE)
							{
								// the two segments are not equal.
								// Therefore, the two parts are not equal
								// no need to do further comparisions
								break;
							}
						}
						else
						{
							// We should never reach here!
							throw UCLIDException("ELI01565", "Invalid segment type!");
						}
					}
					else
					{
						// the segment types are not equal, and therefore
						// we can stop all further comparisions
						break;
					}
				} // end for each segment in vector
			} // end vector size comparison
		} // end starting point comprision
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01658");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPart::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CPart::validateLicense()
{
	static const unsigned long PART_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( PART_COMPONENT_ID, "ELI02616", "Part" );
}
//-------------------------------------------------------------------------------------------------
