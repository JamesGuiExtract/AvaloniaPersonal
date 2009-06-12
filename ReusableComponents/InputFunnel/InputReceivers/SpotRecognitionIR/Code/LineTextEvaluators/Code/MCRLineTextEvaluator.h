
#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CMCRLineTextEvaluator
class ATL_NO_VTABLE CMCRLineTextEvaluator : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMCRLineTextEvaluator, &CLSID_MCRLineTextEvaluator>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILineTextEvaluator, &IID_ILineTextEvaluator, &LIBID_UCLID_SPOTRECOGNITIONIRLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CMCRLineTextEvaluator();
	~CMCRLineTextEvaluator();

DECLARE_REGISTRY_RESOURCEID(IDR_MCRLINETEXTEVALUATOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMCRLineTextEvaluator)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILineTextEvaluator)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);
// ILineTextEvaluator
	STDMETHOD(raw_GetTextScore)(BSTR strLineText, BSTR strInputType, LONG * plScore);
// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);
// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	//******* Type defs **********
	enum EMCRType{kBearing = 0, kAngle, kDistance, kNumber};

	//******* Member variables **********
	// result logger for displaying the testing result
	ITestResultLoggerPtr m_ipLogger;

	// string name for bearing, angle, distance input types
	_bstr_t m_bstrBearingInputType;
	_bstr_t m_bstrAngleInputType;
	_bstr_t m_bstrDistanceInputType;
	// input finder
	IInputFinderPtr m_ipMCRInputFinder;
	// vector of token positions for different input types
	IIUnknownVectorPtr m_ipMCRTextPositions;
	// each MCR text string position
	ITokenPtr m_ipTokenPosition;


	//******* Helper functions **********
	// get the total number of characters in the pass-in string.
	// always skip spaces
	long getNumOfChars(const std::string& strInput);
	// get text score according to the mcr text types
	long getTextScore(EMCRType eMCRType, BSTR bstrInputText);
	// validate license
	void validateLicense();
};
