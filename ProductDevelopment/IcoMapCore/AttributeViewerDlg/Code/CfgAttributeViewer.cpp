//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CfgAttributeViewer.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================
#include "stdafx.h"
#include "CfgAttributeViewer.h"

#include <IConfigurationSettingsPersistenceMgr.h>
#include <UCLIDException.h>
#include <cpputil.h>

#include <io.h>

using namespace std;

//keys
const string CfgAttributeViewer::WINDOW_POS_X = "WindowPositionX";
const string CfgAttributeViewer::WINDOW_POS_Y = "WindowPositionY";
const string CfgAttributeViewer::WINDOW_SIZE_X = "WindowSizeX";
const string CfgAttributeViewer::WINDOW_SIZE_Y = "WindowSizeY";
const string CfgAttributeViewer::SHOW_STORED = "ShowStored";

//---------------------------------------------------------------------------
CfgAttributeViewer::CfgAttributeViewer(IConfigurationSettingsPersistenceMgr* pUserCfgMgr, 
										 string strRootAttributeViewer)
:m_pUserCfgMgr(pUserCfgMgr), m_strRootAttributeViewer(strRootAttributeViewer)
{
}

//---------------------------------------------------------------------------
CfgAttributeViewer::~CfgAttributeViewer()
{
}

//---------------------------------------------------------------------------
void CfgAttributeViewer::getWindowPos(long &lPosX, long &lPosY)
{
	// Check for existence of X position
	if (!m_pUserCfgMgr->keyExists(m_strRootAttributeViewer, WINDOW_POS_X))
	{
		// Not found, just default to 10
		m_pUserCfgMgr->createKey(m_strRootAttributeViewer, WINDOW_POS_X, "10");
		lPosX = 10;
	}
	else
	{
		// Retrieve X position
		string strX( m_pUserCfgMgr->getKeyValue(m_strRootAttributeViewer, WINDOW_POS_X) );
		lPosX = asLong( strX );
	}

	// Check for existence of Y position
	if (!m_pUserCfgMgr->keyExists(m_strRootAttributeViewer, WINDOW_POS_Y))
	{
		// Not found, just default to 10
		m_pUserCfgMgr->createKey(m_strRootAttributeViewer, WINDOW_POS_Y, "10");
		lPosY = 10;
	}
	else
	{
		// Retrieve Y position
		string strY( m_pUserCfgMgr->getKeyValue(m_strRootAttributeViewer, WINDOW_POS_Y) );
		lPosY = asLong( strY );
	}
}
//--------------------------------------------------------------------------------------------------
void CfgAttributeViewer::setWindowPos(long lPosX, long lPosY)
{
	CString cstrX, cstrY;
	cstrX.Format("%ld", lPosX);
	cstrY.Format("%ld", lPosY);

	// Store strings
	m_pUserCfgMgr->setKeyValue( m_strRootAttributeViewer, WINDOW_POS_X, (LPCTSTR)cstrX );
	m_pUserCfgMgr->setKeyValue( m_strRootAttributeViewer, WINDOW_POS_Y, (LPCTSTR)cstrY );
}
//--------------------------------------------------------------------------------------------------
void CfgAttributeViewer::getWindowSize(long &lSizeX, long &lSizeY)
{
	// Check for existence of width
	if (!m_pUserCfgMgr->keyExists(m_strRootAttributeViewer, WINDOW_SIZE_X))
	{
		// Not found, just default to 330
		m_pUserCfgMgr->createKey( m_strRootAttributeViewer, WINDOW_SIZE_X, "280" );
		lSizeX = 330;
	}
	else
	{
		// Retrieve width
		string strX( m_pUserCfgMgr->getKeyValue( m_strRootAttributeViewer, WINDOW_SIZE_X ) );
		lSizeX = asLong( strX );
	}

	// Check for existence of height
	if (!m_pUserCfgMgr->keyExists(m_strRootAttributeViewer, WINDOW_SIZE_Y))
	{
		// Not found, just default to 250
		m_pUserCfgMgr->createKey( m_strRootAttributeViewer, WINDOW_SIZE_Y, "250" );
		lSizeY = 250;
	}
	else
	{
		// Retrieve height
		string strY( m_pUserCfgMgr->getKeyValue( m_strRootAttributeViewer, WINDOW_SIZE_Y ) );
		lSizeY = asLong( strY );
	}
}
//--------------------------------------------------------------------------------------------------
void CfgAttributeViewer::setWindowSize(long lSizeX, long lSizeY)
{
	CString cstrX, cstrY;
	cstrX.Format("%ld", lSizeX);
	cstrY.Format("%ld", lSizeY);

	// Store strings
	m_pUserCfgMgr->setKeyValue( m_strRootAttributeViewer, WINDOW_SIZE_X, (LPCTSTR)cstrX );
	m_pUserCfgMgr->setKeyValue( m_strRootAttributeViewer, WINDOW_SIZE_Y, (LPCTSTR)cstrY );
}
//--------------------------------------------------------------------------------------------------
bool CfgAttributeViewer::getShowStored()
{
	bool	bShow = false;
	string	strShow;

	// Check for existence of key
	if (!m_pUserCfgMgr->keyExists(m_strRootAttributeViewer, SHOW_STORED))
	{
		// Not found, just default to false
		m_pUserCfgMgr->createKey( m_strRootAttributeViewer, SHOW_STORED, "0" );
	}
	else
	{
		// Retrieve value
		strShow = m_pUserCfgMgr->getKeyValue( m_strRootAttributeViewer, SHOW_STORED );
		
		// Check for visibility
		if (strcmp( strShow.c_str(), "1" ) == 0)
		{
			bShow = true;
		}
	}

	return bShow;
}
//--------------------------------------------------------------------------------------------------
void CfgAttributeViewer::setShowStored(bool bShow)
{	
	// Store string
	m_pUserCfgMgr->setKeyValue( m_strRootAttributeViewer, SHOW_STORED, 
		bShow ? "1" : "0" );
}
//--------------------------------------------------------------------------------------------------
