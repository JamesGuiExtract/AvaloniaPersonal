//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CfgIcoMapDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include <string>

class IConfigurationSettingsPersistenceMgr;

class CfgIcoMapDlg 
{
public:
	static const std::string WINDOW_POS_X;
	static const std::string WINDOW_POS_Y;
	static const std::string WINDOW_SIZE_X;
	static const std::string WINDOW_SIZE_Y;
	static const std::string COMMAND_RECO_ENABLED;

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Points to persistence manager
	//
	CfgIcoMapDlg(IConfigurationSettingsPersistenceMgr* pCfgMgr, 
		std::string strRootIcoMapDlg);
	
	//----------------------------------------------------------------------------------------------
	virtual ~CfgIcoMapDlg();
	
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns the left and top values of the window position
	//
	// PROMISE:	
	//
	void getWindowPos(long &lPosX, long &lPosY);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Stores the left and top values of the window position
	//
	void setWindowPos(long lPosX, long lPosY);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns the width and height values of the window size
	//
	// PROMISE:	
	//
	void getWindowSize(long &lSizeX, long &lSizeY);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Stores the width and height values of the window size
	//
	void setWindowSize(long lSizeX, long lSizeY);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns the "Command Recognition Enabled" setting
	//
	bool getCommandRecoEnabled(bool &bCommandRecoEnabled);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Stores the "Command Recognition Enabled" setting
	//
	void setCommandRecoEnabled(bool bCommandRecoEnabled);

private:
	IConfigurationSettingsPersistenceMgr* m_pCfgMgr;
	std::string m_strRootIcoMapDlg;
};
