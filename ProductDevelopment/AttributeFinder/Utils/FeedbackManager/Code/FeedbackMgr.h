// FeedbackMgr.h : Declaration of the CFeedbackMgr

#pragma once

#include "resource.h"       // main symbols
#include "PersistenceMgr.h"

#include <Stopwatch.h>
#include <afxmt.h>

#include <map>
#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CFeedbackMgr
class ATL_NO_VTABLE CFeedbackMgr : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFeedbackMgr, &CLSID_FeedbackMgr>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IFeedbackMgr, &IID_IFeedbackMgr, &LIBID_UCLID_FEEDBACKMANAGERLib>,
	public IDispatchImpl<IFeedbackMgrInternals, &IID_IFeedbackMgrInternals, &LIBID_UCLID_AFCORELib>
{
public:
	CFeedbackMgr();
	~CFeedbackMgr();

DECLARE_REGISTRY_RESOURCEID(IDR_FEEDBACKMGR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CFeedbackMgr)
	COM_INTERFACE_ENTRY(IFeedbackMgr)
	COM_INTERFACE_ENTRY2(IDispatch, IFeedbackMgr)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IFeedbackMgrInternals)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

// IFeedbackMgr
	STDMETHOD(raw_RecordCorrectData)(BSTR strRuleExecutionID, IIUnknownVector* pData);

// IFeedbackMgrInternals
	STDMETHOD(raw_RecordFoundData)(BSTR bstrRuleExecutionID, IIUnknownVector * pData);
	STDMETHOD(raw_RecordException)(BSTR bstrRuleExecutionID, BSTR bstrException);
	STDMETHOD(raw_RecordRuleExecution)(IAFDocument *pAFDoc, BSTR bstrRSDFileName,
		BSTR* pbstrRuleExecutionID);
	STDMETHOD(raw_ClearFeedbackData)(VARIANT_BOOL bShowPrompt);
	STDMETHOD(raw_GetFeedbackRecords)(IUnknown** ppFeedbackRecords);
	STDMETHOD(raw_CloseConnection)();

private:
	//////////////
	// Variables
	//////////////

	// Handles settings persistence
	unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	unique_ptr<PersistenceMgr> ma_pCfgFeedbackMgr;

	// DB Connection is open
	bool		m_bConnectionOpen;

	// ADO Connection Object
	_ConnectionPtr	m_ipConnection;

	// Map of Duration timers
	map<long, StopWatch>	m_mapDurationTimers;

	// Static variable to indicate that connection failed and 
	// Feedback will not be collected
	static bool	ms_bDisableSessionFeedback;

	static CMutex m_sMutex;

	//////////////
	// Methods
	//////////////

	// Updates the Source Document field for the specified database record.  Also
	// copies the Source Document file to the Feedback folder, if appropriate
	void	handleSourceDoc(long lRuleID, IAFDocument *pAFDoc);

	// Opens connection to database.
	// Returns false only if a connection to the database could not be created.
	bool	openDBConnection(const string& strFeedbackFolder);

	// Updates the Computer field for the specified database record
	void	writeComputerName(long lRuleID, const string& strName);

	// Updates the CorrectTime field for the specified database record
	void	writeCorrectTime(long lRuleID, __time64_t t64Time);

	// Updates the Duration field for the specified database record
	void	writeDuration(long lRuleID, double dSeconds);

	// Adds a new record to the database and sets Start Time as specified
	long	writeNewStartTime(__time64_t t64Time);

	// Updates the RSD File field for the specified database record
	void	writeRSDFileName(long lRuleID, const string& strRSDFileName);

	// Checks license state
	void	validateLicense();
};
