// LegalDescriptionTextCorrector.h : Declaration of the CLegalDescriptionTextCorrector

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CLegalDescriptionTextCorrector
class ATL_NO_VTABLE CLegalDescriptionTextCorrector : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLegalDescriptionTextCorrector, &CLSID_LegalDescriptionTextCorrector>,
	public ISupportErrorInfo,
	public IDispatchImpl<IParagraphTextCorrector, &IID_IParagraphTextCorrector, &LIBID_UCLID_SPOTRECOGNITIONIRLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLegalDescriptionTextCorrector();
	~CLegalDescriptionTextCorrector();

DECLARE_REGISTRY_RESOURCEID(IDR_LEGALDESCRIPTIONTEXTCORRECTOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLegalDescriptionTextCorrector)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IParagraphTextCorrector)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);
// IParagraphTextCorrector
	STDMETHOD(raw_CorrectText)(ISpatialString *pTextToCorrect);
// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);
// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	ITestResultLoggerPtr m_ipLogger;

	void removeWhiteSpaceInWord(ISpatialStringPtr ipText, const std::string& strWord);
	void cleanupOCRedParagraphText(ISpatialStringPtr ipText);
	void validateLicense();
};

