// ProgressStatus.h : Declaration of the CProgressStatus

#pragma once

#include "resource.h"       // main symbols
#include "UCLIDCOMUtils.h"

#include <string>

//--------------------------------------------------------------------------------------------------
// CProgressStatus
//
// NOTES:
//		(a)	The ILicensedComponent interface has not been implemented for this object because of
//			the frequency at which this object is going to get created/updated and the concern that
//			by checking the license state we may be causing a slow down that is possibly noticable.
//
//--------------------------------------------------------------------------------------------------

class ATL_NO_VTABLE CProgressStatus :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CProgressStatus, &CLSID_ProgressStatus>,
	public ISupportErrorInfo,
	public IDispatchImpl<IProgressStatus, &IID_IProgressStatus, &LIBID_UCLID_COMUTILSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CProgressStatus();
	~CProgressStatus();

DECLARE_REGISTRY_RESOURCEID(IDR_PROGRESSSTATUS)

BEGIN_COM_MAP(CProgressStatus)
	COM_INTERFACE_ENTRY(IProgressStatus)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IProgressStatus
	STDMETHOD(InitProgressStatus)(/*[in]*/ BSTR strText, 
			/*[in]*/ long nNumItemsCompleted, /*[in]*/ long nNumItemsTotal,
			/*[in]*/ VARIANT_BOOL bCreateOrResetSubProgressStatus);
	STDMETHOD(StartNextItemGroup)(/*[in]*/ BSTR strNextGroupText, 
			/*[in]*/ long nNumItemsInNextGroup);
	STDMETHOD(CompleteCurrentItemGroup)();
	STDMETHOD(get_Text)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(get_NumItemsTotal)(/*[out, retval]*/ long *pVal);
	STDMETHOD(get_NumItemsCompleted)(/*[out, retval]*/ long *pVal);
	STDMETHOD(get_NumItemsInCurrentGroup)(/*[out, retval]*/ long *pVal);
	STDMETHOD(get_SubProgressStatus)(/*[out, retval]*/ IProgressStatus* *pVal);
	STDMETHOD(put_SubProgressStatus)(/*[in]*/ IProgressStatus* newVal);
	STDMETHOD(GetProgressPercent)(/*[out, retval]*/ double *pVal);
	STDMETHOD(ResetSubProgressStatus)();
	STDMETHOD(CompleteProgressItems)(/*[in]*/ BSTR strNextGroupText, 
			/*[in]*/ long nNumItemsCompleted);

private:
	//----------------------------------------------------------------------------------------------
	// Private members
	//----------------------------------------------------------------------------------------------
	// This mutex is used to synchronize access to this object.
	CMutex m_objLock;

	// The textual status
	std::string m_strText;
	
	// Counts related to keeping progress.
	LONG m_nNumItemsTotal;
	LONG m_nNumItemsCompleted;

	// The number of items in the current progress group.  The number of items completed
	// is incremented by this number when the StartNextProgressItemGroup() or 
	// CompleteCurrentProgressGroup() methods are called.
	LONG m_nNumItemsInCurrentGroup;

	// Sub-progress-status for keeping track of progress at lower levels.
	// It is OK for this pointer to be NULL during the normal course of
	// operation.  A NULL sub progress status object pointer just means that we are not 
	// tracking progress at the next lower level of operation.
	UCLID_COMUTILSLib::IProgressStatusPtr m_ipSubProgressStatus;

	//----------------------------------------------------------------------------------------------
	// Private / Helper functions
	//----------------------------------------------------------------------------------------------
	
	// Ensure m_nNumItemsTotal >= m_nNumItemsCompleted
	void validateItemCounts();

	UCLID_COMUTILSLib::IProgressStatusPtr getThisAsCOMPtr();
};
//--------------------------------------------------------------------------------------------------

OBJECT_ENTRY_AUTO(__uuidof(ProgressStatus), CProgressStatus)
