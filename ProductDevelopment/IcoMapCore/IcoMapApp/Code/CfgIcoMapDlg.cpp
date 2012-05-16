//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CfgIcoMapDlg.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================
#include "stdafx.h"
#include "CfgIcoMapDlg.h"

#include <IConfigurationSettingsPersistenceMgr.h>
#include <UCLIDException.h>
#include <cpputil.h>

#include <io.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//keys
const string CfgIcoMapDlg::WINDOW_POS_X = "WindowPositionX";
const string CfgIcoMapDlg::WINDOW_POS_Y = "WindowPositionY";
const string CfgIcoMapDlg::WINDOW_SIZE_X = "WindowSizeX";
const string CfgIcoMapDlg::WINDOW_SIZE_Y = "WindowSizeY";
const string CfgIcoMapDlg::COMMAND_RECO_ENABLED = "CommandRecoEnabled";

//---------------------------------------------------------------------------
CfgIcoMapDlg::CfgIcoMapDlg(IConfigurationSettingsPersistenceMgr* pCfgMgr, 
										 string strRootIcoMapDlg)
:m_pCfgMgr(pCfgMgr), m_strRootIcoMapDlg(strRootIcoMapDlg)
{
}

//---------------------------------------------------------------------------
CfgIcoMapDlg::~CfgIcoMapDlg()
{
}

//---------------------------------------------------------------------------
void CfgIcoMapDlg::getWindowPos(long &lPosX, long &lPosY)
{
	// Check for existence of X position
	if (!m_pCfgMgr->keyExists(m_strRootIcoMapDlg, WINDOW_POS_X))
	{
		// Not found, just default to 10
		m_pCfgMgr->createKey(m_strRootIcoMapDlg, WINDOW_POS_X, "10");
		lPosX = 10;
	}
	else
	{
		// Retrieve X position
		string strX( m_pCfgMgr->getKeyValue(m_strRootIcoMapDlg, WINDOW_POS_X) );
		lPosX = asLong( strX );
		if (lPosX < 10)
		{
			lPosX = 10;
		}
	}

	// Check for existence of Y position
	if (!m_pCfgMgr->keyExists(m_strRootIcoMapDlg, WINDOW_POS_Y))
	{
		// Not found, just default to 10
		m_pCfgMgr->createKey(m_strRootIcoMapDlg, WINDOW_POS_Y, "10");
		lPosY = 10;
	}
	else
	{
		// Retrieve Y position
		string strY ( m_pCfgMgr->getKeyValue(m_strRootIcoMapDlg, WINDOW_POS_Y) );
		lPosY = asLong( strY );
		if (lPosY < 10)
		{
			lPosY = 10;
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CfgIcoMapDlg::setWindowPos(long lPosX, long lPosY)
{
	CString cstrX, cstrY;
	cstrX.Format("%ld", lPosX);
	cstrY.Format("%ld", lPosY);

	// Store strings
	m_pCfgMgr->setKeyValue( m_strRootIcoMapDlg, WINDOW_POS_X, (LPCTSTR)cstrX );
	m_pCfgMgr->setKeyValue( m_strRootIcoMapDlg, WINDOW_POS_Y, (LPCTSTR)cstrY );
}
//--------------------------------------------------------------------------------------------------
void CfgIcoMapDlg::getWindowSize(long &lSizeX, long &lSizeY)
{
	// Check for existence of width
	if (!m_pCfgMgr->keyExists(m_strRootIcoMapDlg, WINDOW_SIZE_X))
	{
		// Not found, just default to 0
		m_pCfgMgr->createKey( m_strRootIcoMapDlg, WINDOW_SIZE_X, "0" );
		lSizeX = 0;
	}
	else
	{
		// Retrieve width
		string strX( m_pCfgMgr->getKeyValue( m_strRootIcoMapDlg, WINDOW_SIZE_X ) );
		lSizeX = asLong( strX );
	}

	// Check for existence of height
	if (!m_pCfgMgr->keyExists(m_strRootIcoMapDlg, WINDOW_SIZE_Y))
	{
		// Not found, just default to 0
		m_pCfgMgr->createKey( m_strRootIcoMapDlg, WINDOW_SIZE_Y, "0" );
		lSizeY = 0;
	}
	else
	{
		// Retrieve height
		string strY( m_pCfgMgr->getKeyValue( m_strRootIcoMapDlg, WINDOW_SIZE_Y ) );
		lSizeY = asLong( strY );
	}
}
//--------------------------------------------------------------------------------------------------
void CfgIcoMapDlg::setWindowSize(long lSizeX, long lSizeY)
{
	CString cstrX, cstrY;
	cstrX.Format("%ld", lSizeX);
	cstrY.Format("%ld", lSizeY);

	// Store strings
	m_pCfgMgr->setKeyValue( m_strRootIcoMapDlg, WINDOW_SIZE_X, (LPCTSTR)cstrX );
	m_pCfgMgr->setKeyValue( m_strRootIcoMapDlg, WINDOW_SIZE_Y, (LPCTSTR)cstrY );
}
//--------------------------------------------------------------------------------------------------
bool CfgIcoMapDlg::getCommandRecoEnabled(bool &bCommandRecoEnabled)
{
	string strKey;

	// Check for existence of value
	if (!m_pCfgMgr->keyExists(m_strRootIcoMapDlg, COMMAND_RECO_ENABLED))
	{
		// Not found, just default to true
		m_pCfgMgr->createKey( m_strRootIcoMapDlg, COMMAND_RECO_ENABLED, "1" );
		bCommandRecoEnabled = true;

		return false;
	}
	else
	{
		// Retrieve value
		strKey = m_pCfgMgr->getKeyValue( m_strRootIcoMapDlg, COMMAND_RECO_ENABLED );
		bCommandRecoEnabled = (asLong( strKey ) != 0);

		return true;
	}
}
//--------------------------------------------------------------------------------------------------
void CfgIcoMapDlg::setCommandRecoEnabled(bool bCommandRecoEnabled)
{
	if(bCommandRecoEnabled)
		m_pCfgMgr->setKeyValue( m_strRootIcoMapDlg, COMMAND_RECO_ENABLED, "1" );
	else
		m_pCfgMgr->setKeyValue( m_strRootIcoMapDlg, COMMAND_RECO_ENABLED, "0" );
}
//--------------------------------------------------------------------------------------------------