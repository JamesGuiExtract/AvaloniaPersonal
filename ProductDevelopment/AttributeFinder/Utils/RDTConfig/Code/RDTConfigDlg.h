// RDTConfigDlg.h : header file
//

#pragma once

#include <IConfigurationSettingsPersistenceMgr.h>
#include <MRUList.h>

/////////////////////////////////////////////////////////////////////////////
// CRDTConfigDlg dialog

class CRDTConfigDlg : public CDialog
{
// Construction
public:
	CRDTConfigDlg(CWnd* pParent = NULL);	// standard constructor
	~CRDTConfigDlg(){};

// Dialog Data
	//{{AFX_DATA(CRDTConfigDlg)
	enum { IDD = IDD_RDTCONFIG_DIALOG };
	CComboBox	m_comboPrefix;
	CComboBox	m_comboRoot;
	CComboBox	m_comboData;
	BOOL	m_bAutoEncrypt;
	BOOL	m_bEFALog;
	BOOL	m_bENDSLog;
	BOOL	m_bLoadOnce;
	BOOL	m_bRuleIDTag;
	BOOL	m_bScrollLogger;
	BOOL	m_bDisplaySRWPercent;
	BOOL	m_bAutoOpenImage;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRDTConfigDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CRDTConfigDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnApply();
	afx_msg void OnBrowseComponent();
	afx_msg void OnBrowseRoot();
	afx_msg void OnDefaults();
	virtual void OnOK();
	afx_msg void OnUpdatePrefix();
	afx_msg void OnKillFocusPrefix();
	afx_msg void OnCheckAutoencrypt();
	afx_msg void OnCheckEfalog();
	afx_msg void OnCheckEndslog();
	afx_msg void OnCheckLoadonce();
	afx_msg void OnCheckRuleidtag();
	afx_msg void OnCheckScrolllogger();
	afx_msg void OnCheckSRWPercent();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	///////
	// Data
	///////

	// Handles Registry items
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pSettingsCfgMgr;

	// Stores collection of most recently chosen DCC and DAT file prefixes
	std::auto_ptr<MRUList>	ma_pRecentPrefixes;

	// Stores collection of most recently chosen Automated Testing root folders
	std::auto_ptr<MRUList>	ma_pRecentRootFolders;

	// Stores collection of most recently chosen Component Data folders
	std::auto_ptr<MRUList>	ma_pRecentDataFolders;

	// Flags to determine if editing is in progress in the Prefix combo box
	bool	m_bEditingPrefix;

	// Registry setting for the command line diff util
	CString		m_zDiffCommandLine;

	//////////
	// Methods
	//////////
	// Adds default strings to combo boxes
	void	addDefaultItems();

	// Adds string to the specified MRU list
	void	addStringToMRUList(std::auto_ptr<MRUList> &ma_pList, const std::string& strNew);

	// Gets and sets AutoEncrypt registry entry
	bool	getAutoEncrypt();
	void	setAutoEncrypt(bool bNewSetting);

	// Gets and sets ComponentData folder registry entry
	std::string	getDataFolder();
	void	setDataFolder(std::string strNewFolder);

	// Gets and sets Entity Finder logging registry entry
	bool	getEFALogging();
	void	setEFALogging(bool bNewSetting);

	// Gets and sets Entity Name Data Scorer logging registry entry
	bool	getENDSLogging();
	void	setENDSLogging(bool bNewSetting);

	// Gets and sets Load Oncer Per Session registry entry
	bool	getLoadOncePerSession();
	void	setLoadOncePerSession(bool bNewSetting);

	// Gets and sets DCC and DAT file prefix registry entry
	std::string	getPrefix();
	void	setPrefix(std::string strNewPrefix);

	// Gets and sets all registry entries
	void	getRegistrySettings();
	void	saveRegistrySettings();

	// Gets and sets Automated Testing Root folder registry entry
	std::string	getRootFolder();
	void	setRootFolder(std::string strNewFolder);

	// Gets and sets Store Rules Worked registry entry
	bool	getRuleIDTag();
	void	setRuleIDTag(bool bNewSetting);

	// Loads previously chosen strings into appropriate combo boxes
	void	loadMRUListItems();

	// Gets and sets Scoll Logger test results registry entry
	bool	getScrollLogger();
	void	setScrollLogger(bool bNewSetting);

	// Gets and sets Display Spot Recognition Window percentage registry entry
	bool	getDisplaySRWPercent();
	void	setDisplaySRWPercent(bool bNewSetting);

	// Gets and sets VOA viewer auto-highlight registry entry
	bool	getAutoOpenImage();
	void	setAutoOpenImage(bool bNewSetting);

	// Return the string for the command line to use to diff the files. The user can specify
	// any program they want to use, replacing the file names with the placeholders %1 and %2. 
	// Validation is not done, if an invalid string is entered, the user can fix it by restoring defaults.
	// c:\program files\DIFFPROGRAM.EXE %1 %2 would be an example
	CString getDiffCommandString();

	// Set the diff string in the registry
	void	setDiffCommandString( CString zDiffString );

private:
	// Registry keys for information persistence
	static const std::string SETTINGS_SECTION;
	static const std::string ENTITYFINDER_SECTION;
	static const std::string GRANTORGRANTEE_SECTION;
	static const std::string TESTING_SECTION;
	static const std::string ENTITYNAMEDATASCORER_SECTION;
	static const std::string SPOTRECOGNITION_SECTION;
	static const std::string VOAVIEWER_SECTION;

	static const std::string AUTOENCRYPT_KEY;
	static const std::string LOADONCE_KEY;
	static const std::string EFALOG_KEY;
	static const std::string RULEIDTAG_KEY;
	static const std::string PREFIX_KEY;
	static const std::string ROOTFOLDER_KEY;
	static const std::string DATAFOLDER_KEY;
	static const std::string SCROLLLOGGER_KEY;
	static const std::string ENDSLOG_KEY;
	static const std::string DISPLAY_PERCENTAGE_KEY;
	static const std::string DIFF_COMMAND_LINE_KEY;
	static const std::string AUTOOPENIMAGE_KEY;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
