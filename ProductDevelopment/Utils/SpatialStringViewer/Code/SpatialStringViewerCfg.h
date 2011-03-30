
#pragma once

#include <IConfigurationSettingsPersistenceMgr.h>
#include <memory>

#include <vector>
#include <string>

// forward declarations
class CSpatialStringViewerDlg;

class SpatialStringViewerCfg
{
public:
	//---------------------------------------------------------------------------------------------
	// REQUIRE: pDlg is a pointer to the dialog whose window
	//			position needs to be restored or saved
	//			Also, pDlg should be a valid pointer for the lifetime of this
	//			object.
	SpatialStringViewerCfg(CSpatialStringViewerDlg *pDlg);
	~SpatialStringViewerCfg();
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To save m_pDlg's current window position in the registry
	void saveCurrentWindowPosition();
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To restore m_pDlg's last saved window position from
	//			the registry.
	void restoreLastWindowPosition();
	//---------------------------------------------------------------------------------------------
	// whether or not advanced in Find dialog is shown
	bool isAdvancedShown();
	//---------------------------------------------------------------------------------------------
	// whether or not to show advanced in Find dialog
	void showAdvanced(bool bShow = false);
	//---------------------------------------------------------------------------------------------
	// get or save the last used regular expression patterns
	std::string getLastRegularExpression();
	void saveLastRegularExpression(std::string strRegExp);
	//---------------------------------------------------------------------------------------------
	// get or save the last find window position
	void getLastFindWindowPos(int& x, int& y);
	void saveLastFindWindowPos(int x, int y);
	//---------------------------------------------------------------------------------------------
	// get or save the last word length distribution window position
	void getLastDistributionWindowPos(int&x, int& y);
	void saveLastDistributionWindowPos(int x, int y);

private:
	
	// pointer to the window whose position is to be saved/restored
	CSpatialStringViewerDlg *m_pDlg;

	// pointer to the persistence manager for registry access
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> m_apCfgMgr;
};