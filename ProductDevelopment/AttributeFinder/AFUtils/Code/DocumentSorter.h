// DocumentSorter.h : Declaration of the CDocumentSorter

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CDocumentSorter
class ATL_NO_VTABLE CDocumentSorter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDocumentSorter, &CLSID_DocumentSorter>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDocumentSorter, &IID_IDocumentSorter, &LIBID_UCLID_AFUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CDocumentSorter();
	~CDocumentSorter();

DECLARE_REGISTRY_RESOURCEID(IDR_DOCUMENTSORTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDocumentSorter)
	COM_INTERFACE_ENTRY(IDocumentSorter)
	COM_INTERFACE_ENTRY2(IDispatch,IDocumentSorter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDocumentSorter
	STDMETHOD(SortDocuments)(/*[in]*/ BSTR strInputFolder, 
		/*[in]*/ BSTR strOutputFolder, 
		/*[in]*/ BSTR strDocIndustryName,
		/*[in]*/ VARIANT_BOOL bOCRImages);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	//////////
	// Methods
	//////////
	// Batch ocr all image files contained in the strInput
	void batchOCR(BSTR strInputFolder);

	// moves the specified .txt file along with any of its related
	// files (i.e. with the same prefix, mostly the image file) to
	// the specified folder
	void moveFile(const std::string& strUSSFileName, 
				  const std::string& strMoveToFolder);

	// classify the file, and then copy it along with its original image
	// file to a specific folder (i.e. either this file can't be classified
	// as one of the existing document types, then they will be moved to the 
	// "Unclassified" folder; or it is one of the documents, they will be moved
	// to the folder with the document type as the folder name.)
	void sortFile(const std::string& strUSSFileName,
				  const std::string& strCategoryName,
				  const std::string& strOutputRootFolder);

	void validateLicense();

	///////////
	// Variables
	///////////
	IOCRUtilsPtr m_ipOCRUtils;
	IOCREnginePtr m_ipOCREngine;
	UCLID_AFUTILSLib::IDocumentClassifierPtr m_ipDocClassifier;
};
