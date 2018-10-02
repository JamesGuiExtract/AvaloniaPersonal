// WorkflowDefinition.h : Declaration of the CWorkflowDefinition

#pragma once
#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"

#include <string>


using namespace std;

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CWorkflowDefinition

class ATL_NO_VTABLE CWorkflowDefinition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CWorkflowDefinition, &CLSID_WorkflowDefinition>,
	public ISupportErrorInfo,
	public IDispatchImpl<IWorkflowDefinition, &IID_IWorkflowDefinition, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CWorkflowDefinition();
	~CWorkflowDefinition();

DECLARE_REGISTRY_RESOURCEID(IDR_WORKFLOWDEFINITION)

BEGIN_COM_MAP(CWorkflowDefinition)
	COM_INTERFACE_ENTRY(IWorkflowDefinition)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IWorkflowDefinition
	STDMETHOD(get_ID)(LONG* pnID);
	STDMETHOD(put_ID)(LONG nID);
	STDMETHOD(get_Type)(EWorkflowType* peWorkflowType);
	STDMETHOD(put_Type)(EWorkflowType eWorkflowType);
	STDMETHOD(get_Name)(BSTR* pName);
	STDMETHOD(put_Name)(BSTR Name);
	STDMETHOD(get_Description)(BSTR* pDescription);
	STDMETHOD(put_Description)(BSTR Description);
	STDMETHOD(get_StartAction)(BSTR* pStartAction);
	STDMETHOD(put_StartAction)(BSTR StartAction);
	STDMETHOD(get_EndAction)(BSTR* pEndAction);
	STDMETHOD(put_EndAction)(BSTR EndAction);
	STDMETHOD(get_PostWorkflowAction)(BSTR* pPostWorkflowAction);
	STDMETHOD(put_PostWorkflowAction)(BSTR PostWorkflowAction);
	STDMETHOD(get_DocumentFolder)(BSTR* pDocumentFolder);
	STDMETHOD(put_DocumentFolder)(BSTR DocumentFolder);
	STDMETHOD(get_OutputAttributeSet)(BSTR* pOutputAttributeSet);
	STDMETHOD(put_OutputAttributeSet)(BSTR OutputAttributeSet);
	STDMETHOD(get_OutputFileMetadataField)(BSTR* pOutputFileMetadataField);
	STDMETHOD(put_OutputFileMetadataField)(BSTR OutputFileMetadataField);
	STDMETHOD(get_OutputFilePathInitializationFunction)(BSTR* pVal);
	STDMETHOD(put_OutputFilePathInitializationFunction)(BSTR newVal);
	STDMETHOD(get_LoadBalanceWeight)(LONG* pnWeight);
	STDMETHOD(put_LoadBalanceWeight)(LONG nWeight);
	STDMETHOD(get_EditAction)(BSTR* pEditAction);
	STDMETHOD(put_EditAction)(BSTR EditAction);
	STDMETHOD(get_PostEditAction)(BSTR* pPostEditAction);
	STDMETHOD(put_PostEditAction)(BSTR PostEditAction);

private:

	// Variables
	long m_nID;
	EWorkflowType m_eType;
	string m_strName;
	string m_strDescription;
	string m_strStartAction;
	string m_strEndAction;
	string m_strPostWorkflowAction;
	string m_strDocumentFolder;
	string m_strOutputAttributeSet;
	string m_strOutputFileMetadataField;
	string m_strOutputFilePathInitializationFunction;
	string m_strEditAction;
	string m_strPostEditAction;
	long m_nLoadBalanceWeight;
};

OBJECT_ENTRY_AUTO(__uuidof(WorkflowDefinition), CWorkflowDefinition)
