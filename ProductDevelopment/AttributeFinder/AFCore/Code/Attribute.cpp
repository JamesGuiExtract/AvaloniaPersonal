// Attribute.cpp : Implementation of CAttribute
#include "stdafx.h"
#include "AFCore.h"
#include "Attribute.h"
#include "RuleSetProfiler.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>
#include <COMUtilsMethods.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 3;
static const GUID gProfilingGUID = { 0x3f9058ec, 0x77e3, 0x43a5, 
			{ 0x95, 0x35, 0x22, 0x97, 0x88, 0x67, 0x1d, 0x6a } };

//-------------------------------------------------------------------------------------------------
// CAttribute
//-------------------------------------------------------------------------------------------------
CAttribute::CAttribute()
: m_strAttributeName(""),
  m_strAttributeType(""),
  m_ipAttributeValue(__nullptr),
  m_ipInputValidator(__nullptr),
  m_ipAttributeSplitter(__nullptr),
  m_ipSubAttributes(__nullptr),
  m_ipDataObject(__nullptr),
  m_ipMemoryManager(__nullptr),
  m_ipMiscUtils(__nullptr),
  m_bDirty(false)
{
	try
	{
		PROFILE_SPECIAL_OBJECT_IF_ACTIVE("[Attribute Created]", "", gProfilingGUID, 0)
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33635");
}
//-------------------------------------------------------------------------------------------------
CAttribute::~CAttribute()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16299");
}
//-------------------------------------------------------------------------------------------------
HRESULT CAttribute::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CAttribute::FinalRelease()
{
	try
	{
		// Release COM objects before the object is destructed
		m_ipSubAttributes = __nullptr;
		m_ipAttributeValue = __nullptr;
		m_ipAttributeSplitter = __nullptr;
		m_ipInputValidator = __nullptr;
		m_ipDataObject = __nullptr;

		// If memory usage has been reported, report that this instance is no longer using any
		// memory.
		RELEASE_MEMORY_MANAGER(m_ipMemoryManager, "ELI36087");
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI26474");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttribute,
		&IID_ILicensedComponent,
		&IID_ICopyableObject,
		&IID_IComparableObject,
		&IID_IPersistStream,
		&IID_IManageableMemory,
		&IID_IIdentifiableObject,
		&IID_ICloneIdentifiableObject
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IAttribute
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::get_Name(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strAttributeName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04140");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::put_Name(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strName = asString(newVal);

		if(strName != m_strAttributeName)
		{

			// Zero-length names are valid (it is the initial state for an attribute)
			// With the addition of rule-specific output handlers it is better to not treat this as
			// an exception.
			// https://extract.atlassian.net/browse/ISSUE-13607
			if(!strName.empty())
			{
				// Check that Name is a valid Identifier
				validateIdentifier( strName );
			}

			m_bDirty = true;
			m_strAttributeName = strName;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04141");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::get_Value(ISpatialString **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipAttributeValue==__nullptr)
		{
			m_ipAttributeValue.CreateInstance(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI05863", m_ipAttributeValue != __nullptr);
		}

		ISpatialStringPtr ipShallowCopy = m_ipAttributeValue;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04142");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::put_Value(ISpatialString *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipAttributeValue = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04143");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::get_InputValidator(IInputValidator **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26476", pVal != __nullptr);
		validateLicense();

		// Default to __nullptr
		*pVal = __nullptr;

		if (m_ipInputValidator)
		{
			IInputValidatorPtr ipShallowCopy = m_ipInputValidator;
			ASSERT_RESOURCE_ALLOCATION("ELI26475", ipShallowCopy != __nullptr);

			*pVal = ipShallowCopy.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04144");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::put_InputValidator(IInputValidator *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipInputValidator = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04145");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::get_SubAttributes(IIUnknownVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26472", pVal != __nullptr);

		validateLicense();

		IIUnknownVectorPtr ipSubAttributes = getSubAttributes();

		*pVal = ipSubAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05263");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::put_SubAttributes(IIUnknownVector *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipSubAttributes = pNewVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05262");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::get_AttributeSplitter(IAttributeSplitter **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26478", pVal != __nullptr);

		validateLicense();

		*pVal = __nullptr;

		if (m_ipAttributeSplitter)
		{
			UCLID_AFCORELib::IAttributeSplitterPtr ipShallowCopy = m_ipAttributeSplitter;
			ASSERT_RESOURCE_ALLOCATION("ELI26477", ipShallowCopy != __nullptr);

			*pVal = (IAttributeSplitter*) ipShallowCopy.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05261");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::put_AttributeSplitter(IAttributeSplitter *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipAttributeSplitter = pNewVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05260");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::get_Type(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Provide string
		*pVal = get_bstr_t(m_strAttributeType).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05838");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::put_Type(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Tokenize the string
		string strValue = asString( newVal );
		vector<string>	vecTypes;
		StringTokenizer	st( '+' );
		st.parse( strValue, vecTypes );

		// Check that each Type (separated by +) is a valid identifier
		for (unsigned int ui = 0; ui < vecTypes.size(); ui++)
		{
			// Get the Type
			const string& strThisType = vecTypes[ui];

			// Check length
			if (strThisType.length() == 0)
			{
				// Create and throw exception
				UCLIDException ue( "ELI09526", "Attribute Type Cannot Have Consecutive +." );
				ue.addDebugInfo( "Type", strValue );
				throw ue;
			}

			// Type must be a valid identifier [FIDSC #3690]
			validateIdentifier(strThisType);
		}

		// Type must be valid
		m_strAttributeType = strValue;

		// Set flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05839");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::AddType(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Check string for reserved character
		string strValue = asString( newVal );
		vector<string>	vecTypes;
		StringTokenizer	st( '+' );
		st.parse( strValue, vecTypes );

		string strNewType = m_strAttributeType;

		// Check that each Type (separated by +) is a valid identifier
		for (unsigned int ui = 0; ui < vecTypes.size(); ui++)
		{
			// Get the Type
			string strThisType = vecTypes[ui];

			// Check length
			if (strThisType.length() == 0)
			{
				// Create and throw exception
				UCLIDException ue( "ELI10505", "Attribute Type Cannot Have Consecutive +." );
				ue.addDebugInfo( "Type", strValue );
				throw ue;
			}

			// Check that Type is a valid Identifier
			// since Names must be storable in the Type field
			validateIdentifier( strThisType );

			if (containsType(strThisType))
			{
				// [LegacyRCAndUtils:5014]
				// This attribute already contains this type; don't duplicate it.
				continue;
			}

			if (!strNewType.empty())
			{
				// Store the new Type 
				strNewType += '+';
			}
			strNewType += strThisType;
		}

		m_strAttributeType = strNewType;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09264");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::ContainsType(BSTR strType, VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		*pVal = asVariantBool(containsType(asString(strType)));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09267");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::IsNonSpatialMatch(IAttribute* pTest, VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Convert test Attribute into smart pointer
		UCLID_AFCORELib::IAttributePtr ipTest( pTest );
		ASSERT_RESOURCE_ALLOCATION("ELI15661", ipTest != __nullptr);

		// Default the return to non-match
		*pVal = VARIANT_FALSE;

		// Compare Name strings
		string	strTestName = asString(ipTest->Name);
		if (strTestName.compare( m_strAttributeName ) != 0)
		{
			// Name strings do not match
			return S_OK;
		}

		// Get local string for Value
		string strLocalValue;
		if (m_ipAttributeValue)
		{
			strLocalValue = asString(m_ipAttributeValue->String);
		}

		// Retrieve Value string from test Attribute
		ISpatialStringPtr ipValue = ipTest->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI15662", ipValue != __nullptr);
		string	strTestValue = asString(ipValue->String);

		// Compare Value strings
		if (strTestValue.compare( strLocalValue ) != 0)
		{
			// Value strings do not match
			return S_OK;
		}

		// Retrieve and compare Type string
		string strTestType = asString(ipTest->Type);
		if (strTestType.compare( m_strAttributeType ) != 0)
		{
			// Type strings do not match
			return S_OK;
		}

		IIUnknownVectorPtr ipTestSub = ipTest->SubAttributes;
		long lThisCount = m_ipSubAttributes != __nullptr ? m_ipSubAttributes->Size() : -1;
		long lTestCount = ipTestSub != __nullptr ? ipTestSub->Size() : -1;

		// Compare collection of Sub-Attributes
		if (lThisCount > 0 || lTestCount > 0)
		{
			// If neither collection is __nullptr and both have a count > 0 then compare
			// both collections of sub attributes
			if (m_ipSubAttributes != __nullptr && ipTestSub != __nullptr)
			{
				// Both collections exist, now check counts
				if (lThisCount != lTestCount)
				{
					// Sub-attributes do not match
					return S_OK;
				}

				// Make extra, disposable shallow copy of local sub-attributes
				IShallowCopyablePtr ipExtraCopy = m_ipSubAttributes;
				ASSERT_RESOURCE_ALLOCATION("ELI15669", ipExtraCopy != __nullptr);
				IIUnknownVectorPtr ipExtra = ipExtraCopy->ShallowCopy();
				ASSERT_RESOURCE_ALLOCATION("ELI15670", ipExtra != __nullptr);

				// Store the size of the extra clone.
				// The clone is the same size as the m_ipSubAttributes at this point.
				long lExtraSize = lThisCount;

				// Loop through each Test sub-attribute
				for (long i = 0; i < lTestCount; i++)
				{
					// Retrieve this sub-attribute
					UCLID_AFCORELib::IAttributePtr ipTestSubAttr = ipTestSub->At( i );
					ASSERT_RESOURCE_ALLOCATION("ELI15654", ipTestSubAttr != __nullptr);

					// Check against updated (extra) set of local sub-attributes
					bool bFoundMatch = false;
					for (long j = 0; j < lExtraSize; j++)
					{
						// Retrieve this sub-attribute
						UCLID_AFCORELib::IAttributePtr ipExtraSubAttr = ipExtra->At( j );
						ASSERT_RESOURCE_ALLOCATION("ELI15671", ipExtraSubAttr != __nullptr);

						// Do a non-spatial comparison of these two items
						if (ipTestSubAttr->IsNonSpatialMatch( ipExtraSubAttr ) == VARIANT_TRUE)
						{
							// Set the flag and remove this sub-attribute (decrement the size count)
							bFoundMatch = true;
							ipExtra->Remove( j );
							lExtraSize--;
							break;
						}
					}

					// A match for this attribute was not found, quit the comparison
					if (!bFoundMatch)
					{
						return S_OK;
					}
				}

				// If we reached here, each Test sub-attribute found a corresponding 
				// match in the extra copy of the local sub-attributes and 
				// the Attributes being non-spatially compared do match
				*pVal = VARIANT_TRUE;
			}
			// Else least one count > 0 then one of the collections must not be null
			// return the default value of no match
			else
			{
				return S_OK;
			}
		}

		// If we reached here, there were no sub-attributes to match and
		// the Attributes being non-spatially compared do match
		*pVal = VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15664");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::get_DataObject(IUnknown **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24400", pVal != __nullptr);

		validateLicense();

		// If caller is accessing a DataObject that doesn't currently exist, but we have a stowed
		// version, restore the DataObject via the stowed version.
		if (m_ipDataObject == __nullptr && m_upStowedDataObject)
		{
			m_ipDataObject = readObjFromByteStream(m_upStowedDataObject.get());
		}

		if (m_ipDataObject == __nullptr)
		{
			*pVal = __nullptr;
		}
		else
		{
			IUnknownPtr ipShallowCopy = m_ipDataObject;
			*pVal = ipShallowCopy.Detach();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24401");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::put_DataObject(IUnknown *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipDataObject != newVal)
		{
			m_ipDataObject = newVal;
			m_upStowedDataObject.reset(__nullptr);
			m_bDirty = true;
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24402");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::GetAttributeSize(long* plAttributeSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26467", plAttributeSize != __nullptr);

		validateLicense();

		// Default the count to 1
		long lCount = 1;

		// Add in the size of each sub attribute
		IIUnknownVectorPtr ipSubAttributes = getSubAttributes();
		long lSize = ipSubAttributes->Size();
		for (long i = 0; i < lSize; i++)
		{
			UCLID_AFCORELib::IAttributePtr ipAttr = ipSubAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI26468", ipAttr != __nullptr);

			lCount += ipAttr->GetAttributeSize();
		}

		*plAttributeSize = lCount;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26469");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		UCLID_AFCORELib::IAttributePtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08214", ipSource != __nullptr);

		copyFrom (pObject, false);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08218");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create a new IAttribute object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI05280", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05281");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IComparableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::raw_IsEqualTo(IUnknown * pObj, VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Default flag to true
		bool	bAllPartsMatch = true;

		/////////////////////////////////////////
		// Check all elements of Attribute object
		/////////////////////////////////////////

		// Comparison object must be an IAttribute
		UCLID_AFCORELib::IAttributePtr ipTest = pObj;
		if (ipTest == __nullptr)
		{
			// Objects obviously do not match
			bAllPartsMatch = false;
		}
		else
		{
			//////////////////////
			// Compare Name string
			//////////////////////
			string	strTest = ipTest->GetName();
			if (strTest.compare( m_strAttributeName ) != 0)
			{
				// Name string does not match
				bAllPartsMatch = false;
				goto stop_checking;
			}

			///////////////////////
			// Compare Value string
			///////////////////////
			// Retrieve Value from Test attribute
			ISpatialStringPtr ipTestValue = ipTest->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15368", ipTestValue != __nullptr);

			// Special comparison available for EAV file attributes (P16 #2215)
			// where SourceDocName == "" && m_eMode == kNonSpatial
			string strSDN = asString( ipTestValue->SourceDocName );
			ESpatialStringMode eMode = ipTestValue->GetMode();
			if ((strSDN == "") && (eMode == kNonSpatialMode))
			{
				// Just compare the STL strings
				string strThisValue = asString( m_ipAttributeValue->String );
				string strTestValue = asString( ipTestValue->String );
				if (strThisValue != strTestValue)
				{
					// Value string does not match
					bAllPartsMatch = false;
					goto stop_checking;
				}
			}
			// Non-EAV attribute, must compare SourceDocName, mode, and STL string
			else
			{
				IComparableObjectPtr ipThis(m_ipAttributeValue);
				if (ipThis == __nullptr)
				{
					throw UCLIDException("ELI05868", 
						"SpatialString doesn't implement IComparableObject.");
				}
				if (ipThis->IsEqualTo(ipTest->Value) == VARIANT_FALSE)
				{
					// Name string does not match
					bAllPartsMatch = false;
					goto stop_checking;
				}
			}

			//////////////////////
			// Compare Type string
			//////////////////////
			strTest = ipTest->GetType();
			if (strTest.compare( m_strAttributeType ) != 0)
			{
				// Type string does not match
				bAllPartsMatch = false;
				goto stop_checking;
			}

			// Compare collection of Sub-Attributes
			IIUnknownVectorPtr	ipTestSub = ipTest->GetSubAttributes();
			if ((m_ipSubAttributes == __nullptr) && (ipTestSub == __nullptr))
			{
				// Neither Attribute object has a defined sub-attribute vector

				// This is okay, both objects may still match
			}
			else if ((m_ipSubAttributes == __nullptr) && (ipTestSub != __nullptr) && 
				(ipTestSub->Size() == 0))
			{
				// Local Attribute object has no sub-attribute vector defined, 
				// Test Attribute object has sub-attribute vector of size 0

				// This is okay, both objects may still match
			}
			else if ((ipTestSub == __nullptr) && (m_ipSubAttributes != __nullptr) && 
				(m_ipSubAttributes->Size() == 0))
			{
				// Test Attribute object has no sub-attribute vector defined, 
				// Local Attribute object has sub-attribute vector of size 0

				// This is okay, both objects may still match
			}
			else if (m_ipSubAttributes != __nullptr && (m_ipSubAttributes->Size() == 0) && ipTestSub != __nullptr && (ipTestSub->Size() == 0))
			{
				// Local Attribute object has sub-attribute vector of size 0
				// Test Attribute object has sub-attribute vector of size 0

				// This is okay, both objects may still match
			}
			else if ((m_ipSubAttributes != __nullptr) && (ipTestSub != __nullptr))
			{
				// Both collections exist, now check for IComparableObject support
				IComparableObjectPtr ipComp1 = m_ipSubAttributes;
				IComparableObjectPtr ipComp2 = ipTestSub;

				// Make sure that both elements implement IComparableObject
				if ((ipComp1 == __nullptr) || (ipComp2 == __nullptr))
				{
					bAllPartsMatch = false;
					goto stop_checking;
				}

				// Compare the objects
				if (ipComp1->IsEqualTo( ipComp2 ) == VARIANT_FALSE)
				{
					// SubAttribute collections do not match
					bAllPartsMatch = false;
					goto stop_checking;
				}
			}
			else
			{
				// One or the other does not have a SubAttributes collection
				bAllPartsMatch = false;
				goto stop_checking;
			}

			///////////////////////////////////////
			// TODO: Compare Input Validator object
			///////////////////////////////////////
//			IInputValidatorPtr	ipTestIV = ipTest->GetInputValidator();
//			if ((m_ipInputValidator == __nullptr) && (ipTestIV == __nullptr))
//			{
//				// This is okay, both objects may still match
//			}
//			else if ((m_ipInputValidator != __nullptr) && (ipTestIV != __nullptr))
//			{
//				// Both objects exist, now compare them
//				IComparableObjectPtr	ipCompTest1 = m_ipInputValidator;
//				IComparableObjectPtr	ipCompTest2 = ipTestIV;
//				if ((ipCompTest1 == __nullptr) || (ipCompTest2 == __nullptr))
//				{
//					// One or the other does not implement IComparableObject
//					bAllPartsMatch = false;
//					goto stop_checking;
//				}
//				else
//				{
//					// Compare the Input Validators
//					if (ipCompTest1->IsEqualTo( ipCompTest2 ) == VARIANT_FALSE)
//					{
//						// Input Validators do not match
//						bAllPartsMatch = false;
//						goto stop_checking;
//					}
//				}
//			}
//			else
//			{
//				// One or the other does not have an Input Validator defined
//				bAllPartsMatch = false;
//				goto stop_checking;
//			}

			//////////////////////////////////////////
			// TODO: Compare Attribute Splitter object
			//////////////////////////////////////////
//			IAttributeSplitterPtr	ipTestSplit = ipTest->GetAttributeSplitter();
//			if ((m_ipAttributeSplitter == __nullptr) && (ipTestSplit == __nullptr))
//			{
//				// This is okay, both objects may still match
//			}
//			else if ((m_ipAttributeSplitter != __nullptr) && (ipTestSplit != __nullptr))
//			{
//				// Both objects exist, now compare them
//				IComparableObjectPtr	ipCompTest1 = m_ipAttributeSplitter;
//				IComparableObjectPtr	ipCompTest2 = ipTestSplit;
//				if ((ipCompTest1 == __nullptr) || (ipCompTest2 == __nullptr))
//				{
//					// One or the other does not implement IComparableObject
//					bAllPartsMatch = false;
//					goto stop_checking;
//				}
//				else
//				{
//					// Compare the Attribute Splitters
//					if (ipCompTest1->IsEqualTo( ipCompTest2 ) == VARIANT_FALSE)
//					{
//						// Attribute Splitters do not match
//						bAllPartsMatch = false;
//						goto stop_checking;
//					}
//				}
//			}
//			else
//			{
//				// One or the other does not have an Attribute Splitter defined
//				bAllPartsMatch = false;
//				goto stop_checking;
//			}
		}

stop_checking:
		// Provide result to caller
		if (bAllPartsMatch)
		{
			*pbValue = VARIANT_TRUE;
		}
		else
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05588");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IManageableMemory
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::raw_ReportMemoryUsage(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_ipMemoryManager == __nullptr)
		{
			m_ipMemoryManager.CreateInstance(MEMORY_MANAGER_CLASS);
		}
		
		IIUnknownVectorPtr ipSubAttributes = getSubAttributes();
		long nAttributeCount = ipSubAttributes->Size();
		m_ipMemoryManager->ReportUnmanagedMemoryUsage(sizeof(*this) + nAttributeCount);
		
		if (m_ipAttributeValue != __nullptr)
		{
			IManageableMemoryPtr ipManageableMemory = m_ipAttributeValue;
			ASSERT_RESOURCE_ALLOCATION("ELI36014", ipManageableMemory != __nullptr);

			ipManageableMemory->ReportMemoryUsage();
		}

		for (long i = 0; i < nAttributeCount; i++)
		{
			IManageableMemoryPtr ipManageableMemory = ipSubAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI36015", ipManageableMemory != __nullptr);

			ipManageableMemory->ReportMemoryUsage();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36016");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_Attribute;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// check m_bDirty flag first, if it's not dirty then
		// check all objects owned by this object
		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		if (!m_bDirty)
		{
			IPersistStreamPtr ipPersistStream;
			if ( m_ipAttributeSplitter != __nullptr )
			{
				ipPersistStream = m_ipAttributeSplitter;
				if (ipPersistStream==__nullptr)
				{
					throw UCLIDException("ELI07564", "Object does not support persistence!");
				}
				hr = ipPersistStream->IsDirty();
				if (hr == S_OK)
				{
					return hr;
				}
			}
			
			ipPersistStream = m_ipAttributeValue;
			if (ipPersistStream==__nullptr)
			{
				throw UCLIDException("ELI07565", "Object does not support persistence!");
			}
			hr = ipPersistStream->IsDirty();
			
			if ( m_ipInputValidator != __nullptr )
			{
				ipPersistStream = m_ipInputValidator;
				if (ipPersistStream==__nullptr)
				{
					throw UCLIDException("ELI07566", "Object does not support persistence!");
				}
				hr = ipPersistStream->IsDirty();
				if (hr == S_OK)
				{
					return hr;
				}
			}
			
			if ( m_ipSubAttributes != __nullptr )
			{
				ipPersistStream = m_ipSubAttributes;
				if (ipPersistStream==__nullptr)
				{
					throw UCLIDException("ELI07567", "Object does not support persistence!");
				}
				hr = ipPersistStream->IsDirty();
			}
			return hr;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07568");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();
		
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), __nullptr);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, __nullptr);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;
		
		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI07626", "Unable to load newer Attribute!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Flags to indicate if the object is null default to true so will 
		// only be read if value read from file is false indicating there is an object
		bool bIsSplitterNull = true;
		bool bIsValidatorNull = true;
		bool bIsSubAttributesNull = true;
		bool bIsDataObjectNull = true;

		if ( nDataVersion >= 1)
		{
			dataReader >> m_strAttributeName;
			dataReader >> m_strAttributeType;
			// Read flags to indicate if that object is null
			dataReader >> bIsSplitterNull;
			dataReader >> bIsValidatorNull;
			dataReader >> bIsSubAttributesNull;
		}

		// If this was saved as version 2 or greater, check to see whether a DataObject was saved.
		if ( nDataVersion >= 2)
		{
			dataReader >> bIsDataObjectNull;
		}

		// Separately read the value finding rule object from the stream

		IPersistStreamPtr ipObj;
		// Read Splitter from stream if not __nullptr
		if ( !bIsSplitterNull )
		{
			readObjectFromStream(ipObj, pStream, "ELI09943");
			if (ipObj == __nullptr)
			{
				throw UCLIDException( "ELI07575", 
					"Splitter object could not be read from stream!" );
			}
			m_ipAttributeSplitter = ipObj;
		}
		else
		{
			m_ipAttributeSplitter = __nullptr;
		}

		// Read Attribute Value from stream
		readObjectFromStream(ipObj, pStream, "ELI09944");
		if (ipObj == __nullptr)
		{
			throw UCLIDException( "ELI07576", 
				"Attribute Value object could not be read from stream!" );
		}
		m_ipAttributeValue = ipObj;

		// Read Input Validator from stream if not __nullptr
		if ( !bIsValidatorNull )
		{
			readObjectFromStream(ipObj, pStream, "ELI09945");
			if (ipObj == __nullptr)
			{
				throw UCLIDException( "ELI07577", 
					"Input Validator object could not be read from stream!" );
			}
			m_ipInputValidator = ipObj;
		}
		else
		{
			m_ipInputValidator = __nullptr;
		}

		// Read Sub Attributes from stream not __nullptr
		if ( !bIsSubAttributesNull )
		{
			readObjectFromStream(ipObj, pStream, "ELI09946");
			if (ipObj == __nullptr)
			{
				throw UCLIDException( "ELI07578", 
					"Value Finding Rule object could not be read from stream!" );
			}
			m_ipSubAttributes = ipObj;
		}
		else
		{
			m_ipSubAttributes = __nullptr;
		}

		// Read the data object from the stream if not __nullptr
		if ( !bIsDataObjectNull )
		{
			readObjectFromStream(ipObj, pStream, "ELI24403");
			if (ipObj == __nullptr)
			{
				throw UCLIDException( "ELI24404", 
					"Attribute data object could not be read from stream!");
			}

			m_ipDataObject = ipObj;
		}
		else
		{
			m_ipDataObject = __nullptr;
		}
		// Regardless of whether data object was loaded, m_upStowedDataObject from any previously
		// loaded attribute is no longer valid.
		m_upStowedDataObject.reset(__nullptr);

		if (nDataVersion >= 3)
		{
			loadGUID(pStream);
		}
		else
		{
			// This will create a GUID for older versions
			getGUID();
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07570");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;
		
		dataWriter << m_strAttributeName;
		dataWriter << m_strAttributeType;

		// Write flags to stream to indicated if object is null
		bool bIsObjectNull = false;
		if ( m_ipAttributeSplitter == __nullptr )
		{
			bIsObjectNull = true;
		}
		dataWriter << bIsObjectNull;

		bIsObjectNull = false;
		if ( m_ipInputValidator == __nullptr )
		{
			bIsObjectNull = true;
		}
		dataWriter << bIsObjectNull;

		bIsObjectNull = false;
		if ( m_ipSubAttributes == __nullptr )
		{
			bIsObjectNull = true;
		}
		dataWriter << bIsObjectNull;

		bool bIsDataObjectNull = (m_ipDataObject == __nullptr && !m_upStowedDataObject);
		dataWriter << bIsDataObjectNull;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), __nullptr);
		pStream->Write(data.getData(), nDataLength, __nullptr);

		// Write each of the objects to the stream separately
		IPersistStreamPtr ipPersistentObj;

		// Write splitter to stream if not __nullptr
		if ( m_ipAttributeSplitter != __nullptr )
		{
			ipPersistentObj = m_ipAttributeSplitter;
			if (ipPersistentObj == __nullptr)
			{
				throw UCLIDException( "ELI07572", 
					"Attribute Splitter object could not be saved!" );
			}
			writeObjectToStream(ipPersistentObj, pStream, "ELI09900", fClearDirty);
		}

		// Write Attribute Value to stream, this should always have a value
		ipPersistentObj = m_ipAttributeValue;
		if (ipPersistentObj == __nullptr)
		{
			throw UCLIDException( "ELI07573", 
				"Attribute Value object could not be saved!" );
		}
		writeObjectToStream(ipPersistentObj, pStream, "ELI09901", fClearDirty);

		// Write Input Validator to stream if not __nullptr
		if ( m_ipInputValidator != __nullptr )
		{
			ipPersistentObj = m_ipInputValidator;
			if (ipPersistentObj == __nullptr)
			{
				throw UCLIDException( "ELI07574", 
					"Input Validator object could not be saved!" );
			}
			writeObjectToStream(ipPersistentObj, pStream, "ELI09902", fClearDirty);
		}

		// Write Sub Attributes to stream if not __nullptr
		if ( m_ipSubAttributes != __nullptr )
		{
			ipPersistentObj = m_ipSubAttributes;
			if (ipPersistentObj == __nullptr)
			{
				throw UCLIDException( "ELI07579", 
					"Sub Attributes object could not be saved!" );
			}

			writeObjectToStream(ipPersistentObj, pStream, "ELI09903", fClearDirty);
		}

		// Write the data object to the stream if not __nullptr.
		if (!bIsDataObjectNull)
		{
			if (m_ipDataObject != __nullptr)
			{
				ipPersistentObj = m_ipDataObject;
				if (ipPersistentObj == __nullptr)
				{
					throw UCLIDException("ELI24405",
						"Attribute data object could not be saved!");
				}

				writeObjectToStream(ipPersistentObj, pStream, "ELI24406", fClearDirty);
			}
			else
			{
				ASSERT_RUNTIME_CONDITION("ELI45984", m_upStowedDataObject, "Unexpected DataObject state.");

				pStream->Write(m_upStowedDataObject->getData(), m_upStowedDataObject->getLength(), __nullptr);
			}
		}

		saveGUID(pStream);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07569");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::GetSizeMax(ULARGE_INTEGER * pcbSize)
{
	if (pcbSize == __nullptr)
		return E_POINTER;
		
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38460")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::SetGUID(const GUID* pGuid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		setGUID( *pGuid );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI40352")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::StowDataObject(IMiscUtils *pMiscUtils)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (m_ipDataObject != __nullptr)
		{
			m_upStowedDataObject = writeObjToByteStream(m_ipDataObject);
			m_ipDataObject = __nullptr;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45973")
}

//-------------------------------------------------------------------------------------------------
// ICloneIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::raw_CloneIdentifiableObject(IUnknown ** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ICloneIdentifiableObjectPtr ipCloneIdentifiable;
		ipCloneIdentifiable.CreateInstance(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI38472", ipCloneIdentifiable != __nullptr);

		IUnknownPtr ipUnk = this;
		ipCloneIdentifiable->CopyFromIdentifiableObject(ipUnk);

		*pObject= ipCloneIdentifiable.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38470")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttribute::raw_CopyFromIdentifiableObject(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		copyFrom(pObject, true);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI40353")
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
bool CAttribute::containsType(string strType)
{
	// Check for empty string
	if (strType.length() == 0)
	{
		throw UCLIDException( "ELI09268", "Cannot search Attribute Type for an empty string!" );
	}

	char cReserved = '+';

	// Don't bother searching if string contains reserved character because it will never match
	if (strType.find( cReserved ) != string::npos)
	{
		return false;
	}

	// Convert desired string to upper case for case-insensitive comparison
	makeUpperCase( strType );

	// Check Type
	if (!m_strAttributeType.empty())
	{
		// Search for a delimiter
		long lPos = m_strAttributeType.find( cReserved );
		if (lPos != string::npos)
		{
			// Parse Type into individual strings
			vector<string>	vecTypes;
			StringTokenizer st( cReserved );
			st.parse( m_strAttributeType.c_str(), vecTypes );

			// Search each individual Type
			for (unsigned int ui = 0; ui < vecTypes.size(); ui++)
			{
				// Retrieve this type and convert to upper case
				string strThisType = vecTypes[ui];
				makeUpperCase( strThisType );

				// Compare
				if (strThisType == strType)
				{
					return true;
				}
			}
		}
		// NO delimiter, strType must be an exact match
		else
		{
			// Make upper case copy of Type string
			string strUpper = m_strAttributeType;
			makeUpperCase( strUpper );

			if (strUpper == strType)
			{
				return true;
			}
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CAttribute::validateIdentifier(const string& strName)
{
	if (strName == "")
	{
		UCLIDException ue("ELI09508", "Empty Attribute Name / Type!");
	}

	if (strName.find("<") != string::npos || strName.find(">") != string::npos)
	{
		UCLIDException ue("ELI07138", "\"<\" and \">\" are UCLID reserved characters. Please do not use\r\n"
			" any of these characters as part of attribute's Name or Type.");
		ue.addDebugInfo("Invalid Attribute Name / Type", strName);
		throw ue;
	}

	// Attribute names/types should not contain spaces [FlexIDSCore #3690]
	if (!isValidIdentifier(strName))
	{
		UCLIDException ue("ELI13021", "Invalid Identifier in Attribute Name / Type!");
		ue.addDebugInfo("Invalid String", strName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CAttribute::getSubAttributes()
{
	if (m_ipSubAttributes == __nullptr)
	{
		m_ipSubAttributes.CreateInstance(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI26471", m_ipSubAttributes != __nullptr);
	}

	return m_ipSubAttributes;
}
//-------------------------------------------------------------------------------------------------
void CAttribute::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04900", "Attribute" );
}
//-------------------------------------------------------------------------------------------------
void CAttribute::copyFrom(UCLID_AFCORELib::IAttributePtr ipSource, bool bWithCloneIdentifiableObject)
{
	ASSERT_ARGUMENT("ELI38483", ipSource != __nullptr);

	m_strAttributeName = asString(ipSource->Name);

	m_ipAttributeValue = cloneObject("ELI08215", ipSource->Value, bWithCloneIdentifiableObject);

	m_strAttributeType = asString(ipSource->Type);

	// only make a copy if the input validator is not null
	m_ipInputValidator = cloneObject("ELI08216", ipSource->InputValidator, bWithCloneIdentifiableObject);

	// only make a copy if the splitter is not null
	m_ipAttributeSplitter = cloneObject("ELI08217", ipSource->AttributeSplitter, bWithCloneIdentifiableObject);

	m_ipSubAttributes = cloneObject("ELI19119", ipSource->SubAttributes, bWithCloneIdentifiableObject);

	// only make a copy if the data object is not null
	m_ipDataObject = cloneObject("ELI24913", ipSource->DataObject, bWithCloneIdentifiableObject);

	// if the ICloneIdentifiableObject should be used and the IdentifiableObject interface is implemented
	// copy the GUID from the source
	if (bWithCloneIdentifiableObject)
	{
		// Copy the GUID
		IIdentifiableObjectPtr ipIdentifiable(ipSource);
		if (ipIdentifiable != __nullptr)
		{
			setGUID(ipIdentifiable->InstanceGUID);
		}
	}
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CAttribute::getMiscUtils()
{
	// check if a MiscUtils object has all ready been created
	if (!m_ipMiscUtils)
	{
		// create MiscUtils object
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI45982", m_ipMiscUtils != __nullptr);
	}

	return m_ipMiscUtils;
}