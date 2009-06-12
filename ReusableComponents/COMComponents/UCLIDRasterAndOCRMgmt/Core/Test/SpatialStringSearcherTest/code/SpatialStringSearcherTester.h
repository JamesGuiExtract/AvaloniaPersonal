// SpatialStringSearcherTester.h : Declaration of the CSpatialStringSearcherTester

#ifndef __SPATIALSTRINGSEARCHERTESTER_H_
#define __SPATIALSTRINGSEARCHERTESTER_H_

#include "resource.h"       // main symbols


#include <string>
#include <vector>
/////////////////////////////////////////////////////////////////////////////
// CSpatialStringSearcherTester
class ATL_NO_VTABLE CSpatialStringSearcherTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpatialStringSearcherTester, &CLSID_SpatialStringSearcherTester>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CSpatialStringSearcherTester()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALSTRINGSEARCHERTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSpatialStringSearcherTester)

	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
END_COM_MAP()

// ISpatialStringSearcherTester
public:
// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);
private:

	ITestResultLoggerPtr m_ipResultLogger;
	bool m_bIncludeDataOnBoundary;
	ESpatialEntity m_eBoundaryDataResolution;
	bool m_bGetDataInRegion;
	long m_lLeft, m_lRight, m_lTop, m_lBottom;
	long m_lCurrTestCase;
	
	const std::string getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const ;
	void processFile(const std::string& str);
	void processTestCase(std::vector<std::string>& vecTokens, const std::string& strDatFile);
	void processSet(std::vector<std::string>& vecTokens, const std::string& strDatFile);

	void processLine(std::string strLine, const std::string& strFileWithLine, std::ifstream &ifs);
	void setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile);

	std::string getSettingsAsString();
};

#endif //__SPATIALSTRINGSEARCHERTESTER_H_
