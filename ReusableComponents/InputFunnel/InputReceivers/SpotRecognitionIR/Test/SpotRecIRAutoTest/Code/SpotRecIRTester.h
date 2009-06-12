// SpotRecIRTester.h : Declaration of the CSpotRecIRTester

#ifndef __SPOTRECIRTESTER_H_
#define __SPOTRECIRTESTER_H_

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CSpotRecIRTester
class ATL_NO_VTABLE CSpotRecIRTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpotRecIRTester, &CLSID_SpotRecIRTester>,
	public IDispatchImpl<ISpotRecIRTester, &IID_ISpotRecIRTester, &LIBID_SPOTRECIRAUTOTESTLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
private:
	ITestResultLoggerPtr m_ipLogger;
	IInteractiveTestExecuterPtr m_ipITCExecuter;
	ISpotRecognitionWindowPtr m_ipSpotRecIR;

public:
	CSpotRecIRTester();

DECLARE_REGISTRY_RESOURCEID(IDR_SPOTRECIRTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSpotRecIRTester)
	COM_INTERFACE_ENTRY(ISpotRecIRTester)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY2(IDispatch, ISpotRecIRTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

// ISpotRecIRTester
public:
// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	// some of the methods use test files which resides in a specific folders.
	std::string m_strAutomatedTestFilesFolder;
	std::string m_strInteractiveTestFilesFolder;
	
	// the following method tests EnableInput, DisableInput, and get_InputIsEnabled
	void runEnableInputTest();

	// the following method tests get_HasWindow()
	void runHasWindowTest();

	// the following method tests get_WindowShown(), and ShowWindow()
	void runShowWindowTest();

	// the following method is the first test that is run, and is intended to ensure
	// that the spot recognition IR came up to the correct initial state.
	void runProperInitializationTest();

	// test to ensure that the component description returned by the spot rec ir
	// is not empty
	void runComponentDescriptionTest();

	// the following method tests OpenImageFile(), GetImageFileName(), and Clear()
	void runOpenImageTest();

	// the following method tests get_WindowHandle()
	void runWindowHandleTest();

	// the following method tests GetTotalPages(), GetCurrentPageNumber(), and 
	// SetCurrentPageNumber()
	void runPageNumberTest();

	// the following method tests OpenGDDFile, Save(), SaveAs(), and Clear()
	void runOpenGDDFileTest();

	// determines and sets the value of AutomatedTestFilesFolder
	void setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile);

};

#endif //__SPOTRECIRTESTER_H_
