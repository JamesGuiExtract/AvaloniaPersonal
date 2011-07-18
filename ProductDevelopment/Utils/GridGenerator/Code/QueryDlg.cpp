// QueryDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "QueryDlg.h"

#include <INIFilePersistenceMgr.h>
#include "Constants.h"

#include <UCLIDException.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

HHOOK ghHook = NULL;			// Windows message hook

//--------------------------------------------------------------------------------------------------
// Hook procedure for WH_GETMESSAGE hook type.
//--------------------------------------------------------------------------------------------------
LRESULT CALLBACK DlgMsgProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	//	See KB Atricle: PRB: ActiveX Control Is the Parent Window of Modeless Dialog
	

	// Switch the module state for the correct handle to be used.
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// Check whether or not to process the message.
	if (nCode >= 0 && PM_REMOVE == wParam)
	{
		// Translate specific messages in controls' PreTranslateMessage().
		LPMSG lpMsg = (LPMSG) lParam;
		UINT nMsg = lpMsg->message;
		if (nMsg >= WM_KEYFIRST && nMsg <= WM_KEYLAST)
		{
			if (AfxGetApp()->PreTranslateMessage(lpMsg))
			{
				// The value returned from this hookproc is ignored, and it cannot
				// be used to tell Windows the message has been handled. To avoid
				// further processing, convert the message to WM_NULL before
				// returning.
				lpMsg->message = WM_NULL;
				lpMsg->lParam = 0L;
				lpMsg->wParam = 0;
			}
		}
	}
	
	// Pass the hook information to the next hook procedure in the current hook chain.
	return ::CallNextHookEx(ghHook, nCode, wParam, lParam);
}

