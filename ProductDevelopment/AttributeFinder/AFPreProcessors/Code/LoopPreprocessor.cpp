// LoopPreprocessor.cpp : Implementation of CLoopPreprocessor

#include "stdafx.h"
#include "LoopPreprocessor.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CLoopPreprocessor
//-------------------------------------------------------------------------------------------------
CLoopPreprocessor::CLoopPreprocessor()
: m_ipPreprocessor(NULL),
m_ipCondition(NULL),
m_nIterations(0),
m_bConditionValue(true),
m_bLogExceptionForMaxIterations(false),
m_eLoopType(kDoLoop)
{
}
//-------------------------------------------------------------------------------------------------
CLoopPreprocessor::~CLoopPreprocessor()
{
	try
	{
		m_ipPreprocessor = __nullptr;
		m_ipCondition	 = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24114");
}
//-------------------------------------------------------------------------------------------------
HRESULT CLoopPreprocessor::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessor::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] =
		{
			&IID_ILoopPreprocessor,
			&IID_IDocumentPreprocessor,
			&IID_IPersistStream,
			&IID_ICategorizedComponent,
			&IID_ISpecifyPropertyPages,
			&IID_ICopyableObject,
			&IID_IMustBeConfiguredObject,
			&IID_ILicensedComponent
		};
		for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i], riid))
			{
				return S_OK;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24115")
	
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IDocumentPreprocessor Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::raw_Process(IAFDocument * pDocument, IProgressStatus * pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Create local Document pointer
		IAFDocumentPtr ipAFDoc( pDocument );

		// Assert arguments
		ASSERT_ARGUMENT("ELI24116", ipAFDoc != __nullptr);

		// Set the Pre Processor from the Object with description
		IDocumentPreprocessorPtr ipPreProcessor = m_ipPreprocessor->Object;
		ASSERT_RESOURCE_ALLOCATION("ELI24117", ipPreProcessor != __nullptr);

		// Set the Condition from the Object with descripiton if not kForLoop
		IAFConditionPtr ipCondition = __nullptr;
		if (m_eLoopType != kForLoop)
		{
			ipCondition = m_ipCondition->Object;
			ASSERT_RESOURCE_ALLOCATION("ELI24118", ipCondition != __nullptr);
		}

		// Initialize the number of iterations through the loop
		long nIterations = 0;

		// Initialize the Condition met flag
		bool bConditionMet = true;

		// Double try...catch to add the number of iterations to any exception that is thrown
		try
		{
			try
			{
				// if the loop is a while loop the condition needs to be tested
				if (m_eLoopType == kWhileLoop)
				{
					bConditionMet = asCppBool(ipCondition->ProcessCondition(ipAFDoc)) == m_bConditionValue;
				}

				// Loop while the condition is met or loop type is kForLoop
				// and the number of iterations has not been reached
				while ((bConditionMet || m_eLoopType == kForLoop) && nIterations < m_nIterations)
				{
					// Run the PreProcessor after running the rule
					ipPreProcessor->Process(ipAFDoc, pProgressStatus);

					// Increment the number of iterations
					nIterations++;

					// Check if conditions are met
					if (m_eLoopType != kForLoop)
					{
						bConditionMet = asCppBool(ipCondition->ProcessCondition(ipAFDoc)) == m_bConditionValue;
					}
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24525");
		}
		catch(UCLIDException &ue)
		{
			ue.addDebugInfo("Loop Iteration", nIterations);
			throw ue;
		}

		// Check to see if exception should be logged
		if (m_eLoopType != kForLoop && m_bLogExceptionForMaxIterations && bConditionMet )
		{
			UCLIDException ue("ELI24119", "Loop did not meet condition before maximum iterations.");
			ue.addDebugInfo("MaxIterations", m_nIterations);

			// Get the currently running Ruleset
			IRuleExecutionEnvPtr ipRuleEnv(CLSID_RuleExecutionEnv);
			
			// Only add the debug info if the RuleExecutionEnv was successfully created
			if (ipRuleEnv != __nullptr)
			{
				ue.addDebugInfo("RuleFile", asString(ipRuleEnv->GetCurrentRSDFileName()));
			}

			// Get the spatial string from the document
			ISpatialStringPtr ipText = ipAFDoc->Text;

			// Get the current file being processed
			if ( ipText != __nullptr )
			{
				ue.addDebugInfo("InputFile", asString(ipText->SourceDocName));
			}
			ue.log();
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24120");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24121", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Loop preprocessor").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24122")
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::raw_Clone(LPUNKNOWN * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI24123", pObject != __nullptr);

		ICopyableObjectPtr ipObjCopy(CLSID_LoopPreprocessor);
		ASSERT_RESOURCE_ALLOCATION("ELI24124", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24125");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::raw_CopyFrom(LPUNKNOWN pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();
		
		ASSERT_ARGUMENT("ELI24126", pObject != __nullptr);

		UCLID_AFPREPROCESSORSLib::ILoopPreprocessorPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI24127", ipSource!=NULL);

		// Copy the Properties
		m_nIterations = ipSource->Iterations;
		m_bConditionValue = asCppBool(ipSource->ConditionValue);
		m_bLogExceptionForMaxIterations = asCppBool(ipSource->LogExceptionForMaxIterations);
		m_eLoopType = (ELoopType)ipSource->LoopType;

		// Clone Condition if it exists.
		ICopyableObjectPtr ipCopyObj = ipSource->Condition;
		if (m_eLoopType != kForLoop)
		{
			if (ipCopyObj != __nullptr)
			{
				m_ipCondition = ipCopyObj->Clone();
			}
		}

		// Clone the preprocessor.
		ipCopyObj = ipSource->Preprocessor;
		if (ipCopyObj != __nullptr)
		{
			m_ipPreprocessor = ipCopyObj->Clone();
		}

		return S_OK;	
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24128");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
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
// IMustBeConfiguredObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::raw_IsConfigured(VARIANT_BOOL * pbConfigured)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI24129", pbConfigured != __nullptr);

		bool bConfigured;
		
		IMustBeConfiguredObjectPtr ipConfigObj;

		// Iterations must always be > 0
		bConfigured = m_nIterations > 0;

		// If the type is not a for loop make sure there is a condition
		if (bConfigured && m_eLoopType != kForLoop)
		{
			// Condition has been set
			if (m_ipCondition != __nullptr)
			{
				ipConfigObj = m_ipCondition->Object;

				// Is configured only if the Condition object is not null and if the 
				// it implements IMustBeConfiguredObject and it is configured
				bConfigured = m_ipCondition->Object != __nullptr;
				bConfigured = bConfigured && (ipConfigObj == __nullptr || 
					asCppBool(ipConfigObj->IsConfigured()));
			}
			else
			{
				bConfigured = false;
			}
		}
		
		// Check if the preprocessor is configured
		if (bConfigured && m_ipPreprocessor != __nullptr)
		{
			// Get the IMustBeConfigured object
			ipConfigObj = m_ipPreprocessor->Object;
			
			// Is configured only if the Preprocessor object is not null and if the 
			// it implements IMustBeConfiguredObject and it is configured
			bConfigured = m_ipPreprocessor->Object != __nullptr;
			bConfigured = bConfigured && (ipConfigObj == __nullptr || 
				asCppBool(ipConfigObj->IsConfigured()));
		}
		else 
		{
			bConfigured = false;
		}

		*pbConfigured = asVariantBool(bConfigured);
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24130");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24131", pClassID != __nullptr);

		*pClassID = CLSID_LoopPreprocessor;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24132");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI24133", pStream != __nullptr);
		
		// Reset all of the values
		m_ipCondition = __nullptr;
		m_ipPreprocessor = __nullptr;
		m_nIterations = 0;
		m_bLogExceptionForMaxIterations = false;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;
		
		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI24134", "Unable to load newer loop preprocessor!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}
		
		// Get the type of loop
		long nTemp = 0;
		dataReader >> nTemp;
		m_eLoopType = (ELoopType)nTemp;

		dataReader >> m_nIterations;
		dataReader >> m_bLogExceptionForMaxIterations;
		dataReader >> m_bConditionValue;

		IPersistStreamPtr ipObj;

		readObjectFromStream(ipObj, pStream, "ELI24135");
		m_ipPreprocessor = ipObj;
		ASSERT_RESOURCE_ALLOCATION("ELI24136", m_ipPreprocessor != __nullptr);
		
		// Only get the Condition if the loop type is not for
		if (m_eLoopType != kForLoop)
		{
			readObjectFromStream(ipObj, pStream, "ELI24137");
			m_ipCondition = ipObj;
			ASSERT_RESOURCE_ALLOCATION("ELI24138", m_ipCondition != __nullptr);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24139");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

		try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI24140", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCurrentVersion;

		// Write the values
		dataWriter << (long)m_eLoopType;
		dataWriter << m_nIterations;
		dataWriter << m_bLogExceptionForMaxIterations;
		dataWriter << m_bConditionValue;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);
		
		// Write PreProcessor
		IPersistStreamPtr ipObj = m_ipPreprocessor;
		ASSERT_RESOURCE_ALLOCATION("ELI24141", ipObj != __nullptr);
		writeObjectToStream(ipObj, pStream, "ELI24142", fClearDirty);

		// Only write the condition if the value is valid
		if ( m_eLoopType != kForLoop )
		{
			// Write Condition
			ipObj = m_ipCondition;
			ASSERT_RESOURCE_ALLOCATION("ELI24143", ipObj != __nullptr);
			writeObjectToStream(ipObj, pStream, "ELI24144", fClearDirty);
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24145");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILoopPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::get_Preprocessor(IObjectWithDescription ** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI24146", pVal != __nullptr);

		// If Preprocessor is NULL return NULL otherwise return a shallow copy
		if (m_ipPreprocessor == __nullptr)
		{
			*pVal = NULL;
		}
		else
		{
			IObjectWithDescriptionPtr ipShallowCopy = m_ipPreprocessor;
			*pVal = ipShallowCopy.Detach();
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24147");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::put_Preprocessor(IObjectWithDescription * newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		
		// Set the new Preprocessor value (can be NULL)
		m_ipPreprocessor = newVal;
		m_bDirty = true;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24148");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::get_Condition(IObjectWithDescription ** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI24149", pVal != __nullptr);

		// If the condition is null return the null value
		if ( m_ipCondition == __nullptr )
		{
			*pVal = NULL;
		}
		else
		{
			// Make a shallow copy of the condition object and return
			IObjectWithDescriptionPtr ipShallowCopy = m_ipCondition;
			*pVal = ipShallowCopy.Detach();
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24150");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::put_Condition(IObjectWithDescription * newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Set the new Condition( this can be NULL)
		m_ipCondition = newVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24151");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::get_ConditionValue( VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI24152", pVal != __nullptr);

		// Return the Conditon Value
		*pVal = asVariantBool(m_bConditionValue);
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24153");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::put_ConditionValue( VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Convert to bool
		bool bNewValue = asCppBool(newVal);

		// Only mark as dirty if the value is different
		m_bDirty = m_bDirty || m_bConditionValue != bNewValue;

		// Set new value for the condition value
		m_bConditionValue = bNewValue;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24154");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::get_LogExceptionForMaxIterations( VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI24155", pVal != __nullptr);

		// Return the Log exception flag
		*pVal = asVariantBool(m_bLogExceptionForMaxIterations);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24156");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::put_LogExceptionForMaxIterations( VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Convet to bool
		bool bNewValue = asCppBool(newVal);
		
		// Only mark as dirty if the value is different
		m_bDirty = m_bDirty || m_bConditionValue != bNewValue;

		// Set the Log exception flag
		m_bLogExceptionForMaxIterations = bNewValue;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24157");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::get_Iterations( long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI24158", pVal != __nullptr);

		// Return the number of iterations
		*pVal = m_nIterations;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24159");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::put_Iterations( long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Only mark as dirty if the value has changed
		m_bDirty = m_bDirty || m_nIterations != newVal;

		// Set the number of iterations
		m_nIterations = newVal;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24160");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::get_LoopType( ELoopType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI24161", pVal != __nullptr);

		// Return the loop type
		*pVal = m_eLoopType;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24162");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessor::put_LoopType( ELoopType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Only mark as dirty if the value has changed
		m_bDirty = m_bDirty || m_eLoopType != newVal;

		// Set the Loop type
		m_eLoopType = newVal;

		// If loop type is kForLoop reset the condition
		if (m_eLoopType == kForLoop)
		{
			m_ipCondition = __nullptr;

			// Log Exception for max iterations should be false if loop type is for
			m_bLogExceptionForMaxIterations = false;
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24163");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessor::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI24164", "Loop Preprocessor" );
}
//-------------------------------------------------------------------------------------------------

