// DateTimeSplitter.cpp : Implementation of CDateTimeSplitter
#include "stdafx.h"
#include "AFSplitters.h"
#include "DateTimeSplitter.h"

#include <cpputil.h>
#include <comutils.h>
#include <DateUtil.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version 2:
//   * Added saving of m_bSplitDefaults
// Version 3:
//   * Added m_lMinTwoDigitYear and m_bTwoDigitYearBeforeCurrentYear
// Version 4: Added CIdentifiableObject
const unsigned long gnCurrentVersion = 4;

const long glDEFAULT_TWO_DIGIT_YEAR = 1970;

//-------------------------------------------------------------------------------------------------
// CDateTimeSplitter
//-------------------------------------------------------------------------------------------------
CDateTimeSplitter::CDateTimeSplitter()
: m_bDirty(false),
  m_bMonthAsName(false),
  m_bFourDigitYear(true),
  m_bShowDayOfWeek(false),
  m_bMilitaryTime(false),
  m_bShowFormattedOutput(false),
  m_bSplitDefaults(true),
  m_lMinTwoDigitYear(glDEFAULT_TWO_DIGIT_YEAR),
  m_bTwoDigitYearBeforeCurrentYear(false)
{
}
//-------------------------------------------------------------------------------------------------
CDateTimeSplitter::~CDateTimeSplitter()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19073");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeSplitter,
		&IID_IDateTimeSplitter,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent,
		&IID_IIdentifiableObject
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IAttributeSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::raw_SplitAttribute(IAttribute *pAttribute, IAFDocument *pAFDoc, 
												   IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		try
		{
			// Create local Attribute object
			IAttributePtr	ipAttr( pAttribute );
			ASSERT_RESOURCE_ALLOCATION("ELI09750", ipAttr != __nullptr);

			// Retrieve Attribute Value text
			ISpatialStringPtr ipValue = ipAttr->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15538", ipValue != __nullptr);
			string strText = asString( ipValue->String );

			///////////////////////////////////
			// Extract Month, Date, Year values
			// and Hour, Minute, Second, AM/PM
			///////////////////////////////////
			//There was an issue with spaces within the time causing them to be split
			//and then strWord would contain "4:30:" , " ", and "25 PM"
			strText = replaceMultipleCharsWithOne(strText, " ", " ", false);
			
			//Remove any empty whitespace around the colon( 12 :25 => 12:25, etc...)
			trimColonWS(strText);

			// Clear date-time part flags and values
			bool	bFoundCentury = false, bFoundDayOfWeek= false,
					bFoundTime = false,	bFoundAMPM = false, bIsAM = false;
			bool	bStillValid = true;
			long	lHour = 0, lMinute = 0, lSecond = 0;
			long	lYear = 0, lMonth = 0, lDay = 0, lCentury = 0, lDayOfWeek = 0;
			bool	bFoundYear = false, bFoundMonth = false, bFoundDay = false;

			// First check for MM/DD/YYYY format
			if (isValidDate( strText, &lMonth, &lDay, &lYear, false, getMinimumTwoDigitYear()))
			{
				// Set flags
				bFoundMonth = true, bFoundDay = true, bFoundYear = true;
				
				//there was an issue with a string that had a date and a time
				//(ie:) Saturday August 12, 2005 1:45:12 PM.  It would send 
				//"Saturday August 12, 2005 1" as the strText to isValidTime. 
				//This fixes that from happening
				int pos = strText.find( ':', 0 );
				if(pos - 2 > 0)
				{
					strText.erase(0, pos-2);
				}

				// Also check for valid time within the whole text
				if (isValidTime( strText, &lHour, &lMinute, &lSecond, &bFoundAMPM, &bIsAM ))
				{
					bFoundTime = true;
				}
			}
			// Also check for only valid time within the whole text
			else if (isValidTime( strText, &lHour, &lMinute, &lSecond, &bFoundAMPM, &bIsAM ))
			{
				bFoundTime = true;
			}
			// Otherwise parse text into words and evaluate individual pieces
			else
			{
				// Convert punctuation and carriage returns into spaces
				int lPos = -1;
				while( (lPos = strText.find_first_of( ",._\r\n" )) != string::npos )
				{
					strText.erase( lPos, 1 );
					strText.insert( lPos, " " );
				}

				// Parse text into words
				std::vector<std::string>	vecWords;
				StringTokenizer	st( ' ' );
				st.parse( strText.c_str(), vecWords );

				// Evaluate each word
				long lSize = vecWords.size();
				for (int i = 0; i < lSize; i++)
				{
					// Get this word
					string strWord = vecWords[i];
					makeUpperCase( strWord );

					if(!parseDate(strWord, bFoundMonth, bFoundDay, bFoundYear, bFoundCentury,
										lMonth, lDay, lYear, lCentury))
					{
						continue;
					}
					parseTime(strWord, bFoundTime, lHour, lMinute, lSecond, bFoundAMPM, bIsAM);

					// Check for Day of Week (not used at this time)
					if (!bFoundDayOfWeek)
					{
						lDayOfWeek = getValidDayOfWeek( strWord );
						if (lDayOfWeek > 0)
						{
							bFoundDayOfWeek = true;
							continue;
						}
					}
				}//end for
			}//end else

			// Modify Hour to fit military time
			if (bFoundTime)
			{
				if (lHour < 12 && bFoundAMPM && !bIsAM)
				{
					lHour += 12;
				}
			}

			////////////////////////////////////////////
			// Add each piece to the COleDateTime object
			////////////////////////////////////////////
			m_dt.SetStatus( COleDateTime::invalid );
			if (bFoundDay && bFoundMonth && bFoundYear)
			{
				// Check if Time is also present
				if (bFoundTime)
				{
					m_dt.SetDateTime( lYear, lMonth, lDay, lHour, lMinute, lSecond );
				}
				// Just Date information
				else
				{
					m_dt.SetDate( lYear, lMonth, lDay );
				}
			}
			else if (bFoundTime)
			{
				m_dt.SetTime( lHour, lMinute, lSecond );
			}

			////////////////////////
			// Create sub-attributes
			////////////////////////
			if (m_dt.GetStatus() == COleDateTime::valid)
			{
				IIUnknownVectorPtr ipMainAttrSub = ipAttr->SubAttributes;
				ASSERT_RESOURCE_ALLOCATION("ELI09756", ipMainAttrSub != __nullptr);

				getDate(ipValue, ipMainAttrSub, m_bShowDayOfWeek, bFoundDay, m_bSplitDefaults,
						lMonth, bStillValid, lDay, lYear);

				// Formatted
				if (m_bShowFormattedOutput)
				{
					formatOutput(ipValue, ipMainAttrSub, bStillValid);
				}

				getTime(ipValue, ipMainAttrSub, bFoundTime, m_bSplitDefaults, lHour, bIsAM);
						
			}//end if
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter("ELI13470", "Unable to format date/time!", ue);
			throw uexOuter;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09724")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19592", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Split date and/or time").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09719")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_DateTimeSplitter );
		ASSERT_RESOURCE_ALLOCATION( "ELI09722", ipObjCopy != __nullptr );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09723");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		UCLID_AFSPLITTERSLib::IDateTimeSplitterPtr ipSource( pObject );
		ASSERT_RESOURCE_ALLOCATION( "ELI09720", ipSource !=NULL);

		// Retrieve settings
		m_bMonthAsName = asCppBool(ipSource->SplitMonthAsName);
		m_bFourDigitYear = asCppBool(ipSource->SplitFourDigitYear);
		m_bShowDayOfWeek = asCppBool(ipSource->SplitDayOfWeek);
		m_bMilitaryTime = asCppBool(ipSource->SplitMilitaryTime);
		m_bShowFormattedOutput = asCppBool(ipSource->ShowFormattedOutput);
		if (m_bShowFormattedOutput)
		{
			m_strOutputFormat = asString(ipSource->OutputFormat);
		}

		m_bSplitDefaults = asCppBool(ipSource->SplitDefaults);

		m_lMinTwoDigitYear = ipSource->MinimumTwoDigitYear;

		m_bTwoDigitYearBeforeCurrentYear = asCppBool(ipSource->TwoDigitYearBeforeCurrent);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09721");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19089", pbValue != __nullptr);

		try
		{
			// Check license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19090");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDateTimeSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::get_SplitMonthAsName(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		*pVal = asVariantBool( m_bMonthAsName );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19163")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::put_SplitMonthAsName(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		m_bMonthAsName = asCppBool( newVal );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19165")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::get_SplitFourDigitYear(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		*pVal = asVariantBool( m_bFourDigitYear );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09732")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::put_SplitFourDigitYear(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		m_bFourDigitYear = asCppBool( newVal );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09733")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::get_SplitDayOfWeek(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		*pVal = asVariantBool( m_bShowDayOfWeek );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09734")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::put_SplitDayOfWeek(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		m_bShowDayOfWeek = asCppBool( newVal );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09735")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::get_SplitMilitaryTime(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		*pVal = asVariantBool( m_bMilitaryTime );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09736")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::put_SplitMilitaryTime(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		m_bMilitaryTime = asCppBool( newVal );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09737")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::get_ShowFormattedOutput(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		*pVal = asVariantBool( m_bShowFormattedOutput );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09738")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::put_ShowFormattedOutput(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		m_bShowFormattedOutput = asCppBool( newVal );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09739")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::get_OutputFormat(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		*pVal = _bstr_t( m_strOutputFormat.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09740")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::put_OutputFormat(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Local copy
		string strNewVal = asString( newVal );

		// Test the format string
		if (isFormatValid( strNewVal ))
		{
			m_strOutputFormat = strNewVal;
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09741")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::get_SplitDefaults(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		*pVal = asVariantBool( m_bSplitDefaults );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10132")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::put_SplitDefaults(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		m_bSplitDefaults = asCppBool( newVal );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10133")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::get_MinimumTwoDigitYear(long *plVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI25894", plVal != __nullptr);

		// Check license state
		validateLicense();

		*plVal = m_lMinTwoDigitYear;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25719")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::put_MinimumTwoDigitYear(long lVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		m_lMinTwoDigitYear = lVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25720")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::get_TwoDigitYearBeforeCurrent(VARIANT_BOOL *pvbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI25895", pvbVal != __nullptr);

		// Check license state
		validateLicense();

		*pvbVal = asVariantBool(m_bTwoDigitYearBeforeCurrentYear);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25721")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::put_TwoDigitYearBeforeCurrent(VARIANT_BOOL vbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license state
		validateLicense();

		m_bTwoDigitYearBeforeCurrentYear = asCppBool(vbVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25722")
		
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_DateTimeSplitter;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset all the member variables
		m_bMonthAsName = false;
		m_bFourDigitYear = true;
		m_bShowDayOfWeek = false;
		m_bMilitaryTime = false;
		m_bShowFormattedOutput = false;
		m_strOutputFormat = "";
		m_bSplitDefaults = true;
		m_lMinTwoDigitYear = glDEFAULT_TWO_DIGIT_YEAR;
		m_bTwoDigitYearBeforeCurrentYear = false;

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
			UCLIDException ue( "ELI09747", "Unable to load newer Date-Time Splitter." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bMonthAsName;
			dataReader >> m_bFourDigitYear;
			dataReader >> m_bShowDayOfWeek;
			dataReader >> m_bMilitaryTime;
			dataReader >> m_bShowFormattedOutput;

			if (m_bShowFormattedOutput)
			{
				dataReader >> m_strOutputFormat;
			}
		}

		if (nDataVersion >= 2)
		{
			dataReader >> m_bSplitDefaults;
		}

		if (nDataVersion >= 3)
		{
			dataReader >> m_lMinTwoDigitYear;
			dataReader >> m_bTwoDigitYearBeforeCurrentYear;
		}

		if (nDataVersion >= 4)
		{
			// Load the GUID for the IIdentifiableObject interface.
			loadGUID(pStream);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09748");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;

		dataWriter << m_bMonthAsName;
		dataWriter << m_bFourDigitYear;
		dataWriter << m_bShowDayOfWeek;
		dataWriter << m_bMilitaryTime;
		dataWriter << m_bShowFormattedOutput;

		if (m_bShowFormattedOutput)
		{
			dataWriter << m_strOutputFormat;
		}

		dataWriter << m_bSplitDefaults;
		dataWriter << m_lMinTwoDigitYear;
		dataWriter << m_bTwoDigitYearBeforeCurrentYear;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09749");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitter::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33556")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CDateTimeSplitter::isFormatValid(const string& strFormat)
{
	CString zFormat;
	if (strFormat.length() > 0)
	{
		// Get current system time
		SYSTEMTIME	now;
		GetSystemTime( &now );

		// Initialize Date-Time object with current time
		COleDateTime	dt( now );

		// Exercise the format string
		zFormat = dt.Format( strFormat.c_str() );
	}

	return (zFormat.GetLength() > 0);
}
//-------------------------------------------------------------------------------------------------
long CDateTimeSplitter::getMinimumTwoDigitYear()
{
	if (m_bTwoDigitYearBeforeCurrentYear)
	{
		CTime currentTime = CTime::GetCurrentTime();
		return currentTime.GetYear() - 99;
	}
	else
	{
		return m_lMinTwoDigitYear;
	}
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_WRITING_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI09718", "Date Time Splitter");
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::trimColonWS(std::string & strText)
{
	//find the first colon (prime the while)			
	int pos = strText.find( ':', 0 );
	//trim spaces around the colon
	while (pos != string::npos)
	{
		//if we have hh :mm
		if(pos > 0 && strText[pos-1] == ' ')
		{
			strText.replace(pos-1, 2, ":");
			//since we're replacing 2 chars with 1, need to update pos in case
			//we have hh : mm (so the following if will catch it)
			pos--;
		}
		//if we have hh: mm
		if(pos + 1 < static_cast<int>(strText.length()) && strText[pos + 1] == ' ')
		{
			strText.replace(pos, 2, ":");
		}
		pos = strText.find(':', pos+1);
	}//end while
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::getDayOfWeek(ISpatialStringPtr& ipString, IIUnknownVectorPtr& ipMainAttrSub)
{
	IAttributePtr	ipDayOfWeek( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION("ELI09766", ipDayOfWeek != __nullptr);

	// Get DayOfWeek from the COleDateTime data member
	string strDOW = getDayOfWeekName( m_dt.GetDayOfWeek() );

	// Search for this string within the original text
	ISpatialStringPtr	ipValue = findSubstring( ipString, strDOW, false, false );
	ASSERT_RESOURCE_ALLOCATION("ELI09767", ipValue != __nullptr);

	// Build the sub-attribute and add to the collection
	ipDayOfWeek->PutName( _bstr_t( "DayOfWeek" ) );
	ipDayOfWeek->PutValue( ipValue );
	ipMainAttrSub->PushBack( ipDayOfWeek );
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::getMonth(ISpatialStringPtr& ipString, IIUnknownVectorPtr& ipMainAttrSub, 
								long &lMonth, bool &bStillValid)
{
	IAttributePtr	ipMonth( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION("ELI09757", ipMonth != __nullptr);
	ISpatialStringPtr	ipValue;

	// Get the month name
	string strMonthName = getMonthName( m_dt.GetMonth() );
	if (m_bMonthAsName)
	{
		// Look for month name in the original text
		ipValue = findSubstring( ipString, strMonthName, false, false );
		ASSERT_RESOURCE_ALLOCATION("ELI19097", ipValue != __nullptr);
	}
	else
	{
		// Retrieve original text
		string strOriginal = asString( ipString->String );

		// Convert text and month name strings to lower case
		makeLowerCase( strOriginal );
		makeLowerCase( strMonthName );

		// Search text for the month name
		long lMonthNamePos = strOriginal.find( strMonthName, 0 );
		bool bFoundName = (lMonthNamePos != string::npos);

		// Protect against invalid month number
		lMonth = m_dt.GetMonth();
		if (lMonth > 0 && lMonth < 13)
		{
			// Month number is valid, create a two-digit zero-padded search string
			CString zMonth;
			zMonth.Format( "%02d", lMonth );
			string strMonth = LPCTSTR(zMonth);

			// Look for month number
			long lMonthNumberPos = strOriginal.find( strMonth, 0 );
			bool bFoundNumber = (lMonthNumberPos != string::npos);

			// Force a hybrid string if month name found and month number also found
			if (bFoundName && bFoundNumber)
			{
				// Get the month sub-attribute as a hybrid string
				ipValue = findSubstring( ipString, strMonth, true, true );
				ASSERT_RESOURCE_ALLOCATION("ELI19098", ipValue != __nullptr);
			}
			else
			{
				// Find the month number
				ipValue = findSubstring( ipString, strMonth, false, true );
				ASSERT_RESOURCE_ALLOCATION("ELI19460", ipValue != __nullptr);
			}
		}
		else
		{
			// Invalid month number
			ipValue.CreateInstance( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION("ELI19110", ipValue != __nullptr);
			bStillValid = false;
		}
	}

	// Build the sub-attribute and add to the collection
	ipMonth->PutName( _bstr_t( "Month" ) );
	ipMonth->PutValue( ipValue );
	ipMainAttrSub->PushBack( ipMonth );
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::getDay(ISpatialStringPtr& ipString, IIUnknownVectorPtr &ipMainAttrSub,
							   long& lDay, bool &bStillValid)
{
	IAttributePtr	ipDay( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION("ELI09759", ipDay != __nullptr);
	ISpatialStringPtr	ipValue;

	// Protect against invalid day number
	lDay = m_dt.GetDay();
	if (lDay > 0 && lDay < 32)
	{
		// Day number is valid, create a two-digit zero-padded search string
		CString zDay;
		zDay.Format( "%02d", lDay );
		string strDay = LPCTSTR(zDay);

		// Find the day number
		ipValue = findSubstring( ipString, strDay, false, true );
		ASSERT_RESOURCE_ALLOCATION("ELI19105", ipValue != __nullptr);
	}
	else
	{
		// Invalid day number
		ipValue.CreateInstance( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION("ELI19107", ipValue != __nullptr);
		bStillValid = false;
	}

	// Build the sub-attribute and add to the collection
	ipDay->PutName( _bstr_t( "Day" ) );
	ipDay->PutValue( ipValue );
	ipMainAttrSub->PushBack( ipDay );
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::getYear(ISpatialStringPtr& ipString, IIUnknownVectorPtr &ipMainAttrSub,
							   long& lYear, bool &bStillValid)
{
	IAttributePtr	ipYear( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION("ELI09760", ipYear != __nullptr);
	ISpatialStringPtr	ipValue;

	// Protect against invalid year
	lYear = m_dt.GetYear();
	if (lYear > 0)
	{
		// Year number is valid, create a search string
		string strYear = asString( lYear );

		if (m_bFourDigitYear)
		{
			// Find the four-digit year number
			ipValue = findSubstring( ipString, strYear, false, false );
			ASSERT_RESOURCE_ALLOCATION("ELI19108", ipValue != __nullptr);
		}
		else
		{
			// Two-digit year, just retain the last two digits
			strYear = strYear.substr( 2 );

			// Find the two-digit year number
			ipValue = findSubstring( ipString, strYear, false, true );
			ASSERT_RESOURCE_ALLOCATION("ELI19196", ipValue != __nullptr);
		}
	}
	else
	{
		ipValue.CreateInstance( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION("ELI19109", ipValue != __nullptr);
		bStillValid = false;
	}

	// Build the sub-attribute and add to the collection
	ipYear->PutName( _bstr_t( "Year" ) );
	ipYear->PutValue( ipValue );
	ipMainAttrSub->PushBack( ipYear );
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::getHour(ISpatialStringPtr& ipString, IIUnknownVectorPtr &ipMainAttrSub,
							   long& lHour, bool &bIsAM)
{
	IAttributePtr	ipHour( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION("ELI09774", ipHour != __nullptr);
	ISpatialStringPtr	ipValue;

	// Military format?
	if (m_bMilitaryTime)
	{
		// Create a search string
		string strHour = asString( m_dt.GetHour() );

		// Find the hour number
		ipValue = findSubstring( ipString, strHour, false, true );
		ASSERT_RESOURCE_ALLOCATION("ELI19085", ipValue != __nullptr);

		// Add Hour sub-attribute
		ipHour->PutName( _bstr_t( "Hour" ) );
		ipHour->PutValue( ipValue );
		ipMainAttrSub->PushBack( ipHour );
	}
	// Otherwise "Normal" format
	else
	{
		// Get military hour value
		long lTempHour = m_dt.GetHour();

		// Between 12:00 AM and 12:59 AM
		if (lTempHour == 0)
		{
			bIsAM = true;
			lHour = 12;
		}
		// Between 1:00 AM and 11:59 AM
		else if (lTempHour < 12)
		{
			bIsAM = true;
			lHour = lTempHour;
		}
		// Between 12:00 PM and 12:59 PM
		else if (lTempHour == 12)
		{
			bIsAM = false;
			lHour = 12;
		}
		// Between 1:00 PM and 11:59 PM
		// or lTempHour >= 13 && <= 23
		else
		{
			bIsAM = false;
			lHour = lTempHour - 12;
		}

		// Find the hour number
		string strHour = asString( lHour );
		ipValue = findSubstring( ipString, strHour, false, true );
		ASSERT_RESOURCE_ALLOCATION("ELI19086", ipValue != __nullptr);

		// Add Hour sub-attribute
		ipHour->PutName( _bstr_t( "Hour" ) );
		ipHour->PutValue( ipValue );
		ipMainAttrSub->PushBack( ipHour );

		// Create and add AM/PM sub-attribute
		IAttributePtr	ipAMPM( CLSID_Attribute );
		ASSERT_RESOURCE_ALLOCATION("ELI09776", ipAMPM != __nullptr);
		ISpatialStringPtr	ipAMPMValue;

		if (bIsAM)
		{
			ipAMPMValue = findSubstring( ipString, "AM", false, false );
		}
		else
		{
			ipAMPMValue = findSubstring( ipString, "PM", false, false );
		}
		ASSERT_RESOURCE_ALLOCATION("ELI09777", ipAMPMValue != __nullptr);

		// Build the sub-attribute and add to the collection
		ipAMPM->PutName( _bstr_t( "AMPM" ) );
		ipAMPM->PutValue( ipAMPMValue );
		ipMainAttrSub->PushBack( ipAMPM );
	}
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::getMinute(ISpatialStringPtr& ipString, IIUnknownVectorPtr &ipMainAttrSub)
{
	IAttributePtr	ipMinute( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION("ELI09778", ipMinute != __nullptr);
	ISpatialStringPtr	ipValue;

	// Create a search string
	string strMinute = asString( m_dt.GetMinute() );

	// Find the minute number
	ipValue = findSubstring( ipString, strMinute, false, true );
	ASSERT_RESOURCE_ALLOCATION("ELI19087", ipValue != __nullptr);

	// Build the sub-attribute and add to the collection
	ipMinute->PutName( _bstr_t( "Minute" ) );
	ipMinute->PutValue( ipValue );
	ipMainAttrSub->PushBack( ipMinute );
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::getSecond(ISpatialStringPtr& ipString, IIUnknownVectorPtr &ipMainAttrSub)
{
	IAttributePtr	ipSecond( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION("ELI09780", ipSecond != __nullptr);
	ISpatialStringPtr	ipValue;

	// Create a search string
	string strSecond = asString( m_dt.GetSecond() );

	// Find the second number
	ipValue = findSubstring( ipString, strSecond, false, true );
	ASSERT_RESOURCE_ALLOCATION("ELI19088", ipValue != __nullptr);

	// Build the sub-attribute and add to the collection
	ipSecond->PutName( _bstr_t( "Second" ) );
	ipSecond->PutValue( ipValue );
	ipMainAttrSub->PushBack( ipSecond );
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::formatOutput(ISpatialStringPtr& ipString, IIUnknownVectorPtr &ipMainAttrSub, 
									 const bool bStillValid)
{
	IAttributePtr	ipFormat( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION("ELI09761", ipFormat != __nullptr);
	ISpatialStringPtr	ipValue;

	if (bStillValid)
	{
		// Build the formatted string
		CString	zFormat = m_dt.Format( m_strOutputFormat.c_str() );

		// Look for the sub-string, and return a hybrid string if not found
		ipValue = findSubstring( ipString, LPCTSTR(zFormat), false, false );
		ASSERT_RESOURCE_ALLOCATION("ELI09764", ipValue != __nullptr);

		// Build the sub-attribute and add to the collection
		ipFormat->PutName( _bstr_t( "Formatted" ) );
		ipFormat->PutValue( ipValue );
		ipMainAttrSub->PushBack( ipFormat );
	}
}
//-------------------------------------------------------------------------------------------------
bool CDateTimeSplitter::parseDate(std::string& strWord, bool& bFoundMonth, 
								  bool& bFoundDay, bool& bFoundYear, bool& bFoundCentury,
									long& lMonth, long& lDay, long& lYear, long& lCentury)
{
	// Check for text words that can be skipped
	if ((strWord == "DAY") || (strWord == "OF"))
	{
		return false;
	}

	// Check for Month
	if (!bFoundMonth)
	{
		lMonth = getValidMonth( strWord );
		if (lMonth > 0)
		{
			bFoundMonth = true;
			return false;
		}
	}

	// Check for Date
	if (!bFoundDay)
	{
		lDay = getValidDay( strWord );
		if (lDay > 0)
		{
			bFoundDay = true;
			return false;
		}
	}

	// Check for Year
	if (!bFoundYear)
	{
		long lMinYear = getMinimumTwoDigitYear();
		lYear = getValidYear(strWord, lMinYear);
		if (lYear > 0)
		{
			// Check for two-digit century (eg. strWord == "19" || strWord == "20")
			long lTwoDigitCentury = lYear % 100;
			long lTwoDigitExpected = lMinYear / 100;
			if ((strWord.length() == 2) && 
				(lTwoDigitCentury == lTwoDigitExpected || lTwoDigitCentury == lTwoDigitExpected + 1))
			{
				// Save this value as a century and look again for year
				lCentury = lTwoDigitCentury;
				bFoundCentury = true;
				bFoundYear = false;
			}
			else
			{
				// Check for previously found century
				if (bFoundCentury)
				{
					// Remove century component from this value
					lYear %= 100;

					// Use previously found century
					lYear = lCentury * 100 + lYear;
				}

				bFoundYear = true;
			}
			return false;
		}
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::parseTime(std::string strWord, bool& bFoundTime, long& lHour,
							 long& lMinute, long& lSecond, bool& bFoundAMPM,
							 bool& bIsAM)
{
	// Check for Time
	if (!bFoundTime)
	{
		bFoundTime = isValidTime( strWord, &lHour, &lMinute, &lSecond, 
			&bFoundAMPM, &bIsAM );
	}

	// Check for separate AM/PM indicator
	if (!bFoundAMPM)
	{
		bFoundAMPM = isValidAMPM( strWord, &bIsAM );
	}
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::getDate(ISpatialStringPtr& ipString, IIUnknownVectorPtr& ipMainAttrSub, 
								bool& m_bShowDayOfWeek, bool& bFoundDay, 
								bool& m_bSplitDefaults, long& lMonth, bool& bStillValid,
								long& lDay, long& lYear)
{
	// Day Of Week
	if (m_bShowDayOfWeek && bFoundDay)
	{
		getDayOfWeek(ipString, ipMainAttrSub);
	}

	// Month - only if Date was found
	if (bFoundDay && m_bSplitDefaults)
	{
		getMonth(ipString, ipMainAttrSub, lMonth, bStillValid);
	}

	// Day - only if Date was found
	if (bFoundDay && m_bSplitDefaults)
	{
		getDay(ipString, ipMainAttrSub, lDay, bStillValid);
	}

	// Year - only if Date was found
	if (bFoundDay && m_bSplitDefaults)
	{
		getYear(ipString, ipMainAttrSub, lYear, bStillValid);
	}
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitter::getTime(ISpatialStringPtr& ipString, IIUnknownVectorPtr& ipMainAttrSub, 
								bool& bFoundTime, bool& m_bSplitDefaults, long& lHour, bool& bIsAM)
{
	// Hour - only if Time was found
	if (bFoundTime && m_bSplitDefaults)
	{
		getHour(ipString, ipMainAttrSub, lHour, bIsAM);
	}

	// Minute - only if Time was found
	if (bFoundTime && m_bSplitDefaults)
	{
		getMinute(ipString, ipMainAttrSub);
	}

	// Second - only if Time was found
	if (bFoundTime && m_bSplitDefaults)
	{
		getSecond(ipString, ipMainAttrSub);
	}//end if
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CDateTimeSplitter::findSubstring(ISpatialStringPtr& ipOriginal, string strSearch, 
												   bool bForceHybrid, bool bPossibleDuplicate)
{
	// The output Spatial String
	ISpatialStringPtr	ipSubString;

	// Find the search string within the original text
	bool bMakeHybridString = false;
	long lFoundPos = ipOriginal->FindFirstInstanceOfStringCIS( get_bstr_t( strSearch ), 0 );
	if (lFoundPos != string::npos && !bForceHybrid)
	{
		if (!bPossibleDuplicate)
		{
			// Found the substring and this is not expected to be duplicated
			ipSubString = ipOriginal->GetSubString( lFoundPos, lFoundPos + strSearch.length() - 1 );
			ASSERT_RESOURCE_ALLOCATION("ELI19076", ipSubString != __nullptr);
		}
		else
		{
			// Found the substring but this may be duplicated
			if (lFoundPos + (long)strSearch.length() < ipOriginal->Size)
			{
				long lFoundPos2 = ipOriginal->FindFirstInstanceOfStringCIS( 
					get_bstr_t( strSearch ), lFoundPos + 1 );
				if (lFoundPos2 != string::npos)
				{
					// Second example found, use hybrid string
					bMakeHybridString = true;
				}
				else
				{
					// Second example not found
					ipSubString = ipOriginal->GetSubString( lFoundPos, 
						lFoundPos + strSearch.length() - 1 );
					ASSERT_RESOURCE_ALLOCATION("ELI19195", ipSubString != __nullptr);
				}
			}
			else
			{
				// Found at end of string, use the hybrid string since duplicated 
				// items are not expected to be at the end of the string
				bMakeHybridString = true;
			}
		}
	}
	else
	{
		// Did not find the search string, or sub-attribute must be hybrid
		bMakeHybridString = true;
	}

	// Make the hybrid string
	if (bMakeHybridString)
	{
		// Create the new Spatial String
		ipSubString.CreateInstance( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION("ELI19077", ipSubString != __nullptr);

		if (ipOriginal->HasSpatialInfo() == VARIANT_TRUE)
		{
			// Use spatial information from original string
			ipSubString->CreateHybridString(ipOriginal->GetOCRImageRasterZones(), strSearch.c_str(),
				ipOriginal->SourceDocName, ipOriginal->SpatialPageInfos);
		}
		else
		{
			// Non-spatial input string, just use the search text
			ipSubString->CreateNonSpatialString(strSearch.c_str(), ipOriginal->SourceDocName);
		}
	}

	// Return the Spatial String
	return ipSubString;
}
//-------------------------------------------------------------------------------------------------
