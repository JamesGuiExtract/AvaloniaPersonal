// Feature.cpp ", "Implementation of CFeature
#include "stdafx.h"
#include "UCLIDFeatureMgmt.h"
#include "Feature.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CFeature
//-------------------------------------------------------------------------------------------------
CFeature::CFeature()
{
	m_eFeatureType = kInvalidFeatureType;
}
//-------------------------------------------------------------------------------------------------
CFeature::~CFeature()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeature::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IUCLDFeature,
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
// IUCLDFeature
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeature::getParts(IEnumPart **pEnumPart)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// create an EnumPart object
		HRESULT hr = NULL;
		UCLID_FEATUREMGMTLib::IEnumPartPtr ptrEnumPart(CLSID_EnumPart);
		ASSERT_RESOURCE_ALLOCATION("ELI01520", ptrEnumPart != NULL);

		// Do a 'QI and get the IEnumPartModifier interface
		UCLID_FEATUREMGMTLib::IEnumPartModifierPtr ptrEnumPartModifier(ptrEnumPart);

		// add the parts to the EnumPart object
		vector<UCLID_FEATUREMGMTLib::IPartPtr>::iterator iter;
		for (iter = m_vecParts.begin(); iter != m_vecParts.end(); iter++)
		{
			ptrEnumPartModifier->addPart(*iter);
		}

		// return the enum part to the caller
		*pEnumPart = (IEnumPart *)ptrEnumPart.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01667");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeature::getNumParts(long *plNumParts)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// return the size of the parts vector
		*plNumParts = m_vecParts.size();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01668");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeature::addPart(IPart *pPart)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// add the part to the vector
		m_vecParts.push_back(pPart);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01669");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeature::setFeatureType(EFeatureType eFeatureType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		m_eFeatureType = eFeatureType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01670");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeature::getFeatureType(EFeatureType *pFeatureType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		*pFeatureType = m_eFeatureType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01671");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeature::valueIsEqualTo(IUCLDFeature *pFeature, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// unless all necessary conditions are met, the two
		// parts are not considered equal
		*pbValue = VARIANT_FALSE;

		UCLID_FEATUREMGMTLib::IUCLDFeaturePtr ptrFeature(pFeature);
		
		// get the two feature-type related attributes
		UCLID_FEATUREMGMTLib::EFeatureType eFeatureType = ptrFeature->getFeatureType();

		// compare the two feature-type related boolean attributes for equality
		if (m_eFeatureType == eFeatureType)
		{
			// build a vector of Part objects for the given feature
			vector<UCLID_FEATUREMGMTLib::IPartPtr> vecParts;
			UCLID_FEATUREMGMTLib::IEnumPartPtr ptrEnumPart= ptrFeature->getParts();
	
			UCLID_FEATUREMGMTLib::IPartPtr ptrPart(NULL);
			do
			{
				ptrPart = ptrEnumPart->next();
				if (ptrPart != NULL)
				{
					vecParts.push_back(ptrPart);
				}

			} while (ptrPart != NULL);


			// compare the size of the part vectors
			if (vecParts.size() == m_vecParts.size())
			{
				// for each part, compare the actual and given values
				long lVecSize = m_vecParts.size();
				for (int i = 0; i < lVecSize; i++)
				{
					// get the current part (from the given feature)
					UCLID_FEATUREMGMTLib::IPartPtr ptrCurrentPart = vecParts[i];

					// get the actual part (in the current feature)
					UCLID_FEATUREMGMTLib::IPartPtr ptrActualPart = m_vecParts[i];

					// compare the two parts
					VARIANT_BOOL bSame = ptrCurrentPart->valueIsEqualTo(ptrActualPart);
					
					// if this is the last part we are comparing, and if the
					// parts are equal, then the two features are equal
					if (i == lVecSize - 1 && bSame == VARIANT_TRUE)
					{
						*pbValue = VARIANT_TRUE;
					}
					else if (bSame == VARIANT_FALSE)
					{
						// the parts segments are not equal.
						// Therefore, the two features are not equal
						// no need to do further comparisions
						break;
					}
				}
			} // end vector size comparison

		} // end feature-type related boolean attributes comparison
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01672");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeature::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CFeature::validateLicense()
{
	static const unsigned long FEATURE_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( FEATURE_COMPONENT_ID, "ELI02613", "Feature" );
}
//-------------------------------------------------------------------------------------------------
