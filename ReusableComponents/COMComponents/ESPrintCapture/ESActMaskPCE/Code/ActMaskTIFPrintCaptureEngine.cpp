// ActMaskTIFPrintCaptureEngine.cpp : Implementation of CActMaskTIFPrintCaptureEngine

#include "stdafx.h"
#include "ActMaskTIFPrintCaptureEngine.h"

#include <UCLIDException.h>
#include <RegistryPersistenceMgr.h>
#include <cpputil.h>
#include <ComUtils.h>

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
//------------------------------------------------------------
// names of exe's for install and uninstall along 
// with the command line arguments for the exe's
// DO NOT EDIT THESE VALUES UNLESS GIVEN A NEW PASSWORD
// OR INSTALLER/UNINSTALLER FROM ACT MASK
// THESE VALUES ARE NOT CUSTOMIZEABLE
//------------------------------------------------------------
// name of the actmask image printer silent installer
const string gstrACTMASK_IMAGE_PRINTER_INSTALLER = "virtual-printer-sdk-image-full.exe";

// name of the uninstaller created by the ActMask installer
// the uninstaller will be located in the common application data folder
const string gstrACTMASK_IMAGE_PRINTER_UNINSTALLER = 
	"\\ActMask Image Virtual Printer SDK\\unins000.exe";

// password for the ActMask silent installer
const string gstrPASSWORD = "ag@extractsystems.com";

// command line arguments for the ActMask silent installer
const string gstrINSTALL_ARGS = "/VERYSILENT /PASSWORD=" + gstrPASSWORD;

//------------------------------------------------------------
// registry keys and values to set
// DO NOT EDIT THESE KEY NAMES THEY ARE REQUIRED FOR 
// THE ACT MASK DRIVER TO FUNCTION PROPERLY
//------------------------------------------------------------
// registry path for the ActMaskPrint driver values
const string gstrPRINTER_SDK_REG_ROOT_KEY = "Software\\ActMask Image Virtual Printer SDK";
const string gstrPRINTER_REG_ROOT_KEY = "Software\\ActMask Virtual Printer";

// AppFileName
// Value - full path to application to run after printing
// The application path will be passed to the Install method
const string gstrREG_APP_KEY = "AppFileName";

// TransMode
// 0 - CommandLine mode
// 1 - WM_COPYDATA Message mode
// 2 - Clipboard mode
const string gstrREG_TRANSMODE_KEY = "TransMode";
const DWORD gdwREG_TRANSMODE_VALUE = 0;

// OutputFormat
const string gstrREG_FORMAT_KEY = "OutputFormat";
const string gstrREG_FORMAT_VALUE = "TIF";

// ResizingRate
// string value 0 ~ 100
// 0 indicates custom size so need to set ResizingWidth and ResizingHeight
// 27 = 27% ~96DPI
// 33 = 33% ~120DPI
// 100 = 100% Highest quality
const string gstrREG_RESIZERATE_KEY = "ResizingRate";
const string gstrREG_RESIZERATE_VALUE = "100";

// ResizingMode
// 0 - Fastest speed, low quality
// 1 - Nearest Neighbor, medium quality
// 2 - Bilinear, better quality
// If a document contains lines or a grid 0 mode is not recommended
const string gstrREG_RESIZEMODE_KEY = "ResizingMode";
const DWORD gdwREG_RESIZEMODE_VALUE = 2;

//--------------------------------------------------------------------------------------------------
// CActMaskTIFPrintCaptureEngine
//--------------------------------------------------------------------------------------------------
CActMaskTIFPrintCaptureEngine::CActMaskTIFPrintCaptureEngine()
{
}
//--------------------------------------------------------------------------------------------------
CActMaskTIFPrintCaptureEngine::~CActMaskTIFPrintCaptureEngine()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20708");
}
//--------------------------------------------------------------------------------------------------
HRESULT CActMaskTIFPrintCaptureEngine::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CActMaskTIFPrintCaptureEngine::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CActMaskTIFPrintCaptureEngine::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IActMaskTIFPrintCaptureEngine,
		&IID_IPrintCaptureEngine
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// IPrintCaptureEngine
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CActMaskTIFPrintCaptureEngine::raw_Install(BSTR bstrHandlerApp)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	INIT_EXCEPTION_AND_TRACING("MLI00093");

	try
	{
		string strModuleDir = getModuleDirectory(_AtlBaseModule.m_hInstResource);
		strModuleDir += "\\";

		// build path to installer application
		string strInstaller(strModuleDir + gstrACTMASK_IMAGE_PRINTER_INSTALLER);

		// install custom ID Shield print capture driver (wait for completion)
		runEXE(strInstaller, gstrINSTALL_ARGS, INFINITE);
		
		// open the HKLM registry key
		RegistryPersistenceMgr regMgrLocal(HKEY_LOCAL_MACHINE, "");
		_lastCodePos = "10";

		// set registry settings for the virtual printer
		regMgrLocal.setKeyValue(gstrPRINTER_SDK_REG_ROOT_KEY, gstrREG_APP_KEY, 
			asString(bstrHandlerApp)); 
		_lastCodePos = "20";

		regMgrLocal.setKeyValue(gstrPRINTER_SDK_REG_ROOT_KEY, gstrREG_TRANSMODE_KEY, 
			gdwREG_TRANSMODE_VALUE);
		_lastCodePos = "30";

		regMgrLocal.setKeyValue(gstrPRINTER_SDK_REG_ROOT_KEY, gstrREG_FORMAT_KEY, 
			gstrREG_FORMAT_VALUE);
		_lastCodePos = "40";

		regMgrLocal.setKeyValue(gstrPRINTER_SDK_REG_ROOT_KEY, gstrREG_RESIZERATE_KEY, 
			gstrREG_RESIZERATE_VALUE);
		_lastCodePos = "50";

		regMgrLocal.setKeyValue(gstrPRINTER_SDK_REG_ROOT_KEY, gstrREG_RESIZEMODE_KEY, 
			gdwREG_RESIZEMODE_VALUE);
		_lastCodePos = "60";
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20725");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CActMaskTIFPrintCaptureEngine::raw_Uninstall()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// get the path to the common application data folder
		string strUninstallerPath = "";
		getSpecialFolderPath(CSIDL_COMMON_APPDATA, strUninstallerPath);

		// build the path to the uninstaller
		strUninstallerPath += gstrACTMASK_IMAGE_PRINTER_UNINSTALLER;

		// ensure the uninstaller exists
		validateFileOrFolderExistence(strUninstallerPath);

		// call the ActMask printer uninstaller
		runEXE(strUninstallerPath, "/VERYSILENT", INFINITE);

		// registry key for SDK should be HKLM\Software\ActMask Image Virtual Printer SDK
		RegistryPersistenceMgr regMgrLM(HKEY_LOCAL_MACHINE, "");

		// delete the SDK registry entries
		regMgrLM.deleteFolder(gstrPRINTER_SDK_REG_ROOT_KEY);

		// delete the Virtual Printer registry entries
		regMgrLM.deleteFolder(gstrPRINTER_REG_ROOT_KEY);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20841");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
