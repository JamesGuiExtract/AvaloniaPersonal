// TestSafeNetUtils.h : Declaration of the CTestSafeNetUtils
#pragma once

#include "resource.h"       // main symbols
#include "SafeNetLicenseMgr.h"
#include <UCLIDException.h>

/////////////////////////////////////////////////////////////////////////////
// CTestSafeNetUtils
class ATL_NO_VTABLE CTestSafeNetUtils : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTestSafeNetUtils, &CLSID_TestSafeNetUtils>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestSafeNetUtils, &IID_ITestSafeNetUtils, &LIBID_SAFENETUTILSTESTLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CTestSafeNetUtils();

DECLARE_REGISTRY_RESOURCEID(IDR_TESTSAFENETUTILS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTestSafeNetUtils)
	COM_INTERFACE_ENTRY(ITestSafeNetUtils)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ITestSafeNetUtils)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITestSafeNetUtils
public:
// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector * pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);
	

private:
	friend UINT decrementThreadFunc(void *pData);
	friend UINT incrementThreadFunc(void *pData);
	////////////
	// Variables
	////////////

	ITestResultLoggerPtr m_ipResultLogger;

	SafeNetLicenseMgr m_snlmLicense;
	// Class for holding the each threads data
	class ThreadDataClass
	{
	public:
		ThreadDataClass(SafeNetLicenseMgr &rsnlmLM, DataCell &rdcCell );
		~ThreadDataClass();
		Win32Event m_threadStartedEvent;
		Win32Event m_threadEndedEvent;
		Win32Event m_threadStopRequest;
		CWinThread* m_pThread;
		UCLIDException m_ue;
		bool m_bException;
		SafeNetLicenseMgr &m_rsnlmLM;
		DataCell &m_rdcCell;
		void resetEvents();
	};
	
	//////////
	// Methods
	//////////
	
	// This test case tests the increment of each of the counters
	void runTestCase1();

	// This test case tests the decrement of each of the counters
	void runTestCase2();

	// This test case tests the decrement of each of the counters with multiple threads
	void runTestCase3();

	// This test case tests the auto unlock
	void runTestCase4();
};
