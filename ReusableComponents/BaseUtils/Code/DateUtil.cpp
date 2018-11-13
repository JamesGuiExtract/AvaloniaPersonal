#include "stdafx.h"
#include "DateUtil.h"
#include "cpputil.h"

using namespace std;

//-------------------------------------------------------------------------------------------------
string getDayOfWeekName(long lNumber)
{
	string strResult;

	switch (lNumber)
	{
	case 1:
		strResult = "Sunday";
		break;

	case 2:
		strResult = "Monday";
		break;

	case 3:
		strResult = "Tuesday";
		break;

	case 4:
		strResult = "Wednesday";
		break;

	case 5:
		strResult = "Thursday";
		break;

	case 6:
		strResult = "Friday";
		break;

	case 7:
		strResult = "Saturday";
		break;
	}

	return strResult;
}
//-------------------------------------------------------------------------------------------------
string getMonthName(long lNumber)
{
	string strResult;

	switch (lNumber)
	{
	case 1:
		strResult = "January";
		break;

	case 2:
		strResult = "February";
		break;

	case 3:
		strResult = "March";
		break;

	case 4:
		strResult = "April";
		break;

	case 5:
		strResult = "May";
		break;

	case 6:
		strResult = "June";
		break;

	case 7:
		strResult = "July";
		break;

	case 8:
		strResult = "August";
		break;

	case 9:
		strResult = "September";
		break;

	case 10:
		strResult = "October";
		break;

	case 11:
		strResult = "November";
		break;

	case 12:
		strResult = "December";
		break;
	}

	return strResult;
}
//-------------------------------------------------------------------------------------------------
long getValidDay(const string& strWord)
{
	// Test for numbers
	try
	{
		long lResult = 0;

		// Trim leading and trailing whitespace
		string strTest = trim( strWord, " \r\n", " \r\n" );

		// Make into upper case
		makeUpperCase( strTest );

		unsigned long ulPos = strTest.find_first_not_of( "0123456789", 0 );
		if (ulPos != string::npos)
		{
			strTest = strTest.substr( 0, ulPos );
		}

		long lNumber = asLong( strTest.c_str() );

		if (lNumber > 0 && lNumber < 32)
		{
			lResult = lNumber;
		}

		return lResult;
	}
	catch (...)
	{
		return 0;
	}
}
//-------------------------------------------------------------------------------------------------
long getValidDayOfWeek(const string& strWord)
{
	long lResult = 0;

	// Trim leading and trailing whitespace
	string strTest = trim( strWord, " \r\n", " \r\n" );

	// Make into upper case
	makeUpperCase( strTest );

	////////////////////////////
	// Test against days of week
	////////////////////////////

	// Monday
	if ((strTest == "MONDAY") || (strTest == "MON"))
	{
		lResult = 2;
	}
	// Tuesday
	else if ((strTest == "TUESDAY") || (strTest == "TUE") || (strTest == "TUES"))
	{
		lResult = 3;
	}
	// Wednesday
	else if ((strTest == "WEDNESDAY") || (strTest == "WED"))
	{
		lResult = 4;
	}
	// Thursday
	else if ((strTest == "THURSDAY") || (strTest == "THU") || (strTest == "THUR") || (strTest == "THURS"))
	{
		lResult = 5;
	}
	// Friday
	else if ((strTest == "FRIDAY") || (strTest == "FRI"))
	{
		lResult = 6;
	}
	// Saturday
	else if ((strTest == "SATURDAY") || (strTest == "SAT"))
	{
		lResult = 7;
	}
	// Sunday
	else if ((strTest == "SUNDAY") || (strTest == "SUN"))
	{
		lResult = 1;
	}

	return lResult;
}
//-------------------------------------------------------------------------------------------------
long getValidMonth(const string& strWord)
{
	long lResult = 0;

	// Trim leading and trailing whitespace
	string strTest = trim( strWord, " \r\n", " \r\n" );

	// Make into upper case
	makeUpperCase( strTest );

	///////////////////////////
	// Test against month names
	///////////////////////////

	// January
	if ((strTest == "JANUARY") || (strTest == "JAN"))
	{
		lResult = 1;
	}
	// February
	else if ((strTest == "FEBRUARY") || (strTest == "FEB"))
	{
		lResult = 2;
	}
	// March
	else if ((strTest == "MARCH") || (strTest == "MAR"))
	{
		lResult = 3;
	}
	// April
	else if ((strTest == "APRIL") || (strTest == "APR"))
	{
		lResult = 4;
	}
	// May
	else if (strTest == "MAY")
	{
		lResult = 5;
	}
	// June
	else if ((strTest == "JUNE") || (strTest == "JUN"))
	{
		lResult = 6;
	}
	// July
	else if ((strTest == "JULY") || (strTest == "JUL"))
	{
		lResult = 7;
	}
	// August
	else if ((strTest == "AUGUST") || (strTest == "AUG"))
	{
		lResult = 8;
	}
	// September
	else if ((strTest == "SEPTEMBER") || (strTest == "SEPT") || (strTest == "SEP"))
	{
		lResult = 9;
	}
	// October
	else if ((strTest == "OCTOBER") || (strTest == "OCT"))
	{
		lResult = 10;
	}
	// November
	else if ((strTest == "NOVEMBER") || (strTest == "NOV"))
	{
		lResult = 11;
	}
	// December
	else if ((strTest == "DECEMBER") || (strTest == "DEC"))
	{
		lResult = 12;
	}

	return lResult;
}
//-------------------------------------------------------------------------------------------------
long getValidYear(const string& strWord, long lMinYear)
{
	try
	{
		long lResult = 0;

		// Trim leading and trailing whitespace
		string strTest = trim( strWord, " \r\n", " \r\n" );

		// Test for numbers
		long lLength = strWord.length();

		// Two characters in string
		if (lLength == 2)
		{
			// Characters must be digits
			if (isDigitChar( strWord[0] ) && isDigitChar( strWord[1] ))
			{
				long lValue = asLong( strWord );
				long lMin = lMinYear % 100;
				if (lValue >= lMin)
				{
					// 20th century
					lResult = lMinYear - lMin + lValue;
				}
				else
				{
					// 21st century
					lResult = lMinYear - lMin + lValue + 100;
				}
			}
		}
		// Four characters in string
		else if (lLength == 4)
		{
			// Characters must be digits
			if (isDigitChar( strWord[0] ) && isDigitChar( strWord[1] ) && 
				isDigitChar( strWord[2] ) && isDigitChar( strWord[3] ))
			{
				long lValue = asLong( strWord );

				// Test for unexpectedly small result
				if (lValue < 100)
				{
					long lMin = lMinYear % 100;
					if (lValue >= lMin)
					{
						// 20th century
						lResult = lMinYear - lMin + lValue;
					}
					else
					{
						// 21st century
						lResult = lMinYear - lMin + lValue + 100;
					}
				}
				// Otherwise just use the result
				else
				{
					lResult = lValue;
				}
			}
		}
		// Five or more characters in string
		else if (lLength > 4)
		{
			// First two characters must be digits
			if (isDigitChar( strWord[0] ) && isDigitChar( strWord[1] ))
			{
				// Check for acceptable century {19, 20, 21}
				string strCentury = strWord.substr( 0, 2 );
				long lCentury = asLong( strCentury );
				if (lCentury > 18 && lCentury < 22)
				{
					// Next two characters must be digits
					if (isDigitChar( strWord[2] ) && isDigitChar( strWord[3] ))
					{
						// Extract first four digits as year
						lResult = asLong( strWord.substr( 0, 4 ) );
					}
				}
			}
		}

		return lResult;
	}
	catch (...)
	{
		return 0;
	}
}
//-------------------------------------------------------------------------------------------------
bool isValidAMPM(const string& strWord, bool* bIsAM)
{
	try
	{
		bool bResult = false;

		// Trim leading and trailing whitespace
		string strTest = trim( strWord, " \r\n", " \r\n" );

		// Make into upper case
		makeUpperCase( strTest );

		// Test for AM
		if ((strTest == "AM") || (strTest == "A"))
		{
			*bIsAM = true;
			bResult = true;
		}
		else if ((strTest == "PM") || (strTest == "P"))
		{
			*bIsAM = false;
			bResult = true;
		}

		return bResult;
	}
	catch (...)
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
bool isValidDate(string strDate, long *plMonth, long *plDay, long *plYear, bool bDayMonthYear,
				 long lMinYear)
{
	try
	{
		bool	bReturn = false;

		//////////////////////////////////
		// Check for appropriate delimiter
		//   Slash --> 09/15/04
		//   Dash  --> 09-15-04
		//   Dot   --> 09.15.04
		//////////////////////////////////
		char	cDelimiter = 0;
		if (strDate.find_first_of( '/' ) != string::npos)
		{
			cDelimiter = '/';
		}
		else if (strDate.find_first_of( '-' ) != string::npos)
		{
			cDelimiter = '-';
		}
		else if (strDate.find_first_of( '.' ) != string::npos)
		{
			cDelimiter = '.';
		}

		if (cDelimiter == 0)
		{
			return false;
		}

		/////////////////
		// Parse the date
		/////////////////

		// Find the one or two delimiters
		unsigned long ulPos1 = strDate.find( cDelimiter );
		if (ulPos1 == string::npos)
		{
			// No delimiter found
			return false;
		}

		unsigned long ulPos2 = string::npos;
		if (strDate.length() > ulPos1 + 1)
		{
			ulPos2 = strDate.find( cDelimiter, ulPos1 + 1 );
		}

		// Get tokens
		unsigned long ulLength = strDate.length();
		string str1 = strDate.substr( 0, ulPos1 );
		string str2;
		string str3;
		if (ulPos2 != string::npos)
		{
			str2 = strDate.substr( ulPos1 + 1, ulPos2 - ulPos1 - 1 );
			if (ulLength > ulPos2 + 1)
			{
				str3 = strDate.substr( ulPos2 + 1, strDate.length() - ulPos2 - 1 );

				// Trim text at first non-digit
				unsigned long ulTrim = str3.find_first_not_of( "0123456789" );
				if (ulTrim != string::npos)
				{
					str3 = str3.substr( 0, ulTrim );
				}
			}
		}
		else
		{
			str2 = strDate.substr( ulPos1 + 1, strDate.length() - ulPos1 - 1 );
		}

		// Determine Month and Day from tokens
		long lMonth = 0;
		long lDay = 0;
		lMonth = bDayMonthYear ? asLong( str2.c_str() ) : asLong( str1.c_str() );
		lDay = bDayMonthYear ? asLong( str1.c_str() ) : asLong( str2.c_str() );

		//////////////////////////////////////
		// Test for mis-identified time string
		//   - Only two substrings
		//   - Trailing A or P after digits 
		//     in second substring
		//////////////////////////////////////
		if (str3.length() == 0)
		{
			// Trim digits from second string
			string strTest = trim( str2, "0123456789", "" );
			makeUpperCase( strTest );
			if ((strTest.length() > 0) && ((strTest[0] == 'A') || (strTest[0] == 'P')))
			{
				return false;
			}
		}

		// Determine Year from token, if present
		long lYear = 0;
		if (str3.length() > 0)
		{
			lYear = getValidYear(str3, lMinYear);
		}
		// Only two tokens, default to this year
		else
		{
			SYSTEMTIME	time;
			GetSystemTime( &time );

			lYear = time.wYear;
		}

		//////////////////////////////////////
		// Create date object and check status
		//////////////////////////////////////
		COleDateTime tmStop;
		tmStop.SetDate( lYear, lMonth, lDay );
		if (tmStop.GetStatus() == COleDateTime::valid)
		{
			// Provide date components to caller
			if (plMonth != __nullptr)
			{
				*plMonth = lMonth;
			}

			if (plDay != __nullptr)
			{
				*plDay = lDay;
			}

			if (plYear != __nullptr)
			{
				*plYear = lYear;
			}

			// Set flag
			bReturn = true;
		}

		return bReturn;
	}
	catch (...)
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
bool isValidTime(const string& strWord, long* lHour, long* lMinute, long* lSecond, 
				 bool* bFoundAMPM, bool* bIsAM)
{
	try
	{
		bool bResult = false;

		// Trim leading and trailing whitespace
		string strTest = trim( strWord, " \r\n", " \r\n" );

		// Make into upper case
		makeUpperCase( strTest );

		long	lLocalHour = -1;
		long	lLocalMinute = -1;
		long	lLocalSecond = -1;
		bool	bLocalFoundAMPM = false;
		bool	bLocalAM = false;

		// Find and process colons
		long	lLastPos = -1;
		bool	bFoundLastColon = false;
		while (!bFoundLastColon)
		{
			// Find "next" colon
			unsigned long ulPos = strTest.find( ':', lLastPos + 1 );
			string strTemp;
			if (ulPos != string::npos)
			{
				// Extract substring before colon
				strTemp = strTest.substr( lLastPos + 1, ulPos - lLastPos - 1 );
				lLastPos = ulPos;
			}
			else
			{
				// No more colons to find
				if (lLastPos > -1)
				{
					// Extract substring after last colon
					strTemp = strTest.substr( lLastPos + 1, strTemp.length() - lLastPos - 1 );
				}

				bFoundLastColon = true;
			}

			// Check for unwanted characters at beginning of first substring
			// This will happen if strWord was "12/11/2004 08:53PM"
			strTemp = trim( strTemp, " \r\n", " \r\n" );
			if ((lLastPos == -1) && (strTemp.length() > 2))
			{
				// Find last space
				unsigned long ulPos = strTemp.find_last_of( ' ' );

				// Extract subsequent substring
				if (ulPos != string::npos)
				{
					strTemp = strTemp.substr( ulPos + 1, strTemp.length() - ulPos - 1 );
				}
			}

			// Handle substring
			if (strTemp.length() > 0)
			{
				bool	bTestAMPM = true;

				//substr is required to prevent "12 PM" from throwing an exception in asLong.
				strTemp = trim( strTemp, " ", "" );
				long lValue = asLong( strTemp.substr(0,2) );

				// Use value as Hour
				if (lLocalHour == -1)
				{
					if ( (lValue >= 0) && (lValue < 24) )
					{
						lLocalHour = lValue;
					}
					else
					{
						return false;
					}
					bTestAMPM = false;
				}
				// Use value as Minute
				else if (lLocalMinute == -1)
				{
					if ((lValue >= 0) && (lValue <= 59))
					{
						lLocalMinute = lValue;
					}
					else
					{
						return false;
					}
				}
				// Use value as Second
				else if (lLocalSecond == -1)
				{
					if ((lValue >= 0) && (lValue <= 59))
					{
						lLocalSecond = lValue;
					}
					else
					{
						return false;
					}
				}

				// Check for AM or PM if Minute or Second just found
				if (bTestAMPM && !bLocalFoundAMPM)
				{
					// Trim digits and spaces from test string
					strTemp = trim( strTemp, "0123456789 ", "" );

					// Check for leading A or P
					if (strTemp.length() > 0)
					{
						if (strTemp[0] == 'A')
						{
							bLocalAM = true;
							bLocalFoundAMPM = true;
						}
						else if (strTemp[0] == 'P')
						{
							bLocalAM = false;
							bLocalFoundAMPM = true;
						}
					}
				}		// end if testing for AM or PM
			}			// end if non-empty substring
		}				// end while processing colons

		// Evaluate results
		//   - Must have Hour and Minute
		//   - Second is optional
		//   - AM / PM is optional
		if ((lLocalHour > -1) && (lLocalMinute > -1))
		{
			// Set flag
			bResult = true;

			//////////////////////
			// Provide time values
			//////////////////////

			// Hours and Minutes
			*lHour = lLocalHour;
			*lMinute = lLocalMinute;

			// Check Hour value for Military time that implies PM
			if (lLocalHour > 12)
			{
				bLocalFoundAMPM = true;
				bLocalAM = false;
			}

			// Seconds
			if (lLocalSecond > -1)
			{
				*lSecond = lLocalSecond;
			}
			else
			{
				// Default to 0
				*lSecond = 0;
			}

			// AM or PM
			if (bLocalFoundAMPM)
			{
				*bFoundAMPM = true;
				*bIsAM = bLocalAM;
			}
			else
			{
				*bFoundAMPM = false;
			}
		}

		return bResult;
	}
	catch (...)
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils tm systemTimeToTm(const SYSTEMTIME &st)
{
	tm tm_time;
	ZeroMemory(&tm_time, sizeof(tm_time));

	tm_time.tm_year = st.wYear - 1900;
	tm_time.tm_mon = st.wMonth - 1;
	tm_time.tm_mday = st.wDay;
	tm_time.tm_hour = st.wHour;
	tm_time.tm_min = st.wMinute;
	tm_time.tm_sec = st.wSecond;
	
	return tm_time;
}
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils SYSTEMTIME tmToSystemTime(const tm &t)
{
	SYSTEMTIME st_time;
	ZeroMemory(&st_time, sizeof(SYSTEMTIME));

	st_time.wYear = t.tm_year + 1900;
	st_time.wMonth = t.tm_mon + 1;
	st_time.wDay = t.tm_mday;
	st_time.wHour = t.tm_hour;
	st_time.wMinute = t.tm_min;
	st_time.wSecond = t.tm_sec;
	
	return st_time;
}
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils ULONGLONG asULongLong(const SYSTEMTIME& st)
{
	FILETIME ft;
	SystemTimeToFileTime(&st, &ft);

	return (((ULONGLONG) ft.dwHighDateTime) << 32) + ft.dwLowDateTime;		
}//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string formatSystemTime(const SYSTEMTIME &st, const string& strFormat)
{
	tm _tm = systemTimeToTm(st);
	char szBuf[256] = {0};
	strftime(szBuf, sizeof(szBuf), strFormat.c_str(), &_tm);
    return string(szBuf);
}
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils SYSTEMTIME asLocalSystemTime(const CTime &ct)
{
	tm _tm;
	ct.GetLocalTm(&_tm);

	return tmToSystemTime(_tm);
}
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils SYSTEMTIME asUTCSystemTime(const CTime &ct)
{
	tm _tm;
	ct.GetGmtTm(&_tm);

	return tmToSystemTime(_tm);
}