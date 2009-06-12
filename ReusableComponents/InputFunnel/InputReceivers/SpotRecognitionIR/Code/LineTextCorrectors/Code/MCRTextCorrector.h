// MCRTextCorrector.h : Declaration of the CMCRTextCorrector

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CMCRTextCorrector
class ATL_NO_VTABLE CMCRTextCorrector : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMCRTextCorrector, &CLSID_MCRTextCorrector>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILineTextCorrector, &IID_ILineTextCorrector, &LIBID_UCLID_SPOTRECOGNITIONIRLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CMCRTextCorrector();
	~CMCRTextCorrector();

DECLARE_REGISTRY_RESOURCEID(IDR_MCRTEXTCORRECTOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMCRTextCorrector)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILineTextCorrector)
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
	void preProcessMCRInput(std::string& strInput);
	void preProcessBearingInput(std::string& strInput);
	void preProcessDistanceInput(std::string& strInput);
	void preProcessAngleInput(std::string& strInput);

	// Support functions for above preProcessXyz() methods
	bool fixedBadZeros(std::string& strInput, std::vector<long>& rvecPos, 
		bool bAngle);
	void locateSymbols(const std::string& strInput, 
		std::vector<long>& rvecPos);
	void stampSymbols(std::string& strInput, long lDegreePos, long lMinutePos, 
		long lSecondPos);

	// PROMISE: return true if ucChar is a symbol character
	// where symbols are in the set: {° « » ' " ^ * ~ -}
	inline bool isSymbolChar(unsigned char ucChar)
	{
		return (ucChar == 176) ||	// ucChar == '°'
			(ucChar == 171) ||	// ucChar == '«'
			(ucChar == 187) ||	// ucChar == '»'
			(ucChar == '\'') || (ucChar == '"') || (ucChar == '^') || 
			(ucChar == '*') || (ucChar == '~') || (ucChar == '-');
	}

	// PURPOSE: To validate proper licensing of this component
	// PROMISE: To throw an exception if this component is not licensed to run
	void validateLicense();

private:
	bool isKnowledgeableOfInputType(const std::string& strInputType);
};

