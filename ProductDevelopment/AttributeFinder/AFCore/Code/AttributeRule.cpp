// AttributeRule.cpp : Implementation of CAttributeRule
#include "stdafx.h"
#include "AFCore.h"
#include "AttributeRule.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Set current version number
// History : m_bApplyModifyingRules is added in version 2
//         : m_ipDocPreprocessor is added in version 3
//		   : m_bIgnoreErrors, m_bIgnorePreprocessorErrors, m_bIgnoreModifierErrors: version 4
const unsigned long gnCurrentVersion = 4;
const long nNUM_PROGRESS_ITEMS_PER_VALUE_MODIFYING_RULE = 1;

//-------------------------------------------------------------------------------------------------
// CAttributeRule
//-------------------------------------------------------------------------------------------------
CAttributeRule::CAttributeRule()
: m_strAttributeRuleDescription(""),
m_bIsEnabled(true), 
m_bApplyModifyingRules(false),
m_bIgnoreErrors(false),
m_bIgnorePreprocessorErrors(false),
m_bIgnoreModifierErrors(false),
m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CAttributeRule::~CAttributeRule()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16302");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeRule,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_ILicensedComponent
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate license first
		validateLicense();

		// Create the other AttributeRule object
		UCLID_AFCORELib::IAttributeRulePtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08219", ipSource != __nullptr);

		// Set this object's Enabled property
		m_bIsEnabled = asCppBool( ipSource->GetIsEnabled() );

		// Set this object's Value Finder Rule
		ICopyableObjectPtr ipCopyableObject = ipSource->GetAttributeFindingRule();
		ASSERT_RESOURCE_ALLOCATION("ELI08220", ipCopyableObject != __nullptr);
		m_ipAttributeFindingRule = ipCopyableObject->Clone();

		// Set this object's Description
		m_strAttributeRuleDescription = ipSource->GetDescription();

		// whether or not to apply the modifying rules
		m_bApplyModifyingRules = asCppBool( ipSource->GetApplyModifyingRules() );

		// Set this object's vector of Attribute Modifying Rules
		ICopyableObjectPtr ipRulesTemp = ipSource->GetAttributeModifyingRuleInfos();
		ASSERT_RESOURCE_ALLOCATION("ELI08221", ipRulesTemp != __nullptr);
		m_ipAttributeModifyingRuleInfos = ipRulesTemp->Clone();

		// Set this object's Rule-Specific Document Preprocessor
		IObjectWithDescriptionPtr ipPreTemp = ipSource->GetRuleSpecificDocPreprocessor();
		ASSERT_RESOURCE_ALLOCATION("ELI08222", ipPreTemp != __nullptr);
		m_ipDocPreprocessor = ipPreTemp->Clone();

		m_bIgnoreErrors = asCppBool(ipSource->IgnoreErrors);
		m_bIgnoreModifierErrors = asCppBool(ipSource->IgnoreModifierErrors);
		m_bIgnorePreprocessorErrors = asCppBool(ipSource->IgnorePreprocessorErrors);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08223");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate license first
		validateLicense();

		// Create a new IAttributeRule object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_AttributeRule );
		ASSERT_RESOURCE_ALLOCATION("ELI04686", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04687");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::get_AttributeFindingRule(IAttributeFindingRule **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		CComQIPtr<IAttributeFindingRule> ipAFR(m_ipAttributeFindingRule);
		ipAFR.CopyTo(pVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04148");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::put_AttributeFindingRule(IAttributeFindingRule *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipAttributeFindingRule = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04149");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::get_IsEnabled(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIsEnabled);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04150");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::put_IsEnabled(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIsEnabled = asCppBool(newVal);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04694");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::get_Description(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strAttributeRuleDescription.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04151");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::put_Description(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strAttributeRuleDescription = asString(newVal);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04152");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::get_AttributeModifyingRuleInfos(IIUnknownVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// if the AttributeModifyingRules vector object has not yet been created, create it.
		if (m_ipAttributeModifyingRuleInfos == __nullptr)
		{
			m_ipAttributeModifyingRuleInfos.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI04573", m_ipAttributeModifyingRuleInfos != __nullptr);
		}

		CComQIPtr<IIUnknownVector> ipModifyingRuleInfos(m_ipAttributeModifyingRuleInfos);
		ipModifyingRuleInfos.CopyTo(pVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04153");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::put_AttributeModifyingRuleInfos(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipAttributeModifyingRuleInfos = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04154");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::get_ApplyModifyingRules(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bApplyModifyingRules);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05466");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::put_ApplyModifyingRules(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bApplyModifyingRules = asCppBool(newVal);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05467");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::get_IgnoreErrors(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIgnoreErrors);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI0");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::put_IgnoreErrors(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIgnoreErrors = asCppBool(newVal);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI0");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::get_IgnorePreprocessorErrors(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIgnorePreprocessorErrors);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI0");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::put_IgnorePreprocessorErrors(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIgnorePreprocessorErrors = asCppBool(newVal);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI0");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::get_IgnoreModifierErrors(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIgnoreModifierErrors);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI0");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::put_IgnoreModifierErrors(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIgnoreModifierErrors = asCppBool(newVal);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI0");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::ExecuteRuleOnText(IAFDocument* pAFDoc, 
											   IProgressStatus *pProgressStatus,
											   IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Wrap the pProgressStatus in a smart pointer
		IProgressStatusPtr ipProgressStatus(pProgressStatus);

		// create a vector to hold values that have 
		// been processed with modifying rules
		IIUnknownVectorPtr ipResultingValues(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI06033", ipResultingValues != __nullptr);

		if (m_bIsEnabled)
		{
			if (m_ipAttributeFindingRule)
			{
				// Determine whether an enabled attribute-level pre-processor exists and is enabled
				bool bEnabledAttributePreProcessorExists = enabledAttributePreProcessorExists();

				// Determine the number of enabled value modifying rules
				long nNumValueModifyingRules = getEnabledValueModifyingRulesCount();

				// Progress related constants
				// NOTE: the constants below are weighted such that the time it takes to run the rules for an average attribute
				// is approximately double the amount of time it takes to execute either the pre-processor or the output handler.
				const long nNUM_PROGRESS_ITEMS_INITIALIZE = 1;
				const long nNUM_PROGRESS_ITEMS_PRE_PROCESSOR = 1;
				const long nNUM_PROGRESS_ITEMS_VALUE_FINDING_RULE = 2;
				const long nNUM_PROGRESS_ITEMS_VALUE_MODIFYING_RULES = nNumValueModifyingRules * nNUM_PROGRESS_ITEMS_PER_VALUE_MODIFYING_RULE;
				long nTOTAL_PROGRESS_ITEMS = nNUM_PROGRESS_ITEMS_INITIALIZE + // initializing is always going to happen
					nNUM_PROGRESS_ITEMS_VALUE_FINDING_RULE + // value finding rule is always going to be run
					nNUM_PROGRESS_ITEMS_VALUE_MODIFYING_RULES; // # of items for any enabled value modifying rules
				nTOTAL_PROGRESS_ITEMS += bEnabledAttributePreProcessorExists ? nNUM_PROGRESS_ITEMS_PRE_PROCESSOR : 0;

				// Initialize and update progress status
				if (ipProgressStatus)
				{
					ipProgressStatus->InitProgressStatus("Initializing field rule execution...", 0, nTOTAL_PROGRESS_ITEMS, VARIANT_TRUE);

					ipProgressStatus->StartNextItemGroup("Initializing field rule execution...", 
						nNUM_PROGRESS_ITEMS_INITIALIZE);
				}

				// use a copy of the document as we want all changes made to the document
				// to be local to this rule
				UCLID_AFCORELib::IAFDocumentPtr ipAFDoc(pAFDoc);
				ICopyableObjectPtr ipCopyObj = ipAFDoc;
				ASSERT_RESOURCE_ALLOCATION("ELI15665", ipCopyObj != __nullptr);
				UCLID_AFCORELib::IAFDocumentPtr ipAFDocCopy = ipCopyObj->Clone();
				ASSERT_RESOURCE_ALLOCATION("ELI15666", ipAFDocCopy != __nullptr);

				try
				{
					try
					{
						// Preprocess the doc if there's any preprocessor thats enabled
						if (bEnabledAttributePreProcessorExists)
						{
							UCLID_AFCORELib::IDocumentPreprocessorPtr ipDocPreprocessor = m_ipDocPreprocessor->Object;
							if (ipDocPreprocessor)
							{
								// Update progress status
								if (ipProgressStatus)
								{
									ipProgressStatus->StartNextItemGroup("Executing field-rule pre-processor...", 
										nNUM_PROGRESS_ITEMS_PRE_PROCESSOR);
								}

								// Create a pointer to the Sub-ProgressStatus object, depending upon whether
								// the caller requested progress information
								IProgressStatusPtr ipSubProgressStatus = (ipProgressStatus == __nullptr) ? 
									__nullptr : ipProgressStatus->SubProgressStatus;

								// Execute the local attribute-level pre-processor rule
								ipDocPreprocessor->Process(ipAFDocCopy, ipSubProgressStatus);
							}
						}
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI0");
				}
				catch(UCLIDException& ue)
				{
					if (m_bIgnorePreprocessorErrors)
					{
						ue.log();
					}
					else
					{
						throw ue;
					}
				}

				// Update the progress status
				if (ipProgressStatus)
				{
					ipProgressStatus->StartNextItemGroup("Executing field finding rule...", 
						nNUM_PROGRESS_ITEMS_VALUE_FINDING_RULE);
				}

				// Find all possible values through current attribute finding rule,
				// if the progress status is non-NULL pass in its sub progress object
				IIUnknownVectorPtr ipOriginAttributes = m_ipAttributeFindingRule->ParseText(
					ipAFDocCopy, (ipProgressStatus ? ipProgressStatus->SubProgressStatus : __nullptr) );
				ASSERT_RESOURCE_ALLOCATION("ELI06034", ipOriginAttributes != __nullptr);

				// Start next item group if progress status was requested
				if (ipProgressStatus)
				{
					ipProgressStatus->StartNextItemGroup("Applying value modifier rules",
						nNUM_PROGRESS_ITEMS_VALUE_MODIFYING_RULES);
				}

				// Get the number of found attributes
				long nSize = ipOriginAttributes->Size();

				// Create a sub progress status if caller requested progress status updates
				IProgressStatusPtr ipSubProgressStatus = 
					ipProgressStatus ? ipProgressStatus->SubProgressStatus : __nullptr;

				// Initialize the sub progress status if it exists
				if (ipSubProgressStatus)
				{
					ipSubProgressStatus->InitProgressStatus("Initializing value modifier rules",
						0, nSize, VARIANT_FALSE);
				}

				try
				{
					try
					{
						// pass the vector of values through value modifying rules one by one
						for (long n = 0; n < nSize; n++)
						{
							// update the sub progress status object if it exists
							if (ipSubProgressStatus)
							{
								ipSubProgressStatus->StartNextItemGroup(
									get_bstr_t("Executing value modifier rule " + asString(n) +
									" of " + asString(nSize)), 1);
							}

							// get each attribute
							UCLID_AFCORELib::IAttributePtr ipAttribute =
								ipOriginAttributes->At(n);
							ASSERT_RESOURCE_ALLOCATION("ELI06035", ipAttribute != __nullptr);

							ISpatialStringPtr ipAttrValue = ipAttribute->Value;
							ASSERT_RESOURCE_ALLOCATION("ELI15496", ipAttrValue != __nullptr);
							if (ipAttrValue->IsEmpty() == VARIANT_TRUE)
							{
								// do not want any attribute with empty value
								continue;
							}

							// if applying modifying rules...
							if (m_bApplyModifyingRules)
							{
								// TODO: In the future, we want to make this
								// into an option where the user can set their
								// preferences on applying modifying rules
								// on all sub attributes of the attribute
								applyModifyingRulesOnAttribute(ipAttribute, ipAFDocCopy,
									ipSubProgressStatus, true);
							}

							// [P13 #4668]
							// only add the resulting value to the return vector if it's not empty
							ipAttrValue = ipAttribute->Value;
							if (ipAttrValue->IsEmpty() == VARIANT_FALSE)
							{
								// store attribute in the returning vector
								ipResultingValues->PushBack(ipAttribute);
							}
						}
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI0");
				}
				catch(UCLIDException& ue)
				{
					if (m_bIgnoreModifierErrors)
					{
						ue.log();
					}
					else
					{
						throw ue;
					}
				}

				// Temporary fix for [P16:2317]: Merge the string tags from the document copy into 
				// the string tags for the orginal document.
				// Long term solution should involve making a separate hierarchical 
				// <RuleTesterDebugInfo> tag proposed by Arvind
				IStrToStrMapPtr ipStrMap = ipAFDoc->StringTags;
				ASSERT_RESOURCE_ALLOCATION("ELI20203", ipStrMap != __nullptr);
				ipStrMap->Merge(ipAFDocCopy->StringTags, kAppend);
			}
		}

		// Update progress status
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}

		*pAttributes = ipResultingValues.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04164");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::get_RuleSpecificDocPreprocessor(IObjectWithDescription **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (m_ipDocPreprocessor == __nullptr)
		{
			m_ipDocPreprocessor.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI05860", m_ipDocPreprocessor != __nullptr);
		}

		IObjectWithDescriptionPtr ipShallowCopy = m_ipDocPreprocessor;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05861")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::put_RuleSpecificDocPreprocessor(IObjectWithDescription *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipDocPreprocessor = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05862")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	*pClassID = CLSID_AttributeRule;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// check m_bDirty flag first, if it's not dirty then
		// check all objects owned by this object
		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		if (!m_bDirty)
		{
			IPersistStreamPtr ipPersistStream(m_ipAttributeFindingRule);
			if (ipPersistStream==__nullptr)
			{
				throw UCLIDException("ELI04788", "Object does not support persistence!");
			}
			hr = ipPersistStream->IsDirty();
			if (hr == S_OK)
			{
				return hr;
			}

			ipPersistStream = __nullptr;
			ipPersistStream = m_ipAttributeModifyingRuleInfos;
			if (ipPersistStream==__nullptr)
			{
				throw UCLIDException("ELI04789", "Object does not support persistence!");
			}
			hr = ipPersistStream->IsDirty();
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04790");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saved: 
//            data version,
//            whether or not the Rule is enabled
//            Rule description
//            Attribute Finding Rule
//            collection of Attribute Modifying Rules (only if AM Rules are applied)
// Version 2:
//   * Additionally saved:
//            whether or not Attribute Modifying Rules are applied (immediately after Rule description)
//   * NOTE:
//            collection of 0 or more AM Rules is now always saved
// Version 3:
//   * Additionally saved:
//            Document Preprocessor (as ObjectWithDescription)
STDMETHODIMP CAttributeRule::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license state
		validateLicense();

		// Reset all the member variables
		m_bIsEnabled = false;
		m_strAttributeRuleDescription = "";
		m_ipAttributeFindingRule = __nullptr;
		m_ipAttributeModifyingRuleInfos = __nullptr;
		// version 2 has m_bApplyModifyingRules
		m_bApplyModifyingRules = false;
		// Version 3 has a Document Preprocessor
		m_ipDocPreprocessor = __nullptr;
		// Verson 4: ignore error flags
		// True for compatibility with previous behavior. Default for new rules is false.
		m_bIgnoreErrors = true; 
		m_bIgnorePreprocessorErrors = false;
		m_bIgnoreModifierErrors = false;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), __nullptr );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, __nullptr );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI07627", "Unable to load newer AttributeRule." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bIsEnabled;
			dataReader >> m_strAttributeRuleDescription;
		}

		if (nDataVersion >= 2)
		{
			dataReader >> m_bApplyModifyingRules;
		}

		if (nDataVersion >= 4)
		{
			dataReader >> m_bIgnoreErrors;
			dataReader >> m_bIgnorePreprocessorErrors;
			dataReader >> m_bIgnoreModifierErrors;
		}

		// Separately read the value finding rule object from the stream
		IPersistStreamPtr ipObj;
		readObjectFromStream(ipObj, pStream, "ELI09952");
		if (ipObj == __nullptr)
		{
			throw UCLIDException( "ELI04582", 
				"Attribute Finding Rule object could not be read from stream!" );
		}
		m_ipAttributeFindingRule = ipObj;

		// Separately read the attribute modifying rules from the stream
		ipObj = __nullptr;
		readObjectFromStream(ipObj, pStream, "ELI09953");
		if (ipObj == __nullptr)
		{
			throw UCLIDException( "ELI04583", 
				"Attribute Modifying Rules collection could not be read from stream!" );
		}
		m_ipAttributeModifyingRuleInfos = ipObj;

		if (nDataVersion < 2)
		{
			// prior to version2, if vector of value modifying rule is not empty,
			// these rules are applied
			m_bApplyModifyingRules = m_ipAttributeModifyingRuleInfos->Size() > 0;
		}

		// If the version # is 3 or higher, then load the
		// DocumentPreprocessor object-with-description
		if (nDataVersion >= 3)
		{
			ipObj = __nullptr;
			readObjectFromStream(ipObj, pStream, "ELI09954");
			if (ipObj == __nullptr)
			{
				throw UCLIDException( "ELI06123", 
					"DocumentPreprocessor object could not be read from stream!");
			}
			m_ipDocPreprocessor = ipObj;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07306");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;
		dataWriter << m_bIsEnabled;
		dataWriter << m_strAttributeRuleDescription;
		dataWriter << m_bApplyModifyingRules;
		dataWriter << m_bIgnoreErrors;
		dataWriter << m_bIgnorePreprocessorErrors;
		dataWriter << m_bIgnoreModifierErrors;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), __nullptr );
		pStream->Write( data.getData(), nDataLength, __nullptr );

		// Separately write the value finding rule object to the stream
		IPersistStreamPtr ipPersistentObj = m_ipAttributeFindingRule;
		if (ipPersistentObj == __nullptr)
		{
			throw UCLIDException( "ELI04580", 
				"Attribute Finding Rule object does not support persistence!" );
		}
		else
		{
			writeObjectToStream(ipPersistentObj, pStream, "ELI09907", fClearDirty);
		}

		ipPersistentObj = getAttribModifyRuleInfos();
		if (ipPersistentObj == __nullptr)
		{
			throw UCLIDException( "ELI04581", 
				"Attribute Modifying Rules collection does not support persistence!" );
		}
		else
		{
			writeObjectToStream(ipPersistentObj, pStream, "ELI09908", fClearDirty);
		}

		// Separately write the DocumentPreprocessor object-with-description to the stream
		ipPersistentObj = getDocPreprocessor();
		if (ipPersistentObj == __nullptr)
		{
			throw UCLIDException( "ELI06124", 
				"DocumentPreprocessor object does not support persistence!" );
		}
		writeObjectToStream( ipPersistentObj, pStream, "ELI09909", fClearDirty );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07307");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeRule::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (pbValue == __nullptr)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CAttributeRule::getAttribModifyRuleInfos()
{
	try
	{
		if (m_ipAttributeModifyingRuleInfos == __nullptr)
		{
			m_ipAttributeModifyingRuleInfos.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI16930", m_ipAttributeModifyingRuleInfos != __nullptr);
		}

		return m_ipAttributeModifyingRuleInfos;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16929");
}
//-------------------------------------------------------------------------------------------------
IObjectWithDescriptionPtr CAttributeRule::getDocPreprocessor()
{
	try
	{
		if (m_ipDocPreprocessor == __nullptr)
		{
			m_ipDocPreprocessor.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI16928", m_ipDocPreprocessor != __nullptr);
		}

		return m_ipDocPreprocessor;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16927");
}
//-------------------------------------------------------------------------------------------------
void CAttributeRule::applyModifyingRulesOnAttribute(UCLID_AFCORELib::IAttributePtr& ripAttribute, 
													UCLID_AFCORELib::IAFDocument* pOriginInput,
													IProgressStatusPtr ipProgressStatus,
													bool bRecursive)
{
	// Do not apply rules to an empty string
	ISpatialStringPtr ipValue = ripAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI15497", ipValue != __nullptr);
	if (ipValue->IsEmpty() == VARIANT_TRUE)
	{
		return;
	}

	// if recursion is requested...
	if (bRecursive)
	{
		IIUnknownVectorPtr ipSubAttributes = ripAttribute->SubAttributes;
		ASSERT_RESOURCE_ALLOCATION("ELI18151", ipSubAttributes != __nullptr);

		// check if the attribute has any subattribute prior to applying modifying rules.
		// NOTE: since some value modifiers create subattributes, recursion 
		// should only be applied if the original attribute had subattributes. [P13 #4669]
		long nSize = ipSubAttributes->Size();

		executeModifyingRulesOnAttribute(ripAttribute, pOriginInput, ipProgressStatus);
	
		if (nSize == 0)
		{
			return;
		}

		for (long n=0; n<nSize; n++)
		{
			// do the recursion
			// NOTE: no progress information is available on the recursive aspect of
			// running the modifying rules.
			UCLID_AFCORELib::IAttributePtr ipSubAttr(ipSubAttributes->At(n));
			applyModifyingRulesOnAttribute(ipSubAttr, pOriginInput, __nullptr, bRecursive);
		}
	}
	else // no recursion was requested
	{
		executeModifyingRulesOnAttribute(ripAttribute, pOriginInput, ipProgressStatus);
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeRule::executeModifyingRulesOnAttribute(UCLID_AFCORELib::IAttributePtr& ripAttribute, 
													  UCLID_AFCORELib::IAFDocument* pOriginInput,
													  IProgressStatusPtr ipProgressStatus)
{
	if (m_ipAttributeModifyingRuleInfos)
	{
		long nSize = m_ipAttributeModifyingRuleInfos->Size();

		// if the progress status object exists initialize its sub progress object
		IProgressStatusPtr ipSubProgressStatus = 
			(ipProgressStatus ? ipProgressStatus->SubProgressStatus : __nullptr);

		if (ipSubProgressStatus)
		{
			ipSubProgressStatus->InitProgressStatus("Initializing field modifying rules",
				0, nSize, VARIANT_FALSE);
		}

		for (long n=0; n<nSize; n++)
		{
			// get individual AttributeModifyingRuleInfo
			IObjectWithDescriptionPtr ipMRInfo(m_ipAttributeModifyingRuleInfos->At(n));

			// Check if rule exists, and if it is enabled
			if (ipMRInfo && ipMRInfo->Enabled)
			{
				// Update progress status
				if (ipSubProgressStatus)
				{
					string strText = "Executing field modifying rule " + asString(n) + string(" of ") +
						asString(nSize) + string("...");
					ipSubProgressStatus->StartNextItemGroup(strText.c_str(), nNUM_PROGRESS_ITEMS_PER_VALUE_MODIFYING_RULE);
				}

				// check to see if the text is empty, if it is, jump out of the loop
				ISpatialStringPtr ipValue = ripAttribute->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI15498", ipValue != __nullptr);
				if (ipValue->IsEmpty() == VARIANT_TRUE)
				{
					break;
				}

				// Get the modifying rule object
				UCLID_AFCORELib::IAttributeModifyingRulePtr ipModifyingRule(ipMRInfo->Object);
				if (ipModifyingRule == __nullptr)
				{
					UCLIDException uclidException("ELI04165", "Failed to get valid ModifyingRule.");
					uclidException.addDebugInfo("ModifyingRuleName", string(ipMRInfo->Description));
					throw uclidException;
				}

				UCLID_AFCORELib::IAFDocumentPtr ipOriginInput(pOriginInput);

				// modify the value, passing the sub-sub progress status if SubProgressStatus exists
				ipModifyingRule->ModifyValue(ripAttribute, ipOriginInput, 
					(ipSubProgressStatus ? ipSubProgressStatus->SubProgressStatus : __nullptr) );
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeRule::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04902", "Attribute Rule" );
}
//-------------------------------------------------------------------------------------------------
bool CAttributeRule::enabledAttributePreProcessorExists()
{
	return (m_ipDocPreprocessor != __nullptr) && (m_ipDocPreprocessor->Object != __nullptr) &&
		asCppBool( m_ipDocPreprocessor->GetEnabled() );
}
//-------------------------------------------------------------------------------------------------
long CAttributeRule::getEnabledValueModifyingRulesCount()
{
	long nCount = 0;

	if (m_bApplyModifyingRules)
	{
		long nSize = m_ipAttributeModifyingRuleInfos->Size();
		for (long n=0; n<nSize; n++)
		{
			// Determine the number of enabled value modifying rules
			int nNumAttributeRules = m_ipAttributeModifyingRuleInfos->Size();
			for (int i = 0; i < nNumAttributeRules; i++)
			{
				// get individual AttributeModifyingRuleInfo
				IObjectWithDescriptionPtr ipMRInfo = m_ipAttributeModifyingRuleInfos->At(i);

				if ( ipMRInfo != __nullptr && asCppBool(ipMRInfo->Enabled) )
				{
					nCount++;
				}
			}
		}
	}

	return nCount;
}
//-------------------------------------------------------------------------------------------------
