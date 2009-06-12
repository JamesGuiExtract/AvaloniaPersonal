// GrantorGranteeFinderV2PP.h : Declaration of the CGrantorGranteeFinderV2PP

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <vector>
#include <map>

EXTERN_C const CLSID CLSID_GrantorGranteeFinderV2PP;

/////////////////////////////////////////////////////////////////////////////
// CGrantorGranteeFinderV2PP
class ATL_NO_VTABLE CGrantorGranteeFinderV2PP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CGrantorGranteeFinderV2PP, &CLSID_GrantorGranteeFinderV2PP>,
	public IPropertyPageImpl<CGrantorGranteeFinderV2PP>,
	public CDialogImpl<CGrantorGranteeFinderV2PP>
{
public:
	CGrantorGranteeFinderV2PP(); 

	enum {IDD = IDD_GRANTORGRANTEEFINDERV2PP};

DECLARE_REGISTRY_RESOURCEID(IDR_GRANTORGRANTEEFINDERV2PP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CGrantorGranteeFinderV2PP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CGrantorGranteeFinderV2PP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CGrantorGranteeFinderV2PP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_AS_SPECIFIED, BN_CLICKED, OnClickedRadioSpecified)
	COMMAND_HANDLER(IDC_ALL, BN_CLICKED, OnClickedRadioAll)
	COMMAND_HANDLER(IDC_LIST_DOC_TYPES, LBN_SELCHANGE, OnSelChangeListDocType)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedRadioSpecified(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioAll(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnSelChangeListDocType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	/////////////
	// Variables
	/////////////
	ATLControls::CListViewCtrl m_listFindParties;
	ATLControls::CListBox m_listDocTypes;
	ATLControls::CButton m_radioAll;
	ATLControls::CButton m_radioSpecified;

	IAFUtilityPtr m_ipAFUtil;

	// Vector for the document type strings
	std::vector<std::string> m_vecDocTypes;

	// Map of DocType to vector of all files for that doc type
	std::map< std::string, std::vector<std::string> > m_mapDocTypeToVecFinderFile;

	// Map of DocType to vector of all Selected files for that doc type
	std::map< std::string, std::vector<std::string> > m_mapSelDocTypeToVecFinderFile;

	// The Current Doc Type displayed in the Parties list
	std::string m_strCurrDocType;

	/////////////
	// Methods
	/////////////

	// Loads the list box list with all doc types from .idx file in 
	// DocumentClassifiers\County Document directory
	void loadDocTypeList();

	// Loads the list of files for the given doc type into the Parties list
	void loadFindPartiesList(const std::string& strDocType );

	// Enables and disables combo, and list controls based on radio button selection
	void updateControls();

	// Returns the vector of dat file names for the doc type from the DocType subdirectory
	std::vector<std::string> getDatFileNamesForDocType(const std::string& strDocType );

	// Loads the mapDocTypeToVecFinderFile map using m_vecDocTypes and getDatFileNamesForDocType
	// Requires m_vecDocTypes to already contain doctypes
	void loadDocTypeToVecFinderFile();

	// Returns the path for the rules folder
	std::string getRulesFolder();

	// Saves Selected DatFiles to the GrantorGranteeV2 object
	void saveSelDatFilesForDocType(UCLID_COUNTYCUSTOMCOMPONENTSLib::IGrantorGranteeFinderV2Ptr ipGGFinderV2);
	
	// Retrieves the Selected DatFiles from GrantorGranteeFinderV2 object and updates the m_mapSelDocTypeToVecFinderFile map
	void loadSelDatFilesForDocType(UCLID_COUNTYCUSTOMCOMPONENTSLib::IGrantorGranteeFinderV2Ptr ipGGFinderV2);

	// Updates the mapSelDocTypeToVecFinderFile with the selected items in the Parties list
	void getSelectedFromList();
	
};

