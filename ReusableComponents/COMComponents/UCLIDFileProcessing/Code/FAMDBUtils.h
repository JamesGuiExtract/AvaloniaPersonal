// FAMDBUtils.h : Declaration of the CFAMDBUtils

#pragma once

#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"

/////////////////////////////////////////////////////////////////////////////
// CFAMDBUtils
class ATL_NO_VTABLE CFAMDBUtils : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFAMDBUtils, &CLSID_FAMDBUtils>,
	public ISupportErrorInfo,
	public IDispatchImpl<IFAMDBUtils, &IID_IFAMDBUtils, &LIBID_UCLID_FILEPROCESSINGLib>
{
public:
	CFAMDBUtils();
	~CFAMDBUtils();

DECLARE_REGISTRY_RESOURCEID(IDR_FAMDBUTILS)

BEGIN_COM_MAP(CFAMDBUtils)
	COM_INTERFACE_ENTRY(IFAMDBUtils)
	COM_INTERFACE_ENTRY2(IDispatch, IFAMDBUtils)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

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

// IFAMDBUtils
	STDMETHOD(PromptForActionSelection)(IFileProcessingDB* pDB, BSTR strTitle, BSTR strPrompt, 
			BSTR *pActionName);

private:

	///////////
	//Variables
	//////////

	//////////
	//Methods
	/////////
};

OBJECT_ENTRY_AUTO(__uuidof(FAMDBUtils), CFAMDBUtils)
