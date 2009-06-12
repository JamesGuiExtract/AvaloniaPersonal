// ArcGISAttributeManager.cpp : Implementation of CArcGISAttributeManager
#include "stdafx.h"
#include "ArcGISUtils.h"
#include "ArcGISAttributeManager.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <RegistryPersistenceMgr.h>
#include <cpputil.h>
#include <RegConstants.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <IcoMapOptions.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string SHAPEFILE = "Shapefiles";

const string CArcGISAttributeManager::ROOT_FOLDER = gstrREG_ROOT_KEY + "\\ArcGISUtils\\ArcGISAttributeManager";
const string CArcGISAttributeManager::GENERAL_FOLDER = "\\General";
const string CArcGISAttributeManager::DATA_INTERPRETER_KEY = "DataInterpreter";

//-------------------------------------------------------------------------------------------------
// CArcGISAttributeManager
//-------------------------------------------------------------------------------------------------
CArcGISAttributeManager::CArcGISAttributeManager()
: m_ipDataInterpreter(NULL),
  m_ipArcMapApp(NULL),
  m_ipArcMapEditor(NULL),
  m_ipSourceDocCollection(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CArcGISAttributeManager::~CArcGISAttributeManager()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISAttributeManager::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeManager,
		&UCLID_COMLMLib::IID_ILicensedComponent,
		&IID_IArcGISDependentComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISAttributeManager::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeManager
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISAttributeManager::raw_SetSketchSourceDocuments(BSTR strFeatureID, IVariantVector *pColSrcDocStrings)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		// currently only support "Create New Feature"
		if (getCurrentTaskName() == "Create New Feature")
		{
			m_ipSourceDocCollection = pColSrcDocStrings;

			/*esriEditor::*/IEditLayersPtr ipEditLayers(m_ipArcMapEditor);
			ASSERT_RESOURCE_ALLOCATION("ELI11727", ipEditLayers != NULL);
			// get current layer that's beening edited
			/*esriCarto::*/IFeatureLayerPtr ipFeatureLayer = ipEditLayers->CurrentLayer;
			ASSERT_RESOURCE_ALLOCATION("ELI11728", ipFeatureLayer != NULL);

			/*esriCarto::*/IHyperlinkContainerPtr ipHyperlinkContainer(ipFeatureLayer);
			ASSERT_RESOURCE_ALLOCATION("ELI11729", ipHyperlinkContainer != NULL);

			// convert the feature id from string to long
			string stdstrFeatureID = _bstr_t(strFeatureID);
			long nFeatureIDNum = ::asLong(stdstrFeatureID);

			// store all source documents into the hyperlink container
			long nNumOfDocs = m_ipSourceDocCollection->Size;
			for (long n=0; n<nNumOfDocs; n++)
			{
				// create a new hyperlink
				/*esriCarto::*/IHyperlinkPtr ipHyperlink(/*esriCarto::*/CLSID_Hyperlink);
				ASSERT_RESOURCE_ALLOCATION("ELI11730", ipHyperlink != NULL);
				ipHyperlink->LinkType = /*esriCarto::*/esriHyperlinkTypeDocument;
				ipHyperlink->Link = _bstr_t(m_ipSourceDocCollection->GetItem(n));
				ipHyperlink->FeatureId = nFeatureIDNum;
				// add to the container
				ipHyperlinkContainer->AddHyperlink(ipHyperlink);
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11445")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISAttributeManager::raw_GetSketchSourceDocuments(BSTR strFeatureID, IVariantVector **ppColSrcDocStrings)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		*ppColSrcDocStrings = NULL;

		if (m_ipSourceDocCollection != NULL)
		{
			m_ipSourceDocCollection.AddRef();
			*ppColSrcDocStrings = m_ipSourceDocCollection;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11446")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISAttributeManager::raw_SetSegmentSourceDocuments(BSTR strSegmentID, IVariantVector *pColSrcDocStrings)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// not implemented
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISAttributeManager::raw_GetSegmentSourceDocuments(BSTR strSegmentID, 
																	IVariantVector **ppColSrcDocStrings)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// not implemented
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISAttributeManager::raw_SetFeatureAttribute(BSTR strFeatureID, 
															  BSTR strStoreFieldName,
															  UCLID_FEATUREMGMTLib::IUCLDFeature *ipFeature)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		// currently only support "Create New Feature"
		if (getCurrentTaskName() == "Create New Feature")
		{
			// if there's one and only one feature selected
			/*esriGeoDatabase::*/IFeaturePtr ipSelectedFeature = getSelectedFeature();
			if (ipSelectedFeature != NULL)
			{
				// string to store attributes
				string strFeatureAttributes("");
				if (ipFeature != NULL)
				{
					strFeatureAttributes = getAttributeDataInterpreter()->getAttributeDataFromFeature(ipFeature);
				}
				
				/*esriGeoDatabase::*/IDatasetPtr ipDataset = ipSelectedFeature->Class;
				ASSERT_RESOURCE_ALLOCATION("ELI11447", ipDataset != NULL);
				/*esriGeoDatabase::*/IWorkspacePtr ipWorkspace = ipDataset->Workspace;
				ASSERT_RESOURCE_ALLOCATION("ELI11448", ipWorkspace != NULL);
				string strDescription = getWorkspaceDescription(ipWorkspace);
				if (strDescription == SHAPEFILE)
				{
					return S_OK;
				}

				/*esriGeoDatabase::*/IWorkspaceEditPtr ipWorkspaceEdit(ipWorkspace);
				// feature field can only be edited in edit mode
				if (ipWorkspaceEdit->IsBeingEdited() == VARIANT_TRUE)
				{
					/*esriGeoDatabase::*/IFeatureClassPtr ipFeatureClass(ipSelectedFeature->Class);
					ASSERT_RESOURCE_ALLOCATION("ELI11717", ipFeatureClass != NULL);
					long nIndex = findStoreAttributeField(ipFeatureClass, strStoreFieldName);
					// if there is a field dedicated to store feature atrributes string
					if (nIndex >= 0)
					{
						unsigned long nFieldLen = getStoreAttributeFieldLength(ipFeatureClass, strStoreFieldName);
						// attribute string length must be no greater than field length
						if (nFieldLen < strFeatureAttributes.size())
						{
							AfxMessageBox("This feature attribute has exceeded the maximum length defined for this field."
										  "Original attribute will not be stored for this feature.");
							return S_OK;
						}

						m_ipArcMapEditor->StartOperation();
						ipSelectedFeature->PutValue(nIndex, CComVariant(strFeatureAttributes.c_str()));
						// commit the transaction
						ipSelectedFeature->Store();
						m_ipArcMapEditor->StopOperation(_bstr_t("Stored feature string in database."));
					}
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11449")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISAttributeManager::raw_GetFeatureAttribute(BSTR strFeatureID, 
															  BSTR strStoreFieldName,
															  UCLID_FEATUREMGMTLib::IUCLDFeature **ipFeature)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		*ipFeature = NULL;
		
		/*esriGeoDatabase::*/IFeaturePtr ipSelectedFeature = getSelectedFeature();
		// if there's one and only one feature selected
		if (ipSelectedFeature != NULL)
		{
			long nFieldIndex = findStoreAttributeField(ipSelectedFeature->Class, strStoreFieldName);
			if (nFieldIndex >= 0)
			{
				_variant_t _varFieldValue = ipSelectedFeature->GetValue(nFieldIndex);
				// only if the value is of type string
				if (_varFieldValue.vt == VT_BSTR)
				{
					// get stored string value from the specified field
					string strFieldValue = _bstr_t(_varFieldValue);
					if (!strFieldValue.empty())
					{
						// use a data enterpreter to translate the string into
						// a UCLID feature
						UCLID_FEATUREMGMTLib::IUCLDFeaturePtr ipUCLIDFeature = 
							getAttributeDataInterpreter()->getFeatureFromAttributeData(_bstr_t(strFieldValue.c_str()));
						
						*ipFeature = ipUCLIDFeature.Detach();
					}
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11450")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISAttributeManager::raw_CanStoreAttributes(BSTR strFeatureID,
															 BSTR strStoreFieldName, 
															 VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		*pbValue = VARIANT_FALSE;
		
		/*esriGeoDatabase::*/IFeaturePtr ipSelectedFeature = getSelectedFeature();
		// if there's one and only one feature selected
		if (ipSelectedFeature != NULL)
		{
			// if selected feature class is shape file, it can't store attributes
			/*esriGeoDatabase::*/IDatasetPtr ipDataset = ipSelectedFeature->Class;
			ASSERT_RESOURCE_ALLOCATION("ELI11731", ipDataset != NULL);
			/*esriGeoDatabase::*/IWorkspacePtr ipWorkspace = ipDataset->Workspace;
			ASSERT_RESOURCE_ALLOCATION("ELI11732", ipWorkspace != NULL);
			string strDescription = getWorkspaceDescription(ipWorkspace);
			if (strDescription == SHAPEFILE)
			{
				return S_OK;
			}
			
			if (findStoreAttributeField(ipSelectedFeature->Class, strStoreFieldName) >= 0)
			{
				*pbValue = VARIANT_TRUE;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11451")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IArcGISDependentComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISAttributeManager::SetApplicationHook(IDispatch *pApp)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Do not call validate license here - it will cause an exception to 
		// be thrown before ArcMap window comes up
		//validateLicense();

		m_ipArcMapApp = pApp;
		ASSERT_RESOURCE_ALLOCATION("ELI11712", m_ipArcMapApp != NULL);
		IUIDPtr ipID(CLSID_UID);
		ASSERT_RESOURCE_ALLOCATION("ELI11711", ipID != NULL);
		ipID->Value = _variant_t(_bstr_t("esriEditor.editor"));

		m_ipArcMapEditor = m_ipArcMapApp->FindExtensionByCLSID(ipID);
		ASSERT_RESOURCE_ALLOCATION("ELI11713", m_ipArcMapEditor != NULL);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11463")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
long CArcGISAttributeManager::findStoreAttributeField(/*esriGeoDatabase::*/IFeatureClassPtr ipFeatureClass,
													  BSTR strFieldName)
{
	long nIndex = -1;

	// only support line and polygon feature at this point
	/*esriGeometry::*/esriGeometryType eGeoType = ipFeatureClass->ShapeType;
	if (eGeoType == /*esriGeometry::*/esriGeometryPolyline || eGeoType == /*esriGeometry::*/esriGeometryPolygon)
	{
		nIndex = ipFeatureClass->FindField(strFieldName);
	}

	return nIndex;
}
//-------------------------------------------------------------------------------------------------
IFeatureAttributeDataInterpreterPtr CArcGISAttributeManager::getAttributeDataInterpreter()
{
	if (m_ipDataInterpreter == NULL)
	{
		if (m_apCfgMgr.get() == NULL)
		{
			m_apCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
				new RegistryPersistenceMgr(HKEY_LOCAL_MACHINE, ROOT_FOLDER));
			ASSERT_RESOURCE_ALLOCATION("ELI11719", m_apCfgMgr.get() != NULL);
		}

		// look in Registry for any specified data interpreter prog id
		if (m_apCfgMgr->keyExists(GENERAL_FOLDER, DATA_INTERPRETER_KEY))
		{
			// get the data interpreter description
			string strDataInterpreterDesc = 
				m_apCfgMgr->getKeyValue(GENERAL_FOLDER, DATA_INTERPRETER_KEY);
			if (!strDataInterpreterDesc.empty())
			{
				ICategoryManagerPtr ipCatMgr(CLSID_CategoryManager);
				ASSERT_RESOURCE_ALLOCATION("ELI11720", ipCatMgr != NULL);
				IStrToStrMapPtr ipDescToProgIDMap = 
					ipCatMgr->GetDescriptionToProgIDMap1(_bstr_t("UCLID IFeatureAttributeDataInterpreter"));

				if (!ipDescToProgIDMap->Contains(_bstr_t(strDataInterpreterDesc.c_str())))
				{
					UCLIDException ue("ELI11721", "Unable to find specified FeatureAttributeDataInterpreter!");
					ue.addDebugInfo("Description", strDataInterpreterDesc);
					throw ue;
				}

				// get prog id
				string strProgID = 
					ipDescToProgIDMap->GetValue(_bstr_t(strDataInterpreterDesc.c_str()));

				CLSID clsid;
				CLSIDFromProgID(CComBSTR(strProgID.c_str()), &clsid);
				// instantiate the data interpreter
				m_ipDataInterpreter.CreateInstance(clsid);
				if (m_ipDataInterpreter == NULL)
				{
					UCLIDException ue("ELI11722", "Unable to instantiate specified FeatureAttributeDataInterpreter!");
					ue.addDebugInfo("Description", strDataInterpreterDesc);
					ue.addDebugInfo("ProgID", strProgID);
					throw ue;
				}
			}
		}
		else
		{
			// if there's no data interpreter description specified in Registry
			// then create and instance of CommaDelimitedFeatureAttributeDataInterpreter
			m_ipDataInterpreter.CreateInstance(__uuidof(CommaDelimitedFeatureAttributeDataInterpreter));
			ASSERT_RESOURCE_ALLOCATION("ELI11723", m_ipDataInterpreter != NULL);
		}
	}

	return m_ipDataInterpreter;
}
//-------------------------------------------------------------------------------------------------
string CArcGISAttributeManager::getCurrentTaskName()
{
	string strTaskName("");

	/*esriEditor::*/IEditTaskPtr ipEditTask = m_ipArcMapEditor->CurrentTask;
	if (ipEditTask != NULL)
	{
		strTaskName = _bstr_t(ipEditTask->Name);
	}

	return strTaskName;
}
//-------------------------------------------------------------------------------------------------
/*esriGeoDatabase::*/IFeaturePtr CArcGISAttributeManager::getSelectedFeature()
{
	/*esriGeoDatabase::*/IFeaturePtr ipSelectedFeature(NULL);
	
	IMxDocumentPtr ipMxDoc = m_ipArcMapApp->Document;
	ASSERT_RESOURCE_ALLOCATION("ELI11724", ipMxDoc != NULL);
	
	// if there's one and only one feature selected
	if (ipMxDoc->FocusMap->SelectionCount == 1)
	{
		/*esriGeoDatabase::*/IEnumFeaturePtr ipEnumFeature = ipMxDoc->FocusMap->FeatureSelection;
		ASSERT_RESOURCE_ALLOCATION("ELI11725", ipEnumFeature != NULL);
		ipEnumFeature->Reset();
		ipSelectedFeature = ipEnumFeature->Next();
		ASSERT_RESOURCE_ALLOCATION("ELI11726", ipSelectedFeature != NULL);
	}

	return ipSelectedFeature;
}
//-------------------------------------------------------------------------------------------------
long CArcGISAttributeManager::getStoreAttributeFieldLength(/*esriGeoDatabase::*/IFeatureClassPtr ipFeatureClass, 
															BSTR strFieldName)
{
	long nLen = 0;
	// find the specified field first
	long nIndex = findStoreAttributeField(ipFeatureClass, strFieldName);
	if (nIndex >= 0)
	{
		// get all fields from this feature class table
		/*esriGeoDatabase::*/IFieldsPtr ipFields = ipFeatureClass->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI11715", ipFields != NULL);
		// get specified field
		/*esriGeoDatabase::*/IFieldPtr ipField = ipFields->GetField(nIndex);
		ASSERT_RESOURCE_ALLOCATION("ELI11716", ipField != NULL);

		nLen = ipField->Length;
	}

	return nLen;
}
//-------------------------------------------------------------------------------------------------
string CArcGISAttributeManager::getWorkspaceDescription(/*esriGeoDatabase::*/IWorkspacePtr ipWorkspace)
{
	/*esriGeoDatabase::*/IWorkspaceFactoryPtr ipWorkspaceFactory = ipWorkspace->WorkspaceFactory;
	ASSERT_RESOURCE_ALLOCATION("ELI11733", ipWorkspaceFactory != NULL);
	string strDescription = asString(ipWorkspaceFactory->GetWorkspaceDescription(VARIANT_FALSE));


	return strDescription;
}
//-------------------------------------------------------------------------------------------------
void CArcGISAttributeManager::validateLicense()
{
	// Call validateIcoMapLicensed() in IcoMapOptions in order to check 
	// either license file or USB key license
	IcoMapOptions::sGetInstance().validateIcoMapLicensed();
}
//-------------------------------------------------------------------------------------------------
void CArcGISAttributeManager::validateObjects()
{
	// m_ipArcMapApp and m_ipArcMapEditor must be set
	if (m_ipArcMapApp == NULL || m_ipArcMapEditor == NULL)
	{
		throw UCLIDException("ELI11714", "Internal Error: you must call SetApplicationHook() first.");
	}
}
//-------------------------------------------------------------------------------------------------
