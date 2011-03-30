// GenericMultiSkipCondition.cpp : Implementation of CGenericMultiFAMCondition

#include "stdafx.h"
#include "GenericMultiSkipCondition.h"
#include "ESSkipConditions.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// CGenericMultiFAMCondition
//--------------------------------------------------------------------------------------------------
CGenericMultiFAMCondition::CGenericMultiFAMCondition()
{
}
//--------------------------------------------------------------------------------------------------
CGenericMultiFAMCondition::~CGenericMultiFAMCondition()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16562");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CGenericMultiFAMCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IGenericMultiFAMCondition
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CGenericMultiFAMCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
	{
		return E_POINTER;
	}

	try
	{
		// validate license
		validateLicense();
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IGenericMultiFAMCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGenericMultiFAMCondition::FileMatchesFAMCondition(IIUnknownVector* pFAMConditions, 
	ELogicalOperator eLogicalOperator, IFileRecord* pFileRecord, IFileProcessingDB* pFPDB, 
	long lActionID, IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipMultiFAMConditions = pFAMConditions;
		// If no FAM conditions exist, then throw an exception
		if (ipMultiFAMConditions == __nullptr || ipMultiFAMConditions->Size() == 0)
		{
			throw UCLIDException("ELI13820", "No FAM conditions specified!");
		}

		// If the logical operator is AND or NONE, 
		// set the default return value to true
		if (eLogicalOperator == kANDOperator || eLogicalOperator == kNONEOperator)
		{
			*pRetVal = VARIANT_TRUE;
		}
		// If the logical operator is OR or Exactly one,
		// set the default return to false
		else if (eLogicalOperator == kOROperator || eLogicalOperator == kEXACTONEOperator)
		{
			*pRetVal = VARIANT_FALSE;
		}
		// If the logical operator is kInvalidOperator or anything else 
		// throw an exception
		else
		{
			UCLIDException ue("ELI13863", "Unknown operator for multiple FAM conditions.");
			ue.addDebugInfo ("Logical operator", eLogicalOperator);
			throw ue;
		}

		// Get the number of FAM condition items
		long nNumItems = ipMultiFAMConditions->Size();
		// Determine how many FAM conditions that return MUST MATCH
		// used in Exactly One operator
		int iNumMustMatchCond = 0;
		// If we need to go to next condition
		bool bGotoNextCond = true;

		for (int i = 0; i < nNumItems; i++)
		{
			// if bGotoNextCond is false, simple go out of the loop
			if(!bGotoNextCond)
			{
				break;
			}

			// Get the FAM conditon object-with-description 
			// at the current position
			IObjectWithDescriptionPtr ipObj = ipMultiFAMConditions->At(i);;
			ASSERT_RESOURCE_ALLOCATION("ELI13824", ipObj != __nullptr);

			// If the current FAM condition is enabled
			if (ipObj->Enabled == VARIANT_TRUE)
			{
				// Get the FAM condition inside the object-with-description
				IFAMConditionPtr ipFAMConditionHandler = ipObj->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI13825", ipFAMConditionHandler != __nullptr);

				// check if file matches FAM condition
				VARIANT_BOOL bVal = ipFAMConditionHandler->FileMatchesFAMCondition(
					pFileRecord, pFPDB, lActionID, pFAMTM);

				// Consider different Logical operator as:
				// "AND"  "OR/ATLEAST ONE"  "EXACTLY ONE"  "NONE"
				switch (eLogicalOperator)
				{
				case kANDOperator:
					// If the current FAM condition returns VARIANT_FALSE, set the final
					// return value to VARIANT_FALSE and do not continue to the next condition
					if (bVal == VARIANT_FALSE)
					{
						*pRetVal = VARIANT_FALSE;
						bGotoNextCond = false;
					}
					break;
				case kOROperator:
					// If the current FAM condition returns VARIANT_TRUE, set the final
					// return value to VARIANT_TRUE and do not continue to the next condition
					if (bVal == VARIANT_TRUE)
					{
						*pRetVal = VARIANT_TRUE;
						bGotoNextCond = false;
					}
					break;
				case kEXACTONEOperator:
					// If a condition return MUST MATCH, simply increase iNumMustMatchCond
					if (bVal == VARIANT_TRUE)
					{
						iNumMustMatchCond++;
					}
					// If there is more than one conditions that return MUST MATCH,
					// set return value to false and do not continue to the next condition
					if (iNumMustMatchCond > 1)
					{
						bGotoNextCond = false;
						*pRetVal = VARIANT_FALSE;
					}
					// If there is exactly one condition that return MUST MATCH
					// Set the return value to VARIANT_TRUE;
					else if (iNumMustMatchCond == 1)
					{
						*pRetVal = VARIANT_TRUE;
					}
					break;
				case kNONEOperator:
					// If there is one condition that return MUST MATCH,
					// Set the return value to VARIANT_FALSE;
					if (bVal == VARIANT_TRUE)
					{
						*pRetVal = VARIANT_FALSE;
						bGotoNextCond = false;
					}
					break;
				default:
					// we should never reach here
					THROW_LOGIC_ERROR_EXCEPTION("ELI13821")
				} // End of Switch
			}// End of whether the FAM condition is enabled
		}// End of the loop for each FAM condition
	}// End of try block
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13822")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CGenericMultiFAMCondition::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI13823", "Generic Multi FAM Condition");
}
//-------------------------------------------------------------------------------------------------
