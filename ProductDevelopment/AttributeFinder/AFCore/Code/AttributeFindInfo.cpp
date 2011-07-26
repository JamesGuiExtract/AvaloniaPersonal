
#include "stdafx.h"
#include "AFCore.h"
#include "AFInternalUtils.h"
#include "AttributeFindInfo.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version 6: Added IgnoreAttributeSplitterErrors
const unsigned long gnCurrentVersion = 6;

//-------------------------------------------------------------------------------------------------
// CAttributeFindInfo
//-------------------------------------------------------------------------------------------------
CAttributeFindInfo::CAttributeFindInfo()
: m_bStopSearchingWhenValueFound(false)
, m_ipAttributeRules(__nullptr)
, m_ipInputValidator(__nullptr)
, m_bIgnoreAttributeSplitterErrors(false)
, m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CAttributeFindInfo::~CAttributeFindInfo()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16301");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeFindInfo,
		&IID_ICopyableObject,
		&IID_IPersistStream,
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
// IAttributeFindInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::get_InputValidator(IObjectWithDescription **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// if the InputValidator object-with-description object has not yet
		// been created, do so now..
		if (m_ipInputValidator == __nullptr)
		{
			m_ipInputValidator.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI04617", m_ipInputValidator != __nullptr);
		}

		IObjectWithDescriptionPtr ipShallowCopy = m_ipInputValidator;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04389")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::put_InputValidator(IObjectWithDescription *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipInputValidator = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04388")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::get_AttributeRules(IIUnknownVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// if the AttributeRules object has not yet been created, create it.
		if (m_ipAttributeRules == __nullptr)
		{
			m_ipAttributeRules.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI04394", m_ipAttributeRules != __nullptr);
		}

		IIUnknownVectorPtr ipShallowCopy = m_ipAttributeRules;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04391")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::put_AttributeRules(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// update the internal value as long as newVal != __nullptr
		if (newVal == __nullptr)
		{
			throw UCLIDException("ELI04395", "Invalid object!");
		}

		m_ipAttributeRules = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04390")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::get_StopSearchingWhenValueFound(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = m_bStopSearchingWhenValueFound ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04393")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::put_StopSearchingWhenValueFound(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bStopSearchingWhenValueFound = (newVal == VARIANT_TRUE);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04392")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::ExecuteRulesOnText(IAFDocument* pAFDoc, 
													IProgressStatus* pProgressStatus,
													IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Wrap the pProgressStatus in a smart pointer
		IProgressStatusPtr ipProgressStatus(pProgressStatus);

		// Determine the number of enabled attribute rules
		long nNumEnabledAttributeRules = getNumEnabledAttributeRules();

		// Determine whether an enabled splitter object exists
		bool bEnabledSplitterExists = enabledSplitterExists();

		// Progress related constants
		// The constants below are weighted with the assumption that running an attribute rule takes
		// approximately twice as long as it takes to split all attributes found by that attribute rule.
		const long nNUM_PROGRESS_ITEMS_PER_ATTRIBUTE_RULE = 2;
		const long nNUM_PROGRESS_ITEMS_ATTRIBUTE_RULES = nNumEnabledAttributeRules * nNUM_PROGRESS_ITEMS_PER_ATTRIBUTE_RULE;
		const long nNUM_PROGRESS_ITEMS_PER_SPLITTER_OPERATION = 1;
		const long nNUM_PROGRESS_ITEMS_SPLITTER = nNumEnabledAttributeRules * nNUM_PROGRESS_ITEMS_PER_SPLITTER_OPERATION;
		long nTOTAL_PROGRESS_ITEMS = nNUM_PROGRESS_ITEMS_ATTRIBUTE_RULES; // Attribute rules are always going to be run
		nTOTAL_PROGRESS_ITEMS += bEnabledSplitterExists ? nNUM_PROGRESS_ITEMS_SPLITTER : 0;

		// Update the progress status
		if (ipProgressStatus)
		{
			ipProgressStatus->InitProgressStatus("Initializing field-level rules execution...", 0, 
				nTOTAL_PROGRESS_ITEMS, VARIANT_TRUE);
		}

		// create a vector object to store all the found attributes
		IIUnknownVectorPtr ipFoundAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI04397", ipFoundAttributes != __nullptr);

		int nNumAttributeRules = m_ipAttributeRules->Size();
		for (int i = 0; i < nNumAttributeRules; i++)
		{
			// get the attribute rule object
			UCLID_AFCORELib::IAttributeRulePtr ipAttributeRule = m_ipAttributeRules->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI04398", ipAttributeRule != __nullptr);

			// If the attribute rule is not enabled, just continue the loop, and skip 
			// processing the current attribute rule
			if (ipAttributeRule->IsEnabled == VARIANT_FALSE)
			{
				continue;
			}

			// Update the progress status
			if (ipProgressStatus)
			{
				string strStatusText = string("Executing field rule ") + asString(i + 1) + 
					string(" of ") + asString(nNumAttributeRules) + "...";
				ipProgressStatus->StartNextItemGroup(strStatusText.c_str(), nNUM_PROGRESS_ITEMS_PER_ATTRIBUTE_RULE);
			}

			// create a smart pointer to the document
			UCLID_AFCORELib::IAFDocumentPtr ipAFDoc(pAFDoc);
			ASSERT_RESOURCE_ALLOCATION("ELI07483", ipAFDoc != __nullptr);

			// execute the attribute rule
			// if the rule throws an exception we don't want to halt processing of this document all
			// together, we just want to log the exception and continue processing with the next rule
			// The behavior will be as if the rule found nothing
			IIUnknownVectorPtr ipAttributes;
			try
			{
				try
				{
					// Create a pointer to the Sub-ProgressStatus object, depending upon whether
					// the caller requested progress information
					IProgressStatusPtr ipSubProgressStatus = (ipProgressStatus == __nullptr) ? 
						__nullptr : ipProgressStatus->SubProgressStatus;

					// Execute the attribute rule
					ipAttributes = ipAttributeRule->ExecuteRuleOnText(ipAFDoc, ipSubProgressStatus);
					ASSERT_RESOURCE_ALLOCATION("ELI04399", ipAttributes != __nullptr);
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10544");
			}
			catch(UCLIDException ue)
			{
				ISpatialStringPtr ipText = ipAFDoc->Text;
				if (ipText != __nullptr)
				{
					string strSourceDoc = ipText->SourceDocName;
					ue.addDebugInfo("File", strSourceDoc);
				}

				// Log the rsd filename currently being executed.
				addCurrentRSDFileToDebugInfo(ue);

				if (ipAttributeRule->IgnoreErrors)
				{
#ifdef _DEBUG
					ue.display();
#else
					ue.log();
#endif
					continue;
				}
				else
				{
					throw ue;
				}
			}

			// get the found attributes, and append
			// the found attributes to our result vector
			int nNumValuesFound = ipAttributes->Size();
			for (int j = 0; j < nNumValuesFound; j++)
			{
				// get each attribute. 
				// Note: when retrieving the attribute from Attribute Rule
				// the attribute only has value associate with it. i.e. there
				// is no attribute name or maybe temp attribute name for
				// the attribute at this time
				UCLID_AFCORELib::IAttributePtr ipAttribute = ipAttributes->At(j);
				ASSERT_RESOURCE_ALLOCATION("ELI19120", ipAttribute != __nullptr);

				// get attribute value as a spatial string first
				// and then get the string value for the attribute
				ISpatialStringPtr ipValue = ipAttribute->Value;
				_bstr_t _bstrValue = ipValue->String;

				// populate the input validator field of the attribute object.
				// NOTE: the name field is expected to be filled in at the 
				// higher scope since this object does not know what the attribute 
				// name is.
				IInputValidatorPtr ipInputValidator = m_ipInputValidator->Object;
				if( m_ipInputValidator->GetEnabled() == VARIANT_TRUE )
				{
					ipAttribute->InputValidator = ipInputValidator;
				}
				else
				{
					ipAttribute->InputValidator = __nullptr;
				}

				try
				{
					try
					{
						// if an attribute splitter has been specified for this
						// attribute, invoke the attribute splitter
						if (bEnabledSplitterExists)
						{
							// Update the progress status
							// [P16:2855] Only initialize the splitter progress group for the first item.
							// Otherwise, we will process multiple groups when we only had progress items 
							// allocated for one group.
							if (ipProgressStatus && j == 0)
							{
								ipProgressStatus->StartNextItemGroup("Executing field splitter rules...", 
									nNUM_PROGRESS_ITEMS_PER_SPLITTER_OPERATION);
							}

							// [P16:2855] Don't allow splitters to update progress unless the way we
							// use progress status's here changes significantly.
							// Since we don't how many attributes we will be splitting before initializing
							// the progress status, we can't know that we have an accurate number
							// steps initialized and therefore don't have enough info to allow splitters
							// to update progress status info
							//IProgressStatusPtr ipSubProgressStatus = (ipProgressStatus == __nullptr) ? 
							//	__nullptr : ipProgressStatus->SubProgressStatus;

							// Get the splitter object
							UCLID_AFCORELib::IAttributeSplitterPtr ipSplitter =
								m_ipAttributeSplitter->Object;
							
							// Execute the split operation
							ipSplitter->SplitAttribute(ipAttribute, ipAFDoc, __nullptr/*ipSubProgressStatus*/);
						}
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32949");
				}
				catch(UCLIDException& ue)
				{
					if (m_bIgnoreAttributeSplitterErrors)
					{
						ue.log();
					}
					else
					{
						throw ue;
					}
				}

				// add the attribute to the vector of found attributes
				ipFoundAttributes->PushBack(ipAttribute);
			}

			// if m_bStopSearchingWhenValueFound is true, then stop 
			// search once first set of attributes are found
			if (m_bStopSearchingWhenValueFound && ipFoundAttributes->Size() > 0)
			{
				break;
			}
		}

		// Update the progress status
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}

		// return the found attributes to the caller
		*pAttributes = ipFoundAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04396")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::get_AttributeSplitter(IObjectWithDescription* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// if the AttributeSplitter object-with-description object has not yet
		// been created, do so now..
		if (m_ipAttributeSplitter == __nullptr)
		{
			m_ipAttributeSplitter.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI05285", m_ipAttributeSplitter != __nullptr);
		}

		IObjectWithDescriptionPtr ipShallowCopy = m_ipAttributeSplitter;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05284")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::put_AttributeSplitter(IObjectWithDescription *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipAttributeSplitter = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05283")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::get_IgnoreAttributeSplitterErrors(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIgnoreAttributeSplitterErrors);
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32950")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::put_IgnoreAttributeSplitterErrors(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIgnoreAttributeSplitterErrors = asCppBool(newVal); 

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32951")
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	*pClassID = CLSID_AttributeFindInfo;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::IsDirty(void)
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
			// Check collection of Attribute Rules
			IPersistStreamPtr ipPersistStream(m_ipAttributeRules);
			if (ipPersistStream==__nullptr)
			{
				throw UCLIDException("ELI04785", "Object does not support persistence!");
			}
			hr = ipPersistStream->IsDirty();
			if (hr == S_OK)
			{
				return hr;
			}

			// Check Input Validator
			ipPersistStream = __nullptr;
			ipPersistStream = m_ipInputValidator;
			if (ipPersistStream==__nullptr)
			{
				throw UCLIDException("ELI04787", "Object does not support persistence!");
			}
			hr = ipPersistStream->IsDirty();
			if (hr == S_OK)
			{
				return hr;
			}

			// Check Attribute Splitter
			ipPersistStream = __nullptr;
			ipPersistStream = m_ipAttributeSplitter;
			if (ipPersistStream == __nullptr)
			{
				throw UCLIDException( "ELI06129", "Object does not support persistence!" );
			}
			hr = ipPersistStream->IsDirty();
			if (hr == S_OK)
			{
				return hr;
			}
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04773");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saved: 
//            data version,
//            whether or not to stop searching when a value was found
//            collection of Attribute Rules
//            collection of Ignore list strings
//            Input Validator object with description
// Version 2:
//   * Additionally saved:
//            Attribute Splitter object with description
// Version 3:
//   * Additionally saved:
//            Document Preprocessor object with description
// Version 4:
//   * Removed from save:
//            collection of Ignore list strings
STDMETHODIMP CAttributeFindInfo::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license state
		validateLicense();

		// Reset all the member variables
		m_bStopSearchingWhenValueFound = false;
		m_ipAttributeRules = __nullptr;
		m_ipInputValidator = __nullptr;
		m_ipAttributeSplitter = __nullptr;

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
			UCLIDException ue( "ELI07625", "Unable to load newer AttributeFindInfo." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bStopSearchingWhenValueFound;
		}

		if (nDataVersion >= 6)
		{
			dataReader >> m_bIgnoreAttributeSplitterErrors;
		}

		// Separately read attribute rules from the stream
		IPersistStreamPtr ipObj;
		readObjectFromStream(ipObj, pStream, "ELI09947");
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI04575", "Attribute Rules collection could not be read from stream!");
		}
		m_ipAttributeRules = ipObj;

		if (nDataVersion < 4)
		{
			// Separately read ignore values list from the stream
			ipObj = __nullptr;
			readObjectFromStream(ipObj, pStream, "ELI09948");
			if (ipObj == __nullptr)
			{
				throw UCLIDException( "ELI04576", 
					"Ignore Values list could not be read from stream!" );
			}

			// Check item count
			IVariantVectorPtr	ipIgnore( ipObj );

			long lSize = ipIgnore->Size;
			if (lSize > 0)
			{
				// Format the error message
				string	strText( "The Ignore Values collection is no longer supported directly.  The list includes: " );

				// Add previous items
				string strItem;
				for (int i = 0; i < lSize - 1; i++)
				{
					// Get this item
					strItem = asString( _bstr_t(ipIgnore->GetItem( i )));

					// Add it to the string
					strText = strText + strItem + string( ", " );
				}

				// Add the last item
				strItem = asString(_bstr_t( ipIgnore->GetItem( lSize - 1 )));
				strText = strText + strItem + string( "." );

				// Display the Message Box
				MessageBox( NULL, strText.c_str(), "Warning", MB_OK | MB_ICONWARNING );
			}

			// Ignore list is no longer in use
		}

		// Separately read the input validator object-with-description from the stream
		ipObj = __nullptr;
		readObjectFromStream(ipObj, pStream, "ELI09949");
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI04577", "Input Validator object could not be read from stream!");
		}
		m_ipInputValidator = ipObj;

		// if the version # is 2 or higher, then read the
		// AttributeSplitter object-with-description in
		if (nDataVersion >= 2)
		{
			ipObj = __nullptr;
			readObjectFromStream(ipObj, pStream, "ELI09950");
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI05287", "AttributeSplitter object could not be read from stream!");
			}
			m_ipAttributeSplitter = ipObj;
		}

		if (nDataVersion >= 3 && nDataVersion <=4)
		{
			// have a dummy object to read the doc preprocessor, which
			// was removed from version 5 and beyond
			ipObj = __nullptr;
			readObjectFromStream(ipObj, pStream, "ELI09951");
		}

		// Clear the dirty flag as we just loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04574");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_bStopSearchingWhenValueFound;
		dataWriter << m_bIgnoreAttributeSplitterErrors;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), __nullptr );
		pStream->Write( data.getData(), nDataLength, __nullptr );

		// Separately write attribute rules to the stream
		IPersistStreamPtr ipPersistentObj;
		ipPersistentObj = m_ipAttributeRules;
		if (ipPersistentObj == __nullptr)
		{
			throw UCLIDException("ELI04403", "Attribute Rules collection does not support persistence!");
		}
		writeObjectToStream(ipPersistentObj, pStream, "ELI09904", fClearDirty);

		// Separately write the input validator object-with-description to the stream
		ipPersistentObj = getValidator();
		if (ipPersistentObj == __nullptr)
		{
			throw UCLIDException("ELI04565", "Input Validator object does not support persistence!");
		}
		writeObjectToStream(ipPersistentObj, pStream, "ELI09905", fClearDirty);

		// Separately write the AttributeSplitter object-with-description to the stream
		ipPersistentObj = getSplitter();
		if (ipPersistentObj == __nullptr)
		{
			throw UCLIDException("ELI05286", "AttributeSplitter object does not support persistence!");
		}
		writeObjectToStream(ipPersistentObj, pStream, "ELI09906", fClearDirty);

		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07305");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate license first
		validateLicense();

		// Create the other AttributeFindInfo object
		UCLID_AFCORELib::IAttributeFindInfoPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08331", ipSource != __nullptr);

		// Set this object's StopSearchingWhenValueFound property
		m_bStopSearchingWhenValueFound = asCppBool(ipSource->GetStopSearchingWhenValueFound());
		m_bIgnoreAttributeSplitterErrors = asCppBool(ipSource->IgnoreAttributeSplitterErrors);

		// Set the other object's Input Validator
		IObjectWithDescriptionPtr ipIVTemp = ipSource->GetInputValidator();
		if (ipIVTemp != __nullptr)
		{
			ICopyableObjectPtr ipCopyObj(ipIVTemp);
			ASSERT_RESOURCE_ALLOCATION("ELI08332", ipCopyObj != __nullptr);
			m_ipInputValidator = ipCopyObj->Clone();
		}

		IObjectWithDescriptionPtr ipAttributeSplitterTemp = ipSource->GetAttributeSplitter();
		if (ipAttributeSplitterTemp != __nullptr)
		{
			ICopyableObjectPtr ipCopyObj(ipAttributeSplitterTemp);
			ASSERT_RESOURCE_ALLOCATION("ELI08333", ipCopyObj != __nullptr);
			m_ipAttributeSplitter = ipCopyObj->Clone();
		}

		IIUnknownVectorPtr ipRulesTemp = ipSource->GetAttributeRules();
		if (ipRulesTemp != __nullptr)
		{
			ICopyableObjectPtr ipCopyObj(ipRulesTemp);
			ASSERT_RESOURCE_ALLOCATION("ELI08334", ipCopyObj != __nullptr);
			m_ipAttributeRules = ipCopyObj->Clone();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08335");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate license first
		validateLicense();

		// Create a new IAttributeFindInfo object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_AttributeFindInfo );
		ASSERT_RESOURCE_ALLOCATION("ELI04666", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04662");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFindInfo::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
IObjectWithDescriptionPtr CAttributeFindInfo::getValidator()
{
	try
	{
		// if the InputValidator object-with-description object has not yet
		// been created, do so now..
		if (m_ipInputValidator == __nullptr)
		{
			m_ipInputValidator.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI16926", m_ipInputValidator != __nullptr);
		}

		return m_ipInputValidator;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16923");
}
//-------------------------------------------------------------------------------------------------
IObjectWithDescriptionPtr CAttributeFindInfo::getSplitter()
{
	try
	{
		// if the AttributeSplitter object-with-description object has not yet
		// been created, do so now..
		if (m_ipAttributeSplitter == __nullptr)
		{
			m_ipAttributeSplitter.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI16925", m_ipInputValidator != __nullptr);
		}

		return m_ipAttributeSplitter;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16924");
}
//-------------------------------------------------------------------------------------------------
void CAttributeFindInfo::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04903", "Attribute Find Info" );
}
//-------------------------------------------------------------------------------------------------
long CAttributeFindInfo::getNumEnabledAttributeRules()
{
	long nCount = 0;

	// Determine the number of enabled attribute rules
	int nNumAttributeRules = m_ipAttributeRules->Size();
	for (int i = 0; i < nNumAttributeRules; i++)
	{
		// get the attribute rule object
		UCLID_AFCORELib::IAttributeRulePtr ipAttributeRule = m_ipAttributeRules->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI16104", ipAttributeRule != __nullptr);

		// If the attribute rule is not enabled, just continue the loop, and skip 
		// processing the current attribute rule
		if (ipAttributeRule->IsEnabled == VARIANT_TRUE)
		{
			nCount++;
		}
	}

	return nCount;
}
//-------------------------------------------------------------------------------------------------
bool CAttributeFindInfo::enabledSplitterExists()
{
	return (m_ipAttributeSplitter != __nullptr) && (m_ipAttributeSplitter->Object != __nullptr) &&
		(m_ipAttributeSplitter->GetEnabled() == VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
