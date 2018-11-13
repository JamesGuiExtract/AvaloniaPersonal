//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DateUtil.cpp
//
// PURPOSE:	Various Date and Time utility functions
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius (November 2004 - present)
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"
#include "cpputil.h"

#include <string>

using namespace std;

// The number of SYSTEMTIME "ticks" in a second.
#define ST_SECOND 10000000

//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns Sunday, Monday, Tuesday, etc. for lNumber = 1, 2, 3, etc.  Returns empty string 
//			if lNumber < 1 OR lNumber > 7.
//
EXPORT_BaseUtils string getDayOfWeekName(long lNumber);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns January, February, etc. for lNumber = 1, 2, etc.  Returns empty string if 
//			lNumber < 1 OR lNumber > 12.
//
EXPORT_BaseUtils string getMonthName(long lNumber);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns 0 if strWord is not recognized as a valid day of the month.  Otherwise, returns
//			1 - 31.  Standard abbreviations are also recognized (i.e. 1st, 2nd, 3rd, 4th, etc.)
//
EXPORT_BaseUtils long getValidDay(const string& strWord);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns 0 if strWord is not recognized as a valid day of the week.  Otherwise, returns
//			1 for Sunday, 2 for Monday, 3 for Tuesday, 4 for Wednesday, 5 for Thursday, 
//			6 for Friday, and 7 for Saturday.  Standard abbreviations are also recognized
//			(i.e. Mon, Tue, Tues, Wed, etc.)
//
EXPORT_BaseUtils long getValidDayOfWeek(const string& strWord);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns 0 if strWord is not recognized as a valid month.  Otherwise, returns
//			1 for January, 2 for February, 3 for March, 4 for April, 5 for May, 6 for June, 
//			7 for July, 8 for August, 9 for September, 10 for October, 11 for November and 
//			12 for December.  Standard abbreviations are also recognized (i.e. Jan, Feb, Mar, etc.)
//
EXPORT_BaseUtils long getValidMonth(const string& strWord);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns 0 if strWord is not recognized as a valid year.  Otherwise, returns the year.
//			If strWord has two digits, the year is assumed to be between lMinYear and 99 years 
//              after. (Defaults to 1970-2069)
//			If strWord has three digits, 0 is returned.
//			If strWord has four digits, the value is returned.  
//			If strWord has five or more characters, 
//				If first two characters are "19","20","21", accept the next two characters if 
//					they are digits.  Remaining characters are ignored.
//				Otherwise, return 0.
//
EXPORT_BaseUtils long getValidYear(const string& strWord, long lMinYear=1970);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns true if strWord is recognized as a valid AM or PM indicator.  Otherwise, 
//			returns false.  
//			Valid inputs include: { "AM", "PM", "A", "P" }
//
EXPORT_BaseUtils bool isValidAMPM(const string& strWord, bool* bIsAM);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return true if specified string is a valid date.  Also returns Month, Day and Year 
//          if appropriate pointers are specified.
// REQUIRES: strDate Format = MM/DD/YY, MM-DD-YY, MM.DD.YY
//           YYYY is also accepted in place of YY
//           MM and DD are switched if bDayMonthYear = true
// NOTE:     Two digit years are assumed to be between lMinYear and 99 years after. For example, 
//           the default is 1970-2069.
EXPORT_BaseUtils bool isValidDate(string strDate, long *plMonth, long *plDay, long *plYear, 
								  bool bDayMonthYear = false, long lMinYear=1970);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns true if strWord is recognized as a valid time.  Otherwise, returns false.
//			Valid formats include:
//				08:53:02AM		(HH:MM:SS plus AM or PM or A or P)
//				08:53P			(HH:MM plus AM or PM or A or P)
//				08:53:02		(HH:MM:SS without AM or PM or A or P)
//				20:53:02		(HH:MM:SS Military time)
//				20:53			(HH:MM Military time)
//
EXPORT_BaseUtils bool isValidTime(const string& strWord, long* lHour, long* lMinute, long* lSecond, 
								  bool* bFoundAMPM, bool* bIsAM);
//-------------------------------------------------------------------------------------------------
// PROMISE: Converts the provided SYSTEMTIME argument into a tm struct
EXPORT_BaseUtils tm systemTimeToTm(const SYSTEMTIME &st);
//-------------------------------------------------------------------------------------------------
// PROMISE: Converts the provided tm argument into a SYSTEMTIME struct
EXPORT_BaseUtils SYSTEMTIME tmToSystemTime(const tm &t);
//-------------------------------------------------------------------------------------------------
// PROMISE: Converts the provided SYSTEMTIME argument into a ULONGLONG
EXPORT_BaseUtils ULONGLONG asULongLong(const SYSTEMTIME &st);
//-------------------------------------------------------------------------------------------------
// PROMISE: Formats that provided SYSTEMTIME using a strftime format specification
EXPORT_BaseUtils string formatSystemTime(const SYSTEMTIME &st, const string& strFormat);
//-------------------------------------------------------------------------------------------------
// PROMISE: Converts the CTime argument into a SYSTEMTIME representing the local time
EXPORT_BaseUtils SYSTEMTIME asLocalSystemTime(const CTime &ct);
//-------------------------------------------------------------------------------------------------
// PROMISE: Converts the CTime argument into a SYSTEMTIME representing the UTC time
EXPORT_BaseUtils SYSTEMTIME asUTCSystemTime(const CTime &ct);