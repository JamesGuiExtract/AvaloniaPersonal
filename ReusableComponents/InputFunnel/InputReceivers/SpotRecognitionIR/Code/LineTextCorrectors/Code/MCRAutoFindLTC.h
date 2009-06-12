// MCRAutoFindLTC.h : Declaration of the CMCRAutoFindLTC

#pragma once

#include "resource.h"       // main symbols

#import "..\..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\HighlightedTextIR\Code\InputFinders\Code\InputFinders.tlb"
using namespace UCLID_INPUTFINDERSLib;

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CMCRAutoFindLTC
class ATL_NO_VTABLE CMCRAutoFindLTC : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMCRAutoFindLTC, &CLSID_MCRAutoFindLTC>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILineTextCorrector, &IID_ILineTextCorrector, &LIBID_UCLID_SPOTRECOGNITIONIRLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CMCRAutoFindLTC();

DECLARE_REGISTRY_RESOURCEID(IDR_MCRAUTOFINDLTC)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMCRAutoFindLTC)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILineTextCorrector)
	COM_INTERFACE_ENTRY2(IDispatch, ILineTextCorrector)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);
// ILineTextCorrector
	STDMETHOD(raw_CorrectText)(BSTR strInputText, BSTR strInputType, BSTR * pstrOutputText);
// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	CComQIPtr<IInputFinder> m_ipMCRInputFinder;

	// PURPOSE: To validate proper licensing of this component
	// PROMISE: To throw an exception if this component is not licensed to run
	void validateLicense();

	bool isKnowledgeableOfInputType(const std::string& strInputType);
};

