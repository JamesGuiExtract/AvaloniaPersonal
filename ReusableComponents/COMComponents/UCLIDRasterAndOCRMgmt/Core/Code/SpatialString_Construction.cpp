// SpatialString.cpp : Implementation of CSpatialString string construction
#include "stdafx.h"
#include "SpatialString.h"
#include "UCLIDRasterAndOCRMgmt.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <CompressionEngine.h>

//-------------------------------------------------------------------------------------------------
// CSpatialString
//-------------------------------------------------------------------------------------------------
CSpatialString::CSpatialString()
: m_strString(""), 
  m_eMode(kNonSpatialMode), 
  m_strSourceDocName(""),
  m_strOCREngineVersion(""),
  m_ipMemoryManager(__nullptr),
  m_ipOCRParameters(__nullptr)
{
}
//-------------------------------------------------------------------------------------------------
CSpatialString::~CSpatialString()
{
	try
	{
		// Clear the page info map
		m_ipPageInfoMap = __nullptr;
		
		// If memory usage has been reported, report that this instance is no longer using any
		// memory.
		RELEASE_MEMORY_MANAGER(m_ipMemoryManager, "ELI36091");
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16539");
}

//-------------------------------------------------------------------------------------------------
// ISpatialString - String construction
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::CreateFromLines(IIUnknownVector* pLines)
{

	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		IIUnknownVectorPtr ipLines(pLines);
		ASSERT_RESOURCE_ALLOCATION("ELI15088", ipLines != __nullptr);

		// Reset everything
		reset(true, true);

		// Flags to determine the type of object we are creating
		bool bOneSpatial = false;
		bool bAllSpatial = true;
		bool bAllHybrid = true;

		vector<UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr> vecLines;

		// Have to find the mode before appending the values otherwise append will not
		// know how to handle the values for each type.
		long lLineCount = ipLines->Size();
		vecLines.reserve(lLineCount);
		for (long i = 0; i < lLineCount; i++)
		{
			// Get each line
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLine = ipLines->At(i);
			ASSERT_RESOURCE_ALLOCATION( "ELI14777", ipLine != __nullptr);
			
			// Place each line in the vector of lines
			vecLines.push_back(ipLine);

			// If any single line is non-spatial, then the lines
			// cannot be all hybrid or all spatial.
			UCLID_RASTERANDOCRMGMTLib::ESpatialStringMode eSourceMode = ipLine->GetMode();

			if ( eSourceMode == kNonSpatialMode )
			{
				bAllHybrid = false;
				bAllSpatial = false;
			}
			// If at least one line is Hybrid or Spatial, then the OneSpatial bool is set.
			else if( eSourceMode == kHybridMode )
			{
				bOneSpatial = true;
				bAllSpatial = false;
			}

			else if( eSourceMode == kSpatialMode )
			{
				bOneSpatial = true;
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI15378");
			}
		}//end for

		// Determine which mode this object will be in. If all lines are non-spatial, then
		// the object will be kNonSpatialMode. If at least 1 line is spatial, the object
		// will be kHybridMode. If all the lines are spatial, the object will be kSpatialMode.
		if( bAllSpatial )
		{
			m_eMode = kSpatialMode;
		}
		// If at least one is spatial OR all were hybrid, mode is hybrid.
		else if( bOneSpatial || bAllHybrid)
		{
			m_eMode = kHybridMode;
		}
		// If none of the above are set, all items were non-spatial
		else
		{
			m_eMode = kNonSpatialMode;
		}		

		// Now this object's insert method can handle each line correctly since it knows
		// what mode it is in.
		for (long i = 0; i < lLineCount; i++)
		{
			// Get each line
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLine = vecLines[i];
			ASSERT_RESOURCE_ALLOCATION( "ELI14804", ipLine != __nullptr);

			// Append the line to this object
			append(ipLine);
			
			// Don't append a new line after the last line and 
			if (i < lLineCount-1 )
			{
				appendString("\r\n");
			}
		}//end for
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08819");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::AddRasterZones(IIUnknownVector *pVal, ILongToObjectMap* pPageInfoMap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check arguments
		IIUnknownVectorPtr ipVal(pVal);
		ASSERT_ARGUMENT("ELI25769", ipVal != __nullptr);

		// Validate license first
		validateLicense();

		// Wrap the page info map as a smart pointer (NULL is a valid value for
		// this argument)
		ILongToObjectMapPtr ipPageInfoMap(pPageInfoMap);

		// Add the raster zones (note the string will now be hybrid no matter what
		// its mode was previously)
		addRasterZones(ipVal, ipPageInfoMap);

		// Update the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14801");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::CreatePseudoSpatialString(IRasterZone *pZone, BSTR bstrText, 
	BSTR bstrSourceDocName, ILongToObjectMap *pPageInfoMap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone(pZone);
		ASSERT_ARGUMENT("ELI19769", ipZone != __nullptr);
		string strText = asString(bstrText);
		ASSERT_ARGUMENT("ELI19907", !strText.empty());
		ASSERT_ARGUMENT("ELI20241", pPageInfoMap != __nullptr);

		// Reset everything
		reset(true, true);

		// Set the page info map
		// NOTE: This must be set before the call to getPageBounds.
		m_ipPageInfoMap = pPageInfoMap;
		
		// After being assigned to a SpatialString, the page info map must not be modifed, otherwise
		// it may affect other SpatialStrings that share these page infos.
		m_ipPageInfoMap->SetReadonly();

		// Get the page bounds (for use by GetRectangularBounds)
		ILongRectanglePtr ipPageBounds = getPageBounds(ipZone->PageNumber, true);
		ASSERT_RESOURCE_ALLOCATION("ELI30320", ipPageBounds != __nullptr);

		// Calculate dimensions needed to generate the letter array
		// [FlexIDSCore #3555] - Pass page info map so that bounds are clipped by page dimensions
		ILongRectanglePtr ipBounds = ipZone->GetRectangularBounds(ipPageBounds);
		ASSERT_RESOURCE_ALLOCATION("ELI19908", ipBounds != __nullptr);

		long nTop, nBottom, nLeft, nRight;
		ipBounds->GetBounds(&nLeft, &nTop, &nRight, &nBottom);
		long nRegionWidth = nRight - nLeft;
		long nRegionHeight = nBottom - nTop;
		unsigned long nLetterCount = strText.size();
		float ufCharWidth = (float) nRegionWidth / (float)nLetterCount;
		float ufLeftBound = (float) nLeft;
		long nPageNum = ipZone->PageNumber;
		vector<CPPLetter> vecLetters;

		// Generate letter objects for the ImageRegionText string and populate
		// them with spatial information that will fill the found image region
		for (unsigned long i = 0; i < nLetterCount; i++)
		{
			unsigned short usLetter = (unsigned short) strText[i];

			vecLetters.push_back(CPPLetter(usLetter,
				usLetter,
				usLetter,
				nTop,
				nBottom,
				(unsigned long) ufLeftBound,
				(unsigned long) (ufLeftBound + ufCharWidth),
				(unsigned short) nPageNum,
				false, false, true, (unsigned char) ufCharWidth, 100, 0));

			ufLeftBound += ufCharWidth;
		}

		// set the last character's right bound to the right bound of the region
		vecLetters[nLetterCount - 1].m_ulRight = nRight;

		processLetters(&vecLetters[0], nLetterCount);

		// set the source doc name
		m_strSourceDocName = asString(bstrSourceDocName);

		// Set the mode to spatial
		m_eMode = kSpatialMode;

		// Update the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19768");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::CreateHybridString(IIUnknownVector* pVecRasterZones, BSTR bstrText, 
		BSTR bstrSourceDocName, ILongToObjectMap *pPageInfoMap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI20253", pPageInfoMap != __nullptr);

		// Reset everything except m_strSourceDocName
		reset(false, true);

		// Handle the raster zones parameter
		IIUnknownVectorPtr ipRZones(pVecRasterZones);
		ASSERT_RESOURCE_ALLOCATION( "ELI14814", ipRZones != __nullptr );

		// Put the Raster Zones into this object's raster zone vector if one exists
		long lSize = ipRZones->Size();
		if( lSize > 0)
		{
			m_vecRasterZones.reserve(lSize);

			// Add each raster zone to the vector of raster zones
			for (long i=0; i < lSize; i++)
			{
				UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipRZones->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI25770", ipZone != __nullptr);

				m_vecRasterZones.push_back(ipZone);
			}

			// Update the mode
			m_eMode = kHybridMode;

			m_ipPageInfoMap = pPageInfoMap;
		}
		// If no raster zones exist, set this object to non spatial mode
		else
		{
			m_eMode = kNonSpatialMode;
		}

		// Update the string of this object
		m_strString = asString( bstrText );

		// set the source doc name
		m_strSourceDocName = asString(bstrSourceDocName);

		// Update the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14813");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::CreateNonSpatialString(BSTR bstrText, BSTR bstrSourceDocName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the text
		string strText = asString(bstrText);

		// Check license
		validateLicense();

		// Reset the entire string
		reset(true, true);

		// Set the string to this text
		m_strString = strText;

		// Set the source doc name
		m_strSourceDocName = asString(bstrSourceDocName);

		// Set the mode to non-spatial
		m_eMode = kNonSpatialMode;

		// Update the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25665");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::CreateFromLetterArray(long nNumLetters, void* pLetters,
		BSTR bstrSourceDocName, ILongToObjectMap* pPageInfoMap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check arguments
		ASSERT_ARGUMENT("ELI25771", (pLetters != __nullptr) || (nNumLetters == 0));
		ASSERT_ARGUMENT("ELI25772", pPageInfoMap != __nullptr);
		string strSourceDocName = asString(bstrSourceDocName);
		ASSERT_ARGUMENT("ELI25773", !strSourceDocName.empty());

		// Check license
		validateLicense();

		// Reset the spatial string
		reset(true, true);

		// Copy the source doc name
		m_strSourceDocName = strSourceDocName;

		// Copy the page info map
		m_ipPageInfoMap = pPageInfoMap;

		// After being assigned to a SpatialString, the page info map must not be modifed, otherwise
		// it may affect other SpatialStrings that share these page infos.
		m_ipPageInfoMap->SetReadonly();
		
		// compute the string from the letters, and 
		// downgrade to non-spatial string if necessary.
		processLetters((CPPLetter*)pLetters, nNumLetters);

		// Update the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25667")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::CreateFromILetters(IIUnknownVector* pLetters,
											BSTR bstrSourceDocName, ILongToObjectMap* pPageInfoMap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

		try
	{
		// Check arguments
		IIUnknownVectorPtr ipLetters(pLetters);
		ASSERT_ARGUMENT("ELI46903", ipLetters != __nullptr);
		ASSERT_ARGUMENT("ELI46905", pPageInfoMap != __nullptr);
		string strSourceDocName = asString(bstrSourceDocName);
		ASSERT_ARGUMENT("ELI46906", !strSourceDocName.empty());

		// Check license
		validateLicense();

		// Reset the spatial string
		reset(true, true);

		// Copy the source doc name
		m_strSourceDocName = strSourceDocName;

		// Copy the page info map
		m_ipPageInfoMap = pPageInfoMap;

		// After being assigned to a SpatialString, the page info map must not be modifed, otherwise
		// it may affect other SpatialStrings that share these page infos.
		m_ipPageInfoMap->SetReadonly();
		
		long nCount = ipLetters->Size();
		vector<CPPLetter> vecLetters(nCount);
		for (int i = 0; i < nCount; i++)
		{
			UCLID_RASTERANDOCRMGMTLib::ILetterPtr ipLetter(ipLetters->At(i));
			ASSERT_RESOURCE_ALLOCATION("ELI46904", ipLetter != __nullptr);

			ipLetter->GetCppLetter(&vecLetters[i]);
		}

		processLetters(&vecLetters[0], nCount);

		// Update the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46902")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::ReplaceAndDowngradeToHybrid(BSTR bstrReplacement)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// check if this string is spatial
		if(m_eMode == kSpatialMode)
		{
			// Downgrade the string to a hybrid string
			downgradeToHybrid();
		}
		else if(m_eMode == kNonSpatialMode)
		{
			throw UCLIDException("ELI17164", 
				"Cannot downgrade a non-spatial string to a hybrid string.");
		}
		// else the spatial string is already hybrid

		// replace the text of the hybrid string
		m_strString = asString(bstrReplacement);
		
		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17068");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::ReplaceAndDowngradeToNonSpatial(BSTR bstrReplacement)
{

	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Downgrade the string to non-spatial mode
		downgradeToNonSpatial();

		// Replace the text of the non-spatial string
		m_strString = asString(bstrReplacement);

		// Update the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25774");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::LoadFrom(BSTR strFullFileName, 
									  VARIANT_BOOL bSetDirtyFlagToTrue, 
									  BSTR *pstrOriginalSourceDocName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		CSingleLock lg(&m_criticalSection, TRUE);

		// Default return OriginalSourceDocName path to empty string
		string strOrigSourceDocName = "";

		// if the user is trying to load from a text file, use the reusable
		// functions to read the text from the ASCII file
		string strInputFile = asString(strFullFileName);
		EFileType eFileType = getFileType(strInputFile);
		if (eFileType == kTXTFile || eFileType == kXMLFile || eFileType == kCSVFile || eFileType == kRichTextFile)
		{
			loadTextWithPositionalData(strInputFile, eFileType);
		}
		else if (eFileType == kUSSFile)
		{
			// Load from new-style USS (zip file with individual pages saved as numbered files)
			if (CompressionEngine::isZipFile(strInputFile))
			{
				loadFromArchive(strInputFile);
			}
			else
			{
				loadFromStorageObject(strInputFile, getThisAsCOMPtr());
			}

			////////////////////////////////////////
			// Check for SourceDocName image, prefer
			// image in same location as USS file
			////////////////////////////////////////

			// Get folder for USS file
			string	strFolder = getDirectoryFromFullPath(strInputFile);

			// Get filename without extension
			string	strSourceFile = getFileNameWithoutExtension(strInputFile);

			// Check existence of SourceDocName file in present folder
			string	strNewSource = strFolder + "\\" + strSourceFile;
			// if the new source file exists, replace the existing source doc name
			if (isValidFile(strNewSource))
			{
				// Retain original source doc name
				strOrigSourceDocName = m_strSourceDocName;

				// the actual source doc name
				m_strSourceDocName = strNewSource;
			}
		}
		else
		{
			UCLIDException ue("ELI06797", "Unknown file type!");
			ue.addDebugInfo("strInputFile", strInputFile);
			throw ue;
		}

		// Original source doc name will be returned to the caller
		*pstrOriginalSourceDocName = _bstr_t( strOrigSourceDocName.c_str() ).Detach();

		// mark this object as dirty depending upon bSetDirtyFlagToTrue
		m_bDirty = asCppBool(bSetDirtyFlagToTrue);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06689");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::LoadPageFromFile(BSTR bstrInputFile, long nPage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		string inputFile = asString(bstrInputFile);

		if (CompressionEngine::isZipFile(inputFile))
		{
			string originalSourceDocName;
			loadPagesFromArchive(inputFile, false, &originalSourceDocName, nPage, true);
			if (!originalSourceDocName.empty())
			{
				m_strSourceDocName = originalSourceDocName;
			}
		}
		else
		{
			// Load the entire string and then truncate it to the desired page
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipThis = getThisAsCOMPtr();
			loadFromStorageObject(inputFile, ipThis);
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr page = ipThis->GetSpecifiedPages(nPage, nPage);
			copyFromSpatialString(page);

			// Page info maps for large files are noticeable when looking for memory leaks (1000 page infos is about a MB).
			// These stick around for a while in .NET if ReportMemoryUsage() isn't used everywhere (e.g., if you call GetWords() and forget to report the word spatial strings).
			// To minimize this, rid of the extra pages so the map is smaller
			if (getPageInfoMap()->Size > 1)
			{
				UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr pageInfo = m_ipPageInfoMap->GetValue(nPage);
				m_ipPageInfoMap.CreateInstance(CLSID_LongToObjectMap);
				m_ipPageInfoMap->Set(nPage, pageInfo);
			}
		}

		////////////////////////////////////////
		// Check for SourceDocName image, prefer
		// image in same location as USS file
		////////////////////////////////////////

		// Get folder for USS file
		string	strFolder = getDirectoryFromFullPath(inputFile);

		// Get filename without extension
		string	strSourceFile = getFileNameWithoutExtension(inputFile);

		// Check existence of SourceDocName file in present folder
		string	strNewSource = strFolder + "\\" + strSourceFile;
		// if the new source file exists, replace the existing source doc name
		if (isFileOrFolderValid(strNewSource))
		{
			m_strSourceDocName = strNewSource;
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46767");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::LoadPagesFromFile(BSTR bstrInputFile, IIUnknownVector** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		string inputFile = asString(bstrInputFile);

		////////////////////////////////////////
		// Check for SourceDocName image, prefer
		// image in same location as USS file
		////////////////////////////////////////

		// Get folder for USS file
		string strFolder = getDirectoryFromFullPath(inputFile);

		// Get filename without extension
		string strSourceFile = getFileNameWithoutExtension(inputFile);

		// Check existence of SourceDocName file in present folder
		string strNewSource = strFolder + "\\" + strSourceFile;
		_bstr_t bstrNewSource = get_bstr_t(strNewSource.c_str());
		bool bHasNewSource = isFileOrFolderValid(strNewSource);

		IIUnknownVectorPtr ipPages(CLSID_IUnknownVector);

		if (CompressionEngine::isZipFile(inputFile))
		{
			string originalSourceDocName;
			auto pageMap = loadPagesFromArchive(inputFile, false, &originalSourceDocName);
			for (auto& p : *pageMap)
			{
				auto& ipPage = p.second;

				// if the new source file exists, use that for the SourceDocName
				if (bHasNewSource)
				{
					ipPage->SourceDocName = bstrNewSource;
				}
				else
				{
					ipPage->SourceDocName = get_bstr_t(originalSourceDocName.c_str());
				}

				ipPages->PushBack(ipPage);
			}
		}
		else
		{
			// Load the entire string and then split into pages
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipWhole(CLSID_SpatialString);
			loadFromStorageObject(inputFile, ipWhole);

			ipPages = ipWhole->GetPages(VARIANT_FALSE, "");

			// if the new source file exists, use that for the SourceDocName
			if (bHasNewSource)
			{
				for (long i = 0, size = ipPages->Size(); i < size; i++)
				{
					UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipPage = ipPages->At(i);
					ipPage->SourceDocName = bstrNewSource;
				}
			}
		}
		
		*pVal = ipPages.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI48351");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::AppendToFile(BSTR bstrOutputFile, VARIANT_BOOL vbCheckForExistingPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		string outputFile = asString(bstrOutputFile);
		ASSERT_RUNTIME_CONDITION("ELI46765", isValidFile(outputFile), "File to append to doesn't exist");

		appendToArchive(outputFile, true, asCppBool(vbCheckForExistingPages));
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46766");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::SaveTo(BSTR strFullFileName, VARIANT_BOOL bCompress,
									VARIANT_BOOL bClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		CSingleLock lg(&m_criticalSection, TRUE);

		// if the user is trying to save to a text file,
		// the text associated with this object to the ASCII file
		string stdstrFullFileName = asString(strFullFileName);
		EFileType eFileType = getFileType(stdstrFullFileName);
		if (eFileType == kTXTFile)
		{
			// save the text file to the specified output file name
			// NOTE: saveToTXTFile calls waitForFileAccess internally, no
			// need to call it here
			saveToTXTFile(stdstrFullFileName);

			// clear the dirty bit as requested
			if (bClearDirty == VARIANT_TRUE)
			{
				m_bDirty = false;
			}
		}
		else if (eFileType == kUSSFile)
		{
			// Save this object to the file using the UNC path [FlexIDSCore #3519]
			string strSourceDocName = m_strSourceDocName;
			try
			{
				// https://extract.atlassian.net/browse/ISSUE-12052
				// If there is no source doc name (empty spatial string), don't try to get the UNC
				// path... it will result in an exception.
				if (!strSourceDocName.empty())
				{
					m_strSourceDocName = ::getUNCPath(strSourceDocName);
				}

				if (m_eMode == kNonSpatialMode)
				{
					saveToStorageObject(stdstrFullFileName, getThisAsCOMPtr(), asCppBool(bCompress), asCppBool(bClearDirty));
				}
				else
				{
					IIUnknownVectorPtr pages = getThisAsCOMPtr()->GetPages(VARIANT_FALSE, "");
					savePagesToArchive(stdstrFullFileName, pages, asCppBool(bCompress));
				}
			}
			catch (...)
			{
				// Restore the original source doc name
				m_strSourceDocName = strSourceDocName;

				throw;
			}

			// Restore the original source doc name
			m_strSourceDocName = strSourceDocName;

			// Wait until the file is readable
			waitForFileToBeReadable(stdstrFullFileName);
		}
		else
		{
			UCLIDException ue("ELI06798", "Unknown file type!");
			ue.addDebugInfo("strInputFile", stdstrFullFileName);
			throw ue;
		}

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06692");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// reset all member variables
		reset(true, true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06618");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::LoadFromMultipleFiles(IVariantVector *pvecFiles, BSTR strSourceDocName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Reset the string (including spatial page info and source doc name)
		reset(true, true);

		// store the new value of the source-doc-name attribute
		m_strSourceDocName = asString(strSourceDocName);

		// Wrap the vector in a smart pointer
		IVariantVectorPtr ipvecFiles = pvecFiles;
		ASSERT_RESOURCE_ALLOCATION( "ELI09067", ipvecFiles != __nullptr );

		// Get the number of files and iterate over the vector
		long nNumFiles = ipvecFiles->Size;
		for ( long i = 0; i < nNumFiles; i++)
		{
			// Create a temporary spatial string to append onto the back of this
			// object once it has been loaded from the file
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipText(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION( "ELI09072", ipText != __nullptr );

			// Load the file
			ipText->LoadFrom( _bstr_t(ipvecFiles->GetItem(i)), VARIANT_TRUE );
			
			// Set the source document name to the given source name
			ipText->SourceDocName = strSourceDocName;
				
			if( ipText->HasSpatialInfo() )
			{
				ipText->UpdatePageNumber( i+1 );
			}

			// Add Blank line before the new string is added if at least 1 page has been loaded
			if ( i > 0)
			{
				appendString("\r\n\r\n");
			}

			// Append the new spatial string. This will append any RasterZones that are on the
			// spatial string from the file.
			append(ipText);
		}
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09065");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::CreateFromSpatialStrings(IIUnknownVector *pStrings, VARIANT_BOOL vbInsertPageBreaks)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		IIUnknownVectorPtr ipStrings(pStrings);
		ASSERT_RESOURCE_ALLOCATION("ELI37129", ipStrings != __nullptr);

		// Clear member variables
		// https://extract.atlassian.net/browse/ISSUE-15691
		reset(true, true);

		// If no strings, then nothing to do (string needs to remain in non-spatial mode)
		if (ipStrings->Size() == 0)
		{
			return S_OK;
		}

		// Need to verify that each SpatialString in the vector
		// has mode kSpatialMode and calculate the final size of the string
		long lSize = ipStrings->Size();
		long lTotalStringSize = 0;
		for (long i = 0; i < lSize; i++)
		{
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStr = ipStrings->At(i);
			lTotalStringSize += ipStr->Size;
			if (i > 0)
			{
				lTotalStringSize += 4;
			}
			if (ipStr->GetMode() != kSpatialMode)
			{
				UCLIDException ue("ELI37130", "All strings should be spatial.");
				ue.addDebugInfo("String number", i+1);
				throw ue;
			}
		}
		m_eMode = kSpatialMode;
		m_strString.reserve(lTotalStringSize + 1);
		m_vecLetters.reserve(lTotalStringSize);
		m_ipPageInfoMap.CreateInstance(CLSID_LongToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI37132", m_ipPageInfoMap != __nullptr);
		long nPos = 0;
		long nLastProcessedPage = 0;
		for (long i = 0; i < lSize; i++)
		{
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStr = ipStrings->At(i); 
			long nCurrPage = ipStr->GetFirstPageNumber();
			long nLastPage = ipStr->GetLastPageNumber();
			
			// if the current page is < than the last processed page throw an exception
            if (nCurrPage < nLastProcessedPage)
			{
				UCLIDException ue("ELI37156", "Pages must be in order.");
				ue.addDebugInfo("Current Page", nCurrPage);
				ue.addDebugInfo("Last Processed Page", nLastProcessedPage);
				throw ue;
			}

			if (i == 0)
			{
				m_strSourceDocName = ipStr->SourceDocName;
				m_strOCREngineVersion = ipStr->OCREngineVersion;

				UCLID_RASTERANDOCRMGMTLib::IHasOCRParametersPtr ipHasOCRParameters(ipStr);
				ASSERT_RESOURCE_ALLOCATION("ELI49564", ipHasOCRParameters != __nullptr);
				m_ipOCRParameters = ipHasOCRParameters->OCRParameters;
			}
			else if (nCurrPage == nLastProcessedPage)
			{
				// will be adding string on the start of a new line
				getThisAsCOMPtr()->AppendString("\r\n");
				nPos += 2;
			}
			else if (nCurrPage != nLastProcessedPage && vbInsertPageBreaks)
			{
				getThisAsCOMPtr()->AppendString("\r\n\r\n");
				nPos += 4;
			}

			// Add the string to the m_strString object
			m_strString += asString(ipStr->String);

			// Acc the page letter object to the m_vecLetters
			CPPLetter *letters = NULL;
			long nNumLetters = 0;
			ipStr->GetOCRImageLetterArray(&nNumLetters, (void**)&letters);

			m_vecLetters.resize(nPos + nNumLetters);
			long lCopySize = sizeof(CPPLetter) * nNumLetters;
			memcpy_s(&(m_vecLetters[nPos]), lCopySize, letters, lCopySize);

			// Add the pageInfo objects
			// Need to iterate through all the pages that are contained in the source string
			ILongToObjectMapPtr ipSourcePageInfos = ipStr->SpatialPageInfos;
			long nSize = ipSourcePageInfos->Size;
			for (long k = 0; k < nSize; k++)
			{
				long nPage;
				IUnknownPtr ipUnkVariableValue;
				ipSourcePageInfos->GetKeyValue(k, &nPage, &ipUnkVariableValue);

				// Do not add page info if the page is less than or equal to the last processed
				// since there should already be a page info for those pages
				if (nPage > nLastProcessedPage)
				{
					UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo = ipUnkVariableValue;
					ASSERT_RESOURCE_ALLOCATION("ELI37782", ipPageInfo != __nullptr);
					m_ipPageInfoMap->Set(nPage, ipPageInfo);
				}
			}

			nLastProcessedPage = nLastPage;

			nPos += nNumLetters;
		}

		m_ipPageInfoMap->SetReadonly();
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37128");
}