//-------------------------------------------------------------------------------------------------
// QueryDlg dialog
//-------------------------------------------------------------------------------------------------
QueryDlg::QueryDlg(IApplicationPtr ipApp, CWnd* pParent /*=NULL*/)
: m_ipApp(ipApp),
  CDialog(QueryDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(QueryDlg)
	m_nRangeDir = 0;
	m_nTownshipDir = 0;
	m_zCountyCode = _T("");
	m_zQQ = _T("");
	m_zQQQQ = _T("");
	m_zQuarter = _T("");
	m_zRange = _T("");
	m_zSectionNum = _T("");
	m_zTownship = _T("");
	m_nLayer = -1;
	m_zQQQ = _T("");
	//}}AFX_DATA_INIT

	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI08320", m_ipApp != NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08319")
}
//-------------------------------------------------------------------------------------------------
QueryDlg::~QueryDlg()
{
	if (ghHook)
	{
		VERIFY(::UnhookWindowsHookEx(ghHook));
		ghHook = NULL;
	}
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(QueryDlg)
	DDX_Control(pDX, IDC_EDIT_QQQ, m_editQQQ);
	DDX_Control(pDX, IDC_EDIT_QQQQ, m_editQQQQ);
	DDX_Control(pDX, IDC_EDIT_QQ, m_editQQ);
	DDX_Control(pDX, IDC_EDIT_QUARTER, m_editQuarter);
	DDX_Control(pDX, IDC_EDIT_SECTION_NUM, m_editSectionNum);
	DDX_Control(pDX, IDC_CMB_LAYER, m_cmbLayer);
	DDX_CBIndex(pDX, IDC_CMB_RANGE_DIR2, m_nRangeDir);
	DDX_CBIndex(pDX, IDC_CMB_TOWNSHIP_DIR2, m_nTownshipDir);
	DDX_Text(pDX, IDC_EDIT_COUNTY_CODE2, m_zCountyCode);
	DDV_MaxChars(pDX, m_zCountyCode, 3);
	DDX_Text(pDX, IDC_EDIT_QQ, m_zQQ);
	DDV_MaxChars(pDX, m_zQQ, 2);
	DDX_Text(pDX, IDC_EDIT_QQQQ, m_zQQQQ);
	DDV_MaxChars(pDX, m_zQQQQ, 4);
	DDX_Text(pDX, IDC_EDIT_QUARTER, m_zQuarter);
	DDV_MaxChars(pDX, m_zQuarter, 1);
	DDX_Text(pDX, IDC_EDIT_RANGE2, m_zRange);
	DDV_MaxChars(pDX, m_zRange, 3);
	DDX_Text(pDX, IDC_EDIT_SECTION_NUM, m_zSectionNum);
	DDV_MaxChars(pDX, m_zSectionNum, 3);
	DDX_Text(pDX, IDC_EDIT_TOWNSHIP2, m_zTownship);
	DDV_MaxChars(pDX, m_zTownship, 3);
	DDX_CBIndex(pDX, IDC_CMB_LAYER, m_nLayer);
	DDX_Text(pDX, IDC_EDIT_QQQ, m_zQQQ);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(QueryDlg, CDialog)
	//{{AFX_MSG_MAP(QueryDlg)
	ON_CBN_SELCHANGE(IDC_CMB_LAYER, OnSelchangeCmbLayer)
	ON_BN_CLICKED(IDC_BTN_QUERY, OnBtnQuery)
	ON_WM_CLOSE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// QueryDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL QueryDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	try
	{
		ghHook = ::SetWindowsHookEx(WH_GETMESSAGE, DlgMsgProc,
					AfxGetInstanceHandle(), ::GetCurrentThreadId());
		ASSERT_RESOURCE_ALLOCATION("ELI08390", ghHook != NULL);

		m_cmbLayer.AddString(TOWNSHIP_LAYER.c_str());
		//addLayerFromINItoLB( TOWNSHIP_LAYER );
		addLayerFromINItoLB( SECTION_LAYER );
		addLayerFromINItoLB( QUARTER_LAYER );
		addLayerFromINItoLB( QQ_LAYER );
		addLayerFromINItoLB( QQQ_LAYER );
		addLayerFromINItoLB( QQQQ_LAYER );

		// set selection to first item
		m_nLayer = 0;

		updateEditControls();

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08293");
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::OnSelchangeCmbLayer() 
{
	try
	{
		UpdateData();
		updateEditControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08302");
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::OnBtnQuery() 
{
	try
	{
		UpdateData();

		CWaitCursor wait;

		selectFeature();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08306");
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::OnCancel()
{
	// purpose of having this function here is to prevent
	// user from closing the dialog by pressing Escape key
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::OnOK()
{
	// when Enter key is pressed, call Query
	OnBtnQuery();
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::OnClose() 
{	
	CDialog::OnClose();

	CDialog::OnCancel();
}


//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
ILayerPtr QueryDlg::getLayer(const string& strLayerName)
{
	IMxDocumentPtr ipMxDoc(m_ipApp->Document);
	IMapPtr ipMap(ipMxDoc->FocusMap);
	long nNumOfLayers = ipMap->LayerCount;
	for (long n=0; n<nNumOfLayers; n++)
	{
		ILayerPtr ipLayer = ipMap->GetLayer(n);
		string strLayer = _bstr_t(ipLayer->Name);

		// if this is the layer we're looking for
		if (strLayer == strLayerName)
		{
			return ipLayer;
		}
	}

	return NULL;
}
//-------------------------------------------------------------------------------------------------
string QueryDlg::getWhereClause()
{
	string strWhereClause("");
	// county code
	addANDClause( strWhereClause, COUNTY_CODE, m_zCountyCode.operator LPCTSTR() );

	// township
	addANDClause( strWhereClause, TOWNSHIP, m_zTownship.operator LPCTSTR());
	string strTownshipDir = m_nTownshipDir == 0 ? "N" : "S";
	addANDClause( strWhereClause, TOWNSHIP_DIR, strTownshipDir, esriFieldTypeString );
	
	// range
	addANDClause( strWhereClause, RANGE, m_zRange.operator LPCTSTR());
	string strRangeDir = m_nRangeDir == 0 ? "E" : "W";
	addANDClause( strWhereClause, RANGE_DIR, strRangeDir, esriFieldTypeString );

	// section
	addANDClause( strWhereClause, SECTION_NUM, m_zSectionNum.operator LPCTSTR());

	string strCurrLevelStr = "";

	switch (m_nLayer)
	{
	case 5:	// QQQQ
		addANDClause( strWhereClause, QQQQ, m_zQQQQ.operator LPCTSTR() );
		break;

	case 4:	// QQQ
		strCurrLevelStr = m_zQQQ.operator LPCTSTR();
		addANDClause( strWhereClause, QUARTER_QUARTER_QUARTER, strCurrLevelStr );
		strCurrLevelStr = strCurrLevelStr.substr(1,2);

	case 3:	// QQ
		if ( m_nLayer == 3 )
		{
			strCurrLevelStr = m_zQQ.operator LPCTSTR();
		}
		addANDClause( strWhereClause, QUARTER_QUARTER, strCurrLevelStr );
		strCurrLevelStr = strCurrLevelStr.substr(1,1);

	case 2:	// Quarter
		if ( m_nLayer == 2 )
		{
			strCurrLevelStr = m_zQuarter.operator LPCTSTR();
		}
		addANDClause( strWhereClause, QUARTER, strCurrLevelStr );
	}

	return strWhereClause;
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::selectFeature()
{
	// clear all selections
	IMxDocumentPtr ipMxDoc(m_ipApp->Document);
	IMapPtr ipMap(ipMxDoc->FocusMap);
	ipMap->ClearSelection();
	IActiveViewPtr ipActiveView(ipMap);
	ASSERT_RESOURCE_ALLOCATION("ELI08330", ipActiveView != NULL);
	ipActiveView->Refresh();

	// get current selected layer name
	CString zSelectedLayerName("");
	m_cmbLayer.GetLBText(m_nLayer, zSelectedLayerName);
	
	ILayerPtr ipSelectedLayer = getLayer((LPCTSTR)zSelectedLayerName);
	// if there's no match layer name, no selection
	if (ipSelectedLayer == NULL)
	{
		UCLIDException ue("ELI08329", "Unable to find the specified layer.");
		ue.addDebugInfo("Layer", (LPCTSTR)zSelectedLayerName);
		throw ue;
	}

	IFeatureSelectionPtr ipFeatureSel(ipSelectedLayer);
	ASSERT_RESOURCE_ALLOCATION("ELI08325", ipFeatureSel != NULL);

	IQueryFilterPtr ipQFilter(CLSID_QueryFilter);
	ASSERT_RESOURCE_ALLOCATION("ELI08324", ipQFilter != NULL);

	string strWhereClause = getWhereClause();
	if (strWhereClause.empty())
	{
		// nothing to be selected
		return;
	}

	ipQFilter->WhereClause = _bstr_t(strWhereClause.c_str());

	ipFeatureSel->SelectFeatures(ipQFilter, esriSelectionResultNew, VARIANT_FALSE);
	ipActiveView->PartialRefresh(esriViewGeoSelection, NULL, NULL);

	zoomToSelectedFeature(ipActiveView);
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::updateEditControls()
{
	m_editQuarter.EnableWindow(FALSE);
	m_editQQ.EnableWindow(FALSE);
	m_editQQQ.EnableWindow(FALSE);
	m_editQQQQ.EnableWindow(FALSE);

	m_editSectionNum.EnableWindow(TRUE);

	switch (m_nLayer)
	{
	case 5:	// if QQQQ layer is selected
		m_editQQQQ.EnableWindow(TRUE);
		break;

	case 4:
		m_editQQQ.EnableWindow(TRUE);
		break;

	case 3:	// if QQ layer is selected
		m_editQQ.EnableWindow(TRUE);
		break;

	case 2:	// if Quarter layer is selected
		m_editQuarter.EnableWindow(TRUE);
		break;

	case 1: // if Sections layer is selected
		m_editSectionNum.EnableWindow(TRUE);
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::zoomToSelectedFeature(IActiveViewPtr ipActiveView)
{
	IMxDocumentPtr ipMxDoc(m_ipApp->Document);
	IMapPtr ipMap(ipMxDoc->FocusMap);

	IEnumFeaturePtr ipEnumFeature(ipMap->FeatureSelection);
	ASSERT_RESOURCE_ALLOCATION("ELI08339", ipEnumFeature != NULL);
	ipEnumFeature->Reset();
	IFeaturePtr ipFeature = ipEnumFeature->Next();
	IEnvelopePtr ipEnvelope(NULL);
	while (ipFeature != NULL)
	{
		IEnvelopePtr ipIndividualEnvelope = ipFeature->Extent;
		if (ipEnvelope == NULL)
		{
			ipEnvelope = ipIndividualEnvelope;
		}
		else
		{
			if (ipEnvelope->GetXMin() > ipIndividualEnvelope->GetXMin())
			{
				ipEnvelope->PutXMin(ipIndividualEnvelope->GetXMin());
			}

			if (ipEnvelope->GetXMax() < ipIndividualEnvelope->GetXMax())
			{
				ipEnvelope->PutXMax(ipIndividualEnvelope->GetXMax());
			}

			if (ipEnvelope->GetYMin() > ipIndividualEnvelope->GetYMin())
			{
				ipEnvelope->PutYMin(ipIndividualEnvelope->GetYMin());
			}
			
			if (ipEnvelope->GetYMax() < ipIndividualEnvelope->GetYMax())
			{
				ipEnvelope->PutYMax(ipIndividualEnvelope->GetYMax());
			}
		}

		ipFeature = ipEnumFeature->Next();
	}

	if (ipEnvelope)
	{
		// expand the envelope a little
		ipEnvelope->Expand(1.3, 1.3, VARIANT_TRUE);
		ipActiveView->Extent = ipEnvelope;
		ipActiveView->Refresh();
	}
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::addLayerFromINItoLB( std::string strLayerIDName)
{
	string strINIPath = ::getModuleDirectory(_Module.m_hInst) + "\\" + string( "GridGenerator.ini" );
	INIFilePersistenceMgr	mgrSettings( strINIPath  );
	
	// Create folder name from strSection
	string strFolder = strINIPath;
	strFolder += "\\";
	strFolder += "Layers";
	
	string strLayerName;
	// populate the layer combo box
	strLayerName = mgrSettings.getKeyValue( strFolder, strLayerIDName, "" );
	if ( !strLayerName.empty() )
	{
		m_cmbLayer.AddString(strLayerName.c_str());
	}
	else
	{
		UCLIDException ue("ELI12677", "Missing Layer Name in GridGenerator.ini file");			
		ue.addDebugInfo ( "LayerNameMissing", strLayerIDName );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void QueryDlg::addANDClause( std::string& strWhereClause, const std::string &strFldName, std::string strValue, esriFieldType fieldType )
{
	if ( !strValue.empty() )
	{
		if ( !strWhereClause.empty() )
		{
			strWhereClause += " AND ";
		}
		if  ( fieldType == esriFieldTypeString )
		{
			strWhereClause += strFldName + " = '" + strValue + "'";
		}
		else
		{
			strWhereClause += strFldName + " = " + strValue;
		}
	}	
}
//-------------------------------------------------------------------------------------------------
