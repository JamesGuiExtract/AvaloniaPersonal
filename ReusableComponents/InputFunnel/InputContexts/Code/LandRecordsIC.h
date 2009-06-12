// LandRecordsIC.h : Declaration of the CLandRecordsIC

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CLandRecordsIC
class ATL_NO_VTABLE CLandRecordsIC : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLandRecordsIC, &CLSID_LandRecordsIC>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IInputContext, &IID_IInputContext, &LIBID_UCLID_INPUTFUNNELLib>
{
public:
	CLandRecordsIC();
	~CLandRecordsIC();

DECLARE_REGISTRY_RESOURCEID(IDR_LANDRECORDSIC)

//DECLARE_CLASSFACTORY_SINGLETON(CLandRecordsIC)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLandRecordsIC)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IInputContext)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

//	ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IInputContext
	STDMETHOD(raw_Activate)(IInputManager* pInputManager);
	STDMETHOD(raw_NotifyNewIRConnected)(IInputReceiver *pNewInputReceiver);

private:
	///////////////////
	// Member variables
	///////////////////

	// Supporting objects for use with the SpotRecognitionWindow
	IParagraphTextCorrectorPtr m_ipParagraphTextCorrector;
	IIUnknownVectorPtr m_ipParagraphTextHandlers;
	ISubImageHandlerPtr m_ipSubImageHandler;
	ILineTextCorrectorPtr m_ipMCRLineTextCorrector;
	ILineTextEvaluatorPtr m_ipLineTextEvaluator;


	///////////////////
	// Member functions
	////////////////////
	// if input receiver that's connected is of SpotRecIR, then set
	// appropriate variables to the newly connected SRIR
	// Return true if connection is succeeded
	bool connectSRIR(IInputReceiver *pNewInputReceiver);
	// initialized variables
	void init();
	// validate license
	void validateLicense();
};
