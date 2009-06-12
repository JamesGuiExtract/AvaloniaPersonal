//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LMData.cpp
//
// PURPOSE:	Implementation of the LicenseManagement ComponentData class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#include "stdafx.h"
#include "ComponentData.h"
#include "UCLIDException.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// ComponentData
//-------------------------------------------------------------------------------------------------
ComponentData::ComponentData() :
m_bDisabled(false),
m_bIsLicensed(false),
m_ExpirationDate(CTime::GetCurrentTime())
{
}
//-------------------------------------------------------------------------------------------------
ComponentData::ComponentData(bool bIsLicensed, const CTime& ExpirationDate) :
m_bDisabled(false),
m_bIsLicensed(bIsLicensed),
m_ExpirationDate(ExpirationDate)
{
}
//-------------------------------------------------------------------------------------------------
ComponentData::ComponentData(const ComponentData& Data) :
m_bDisabled(Data.m_bDisabled),
m_bIsLicensed(Data.m_bIsLicensed),
m_ExpirationDate(Data.m_ExpirationDate)
{
}
//-------------------------------------------------------------------------------------------------
ComponentData::~ComponentData()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16430");
}
//-------------------------------------------------------------------------------------------------
bool ComponentData::isExpired()
{
	bool	bResult = false;

	// Check for state of license
	if (m_bIsLicensed)
	{
		bResult = true;
	}
	else
	{
		// Get current time
		// Check expiration date against today
		CTime	today;
		today = CTime::GetCurrentTime();

		// Get time at beginning of today
		CTime	earlyToday( today.GetYear(), today.GetMonth(), today.GetDay(), 
			0, 0, 1 );

		// This test allows for an expiration date (and time) of sometime 
		// today to always be accepted.  No assumption that expiration time 
		// = 11:59:59 P.M. is required.
		if (m_ExpirationDate < earlyToday)
		{
			bResult = true;
		}
	}

	// Provide result to caller
	return bResult;
}
//-------------------------------------------------------------------------------------------------
ComponentData& ComponentData::operator =(const ComponentData& Data)
{
	// Copy the data members
	this->m_bIsLicensed = Data.m_bIsLicensed;  
	this->m_ExpirationDate = Data.m_ExpirationDate;      
	this->m_bDisabled = Data.m_bDisabled;

	return *this;
}
//-------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, 
									ComponentData Data)
{
	// Write the license state
	rManipulator << Data.m_bIsLicensed;

	// Write the expiration date
	rManipulator << Data.m_ExpirationDate;

	return rManipulator;
}
//-------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, 
									ComponentData& rData)
{
	// Read the license state
	rManipulator >> rData.m_bIsLicensed;

	// Read the expiration date
	rManipulator >> rData.m_ExpirationDate;

	// Set the disabled state to false
	rData.m_bDisabled = false;

	return rManipulator;
}
//-------------------------------------------------------------------------------------------------
