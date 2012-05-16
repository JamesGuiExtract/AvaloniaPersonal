//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CfgAttributeViewer.h
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

#include "AttributeViewerDLL.h"

#include <string>

class IConfigurationSettingsPersistenceMgr;

class CLASS_DECL_AttributeViewerDLL CfgAttributeViewer 
{
public:
	static const std::string WINDOW_POS_X;
	static const std::string WINDOW_POS_Y;
	static const std::string WINDOW_SIZE_X;
	static const std::string WINDOW_SIZE_Y;
	static const std::string SHOW_STORED;

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Points to persistence manager
	//
	CfgAttributeViewer(IConfigurationSettingsPersistenceMgr* pUserCfgMgr, 
		std::string strRootAttributeViewer);
	
	//----------------------------------------------------------------------------------------------
	virtual ~CfgAttributeViewer();
	
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
	// PURPOSE:  Returns visibility of Stored (Original) attributes
	//
	// PROMISE:	
	//
	bool getShowStored();

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Stores visibility of Stored (Original) attributes
	//
	void setShowStored(bool bShow);

private:
	IConfigurationSettingsPersistenceMgr* m_pUserCfgMgr;
	std::string m_strRootAttributeViewer;
};
