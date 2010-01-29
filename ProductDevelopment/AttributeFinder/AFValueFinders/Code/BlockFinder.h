// BlockFinder.h : Declaration of the CBlockFinder

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CBlockFinder
class ATL_NO_VTABLE CBlockFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CBlockFinder, &CLSID_BlockFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<IBlockFinder, &IID_IBlockFinder, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CBlockFinder>
{
public:
	CBlockFinder();
	~CBlockFinder();

DECLARE_REGISTRY_RESOURCEID(IDR_BLOCKFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CBlockFinder)
	COM_INTERFACE_ENTRY(IBlockFinder)
	COM_INTERFACE_ENTRY2(IDispatch,IBlockFinder)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CBlockFinder)
	PROP_PAGE(CLSID_BlockFinderPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CBlockFinder)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IBlockFinder
	STDMETHOD(get_IsCluePartOfAWord)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCluePartOfAWord)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_Clues)(/*[out, retval]*/ IVariantVector **pVal);
	STDMETHOD(put_Clues)(/*[in]*/ IVariantVector *newVal);
	STDMETHOD(GetBlockScore)(/*[in]*/ BSTR strBlockText, /*[out, retval]*/ long *pScore);
	STDMETHOD(get_GetMaxOnly)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_GetMaxOnly)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_MinNumberOfClues)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_MinNumberOfClues)(/*[in]*/ long newVal);
	STDMETHOD(get_IsClueRegularExpression)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsClueRegularExpression)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_FindAllBlocks)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_FindAllBlocks)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_InputAsOneBlock)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_InputAsOneBlock)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_BlockSeperator)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_BlockSeperator)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_DefineBlocksType)(/*[out, retval]*/ EDefineBlocksType *pVal);
	STDMETHOD(put_DefineBlocksType)(/*[in]*/ EDefineBlocksType newVal);
	STDMETHOD(get_BlockBegin)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_BlockBegin)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_BlockEnd)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_BlockEnd)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_PairBeginAndEnd)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_PairBeginAndEnd)(/*[in]*/ VARIANT_BOOL newVal);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute* pAttribute, IAFDocument* pOriginInput, 
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	///////////
	// Methods
	///////////
	void validateLicense();

	IIUnknownVectorPtr getBlocksBeginEnd(ISpatialStringPtr ipSS);
	IIUnknownVectorPtr getBlocksSeparator(ISpatialStringPtr ipSS);

	IIUnknownVectorPtr chooseBlocks(IIUnknownVectorPtr ipItems);
	IIUnknownVectorPtr getBlocks(ISpatialStringPtr ipSS);

	////////////
	// Variables
	////////////
	// flag to keep track of whether this object has been modified
	// since the last save-to-stream operation
	bool m_bDirty;

	std::string m_strBlockSeperator;
	// If no seperator is found in the input text, whether or not
	// the entire input text shall be treated as a block
	bool m_bInputAsOneBlock;
	// find all blocks or find those blocks that meet certain criteria
	bool m_bFindAllBlocks;
	// whether or not to treat the clues as regular expression
	bool m_bIsClueRegularExpression;
	// minimum number of clues to be found in the text
	long m_nMinNumberOfClues;
	// whether or not to get only those blocks with maximum number of clues
	bool m_bGetMaxOnly;
	// whether or not each clue text is part of a word
	bool m_bIsCluePartOfAWord;

	EDefineBlocksType m_eDefineBlocks;
	std::string m_strBlockBegin;
	std::string m_strBlockEnd;
	bool m_bPairBeginEndStrings;

	// list of clues defined
	IVariantVectorPtr m_ipClues;

	IMiscUtilsPtr m_ipMiscUtils;
};
