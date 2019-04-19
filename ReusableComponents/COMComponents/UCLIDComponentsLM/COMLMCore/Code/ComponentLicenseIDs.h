// This file must stay aligned with Components.dat used in license file creation.
//
//-------------------------------------------------------------------------------------------------
// Constants to define which passwords are used to decrypt Extract Systems license files
//-------------------------------------------------------------------------------------------------
const unsigned int gnDEFAULT_PASSWORDS					= 0;
const unsigned int gnICOMAP_PASSWORDS					= 1;
// Used for both FLEX Index and ID Shield rule writing
const unsigned int gnSIMPLE_RULE_WRITING_PASSWORDS		= 2;

//-------------------------------------------------------------------------------------------------
// ID offset to be modified for each major version release
// HISTORY
// - FLEX Index and ID Shield 5.0				1000
// - IcoMap for ArcGIS 4.2						1000
// - FLEX Index and ID Shield 6.0				2000
// - ID Shield for Laserfiche 1.0				2000
// - ID Shield Office 1.0						2000
// - FLEX Index and ID Shield 7.0				3000
// - LabDE 1.0									4000
// - FLEX Index and ID Shield 8.0				5000
// - FLEX Index & ID Shield & LabDE 9.0			6000
// - FLEX Index & ID Shield & LabDE 10.0		6000
//-------------------------------------------------------------------------------------------------
const unsigned int gnBASE_OFFSET						= 6000;

//-------------------------------------------------------------------------------------------------
// Constants for core objects
//-------------------------------------------------------------------------------------------------
// The ExtractCore package is for all reusable components that are 
// not specifically part of any of the packages below, but are used
// in more than one product.
const unsigned int gnEXTRACT_CORE_OBJECTS				= gnBASE_OFFSET + 1;

// The InputFunnelCore package is for all input funnel components that
// would be shipped with any end-user product that uses the
// InputFunnel technology.  Examples of components that are
// included are the SpotRecognition window (minus OCR functionality),
// input validators, etc.
const unsigned int gnINPUTFUNNEL_CORE_OBJECTS			= gnBASE_OFFSET + 2;

// The FlexIndexAndIDShieldCore package is for all components that will be
// shipped to all FLEX Index and ID Shield customers.  Examples
// of components included are the general purpose rule objects,
// File action manager, various utilities like RuleTester and
// UEXViewer, etc.  Specific rule objects (e.g. LegalDescriptionFinder)
// are not part of this package.  Rule objects that are required for 
// Simple Rule Writing (e.g. ValueAfterClue, XMLOutputHandler, etc.) 
// are not part of this package.
const unsigned int gnFLEXINDEX_IDSHIELD_CORE_OBJECTS	= gnBASE_OFFSET + 3;

// The FLEX Index package is for all components that are specific to
// FLEX Index (e.g. LegalDescriptionFinder) and not included in the
// FlexIndexAndIDShieldCore package and shipped to all FLEX Index
// customers.
const unsigned int gnFLEXINDEX_CORE_OBJECTS				= gnBASE_OFFSET + 4;

// The ID Shield package is for all components that are specific to
// ID Shield and not included in the FlexIndexAndIDShieldCore package 
// and shipped to all ID Shield customers.
const unsigned int gnIDSHIELD_CORE_OBJECTS				= gnBASE_OFFSET + 5;

// The RuleDevelopmentToolkit package is for all components specific
// to the development and testing of rules, such as automated tester
// components, rule object property pages.  The RuleSetEditor UI is 
// not included here because it has its own individual ID allowing for 
// use in Simple Rule Writing.
const unsigned int gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS	= gnBASE_OFFSET + 6;

// The InputFunnelSDK package is for all components that are not part of the
// core InputFunnel package, but are delivered to customers who will be 
// integrating the InputFunnel technology at the COM object level.
// *** THIS PACKAGE IS NOT USED AT THIS TIME ***
const unsigned int gnINPUTFUNNEL_SDK_OBJECTS			= gnBASE_OFFSET + 9;

// The Redaction Core package is for all components that are specific to 
// any redaction product including ID Shield and ID Shield Office.
const unsigned int gnREDACTION_CORE_OBJECTS             = gnBASE_OFFSET + 10;


//-------------------------------------------------------------------------------------------------
// Constants for function-specific objects and features
//-------------------------------------------------------------------------------------------------
// This package is for all components that must be licensed
// for OCR functionality to be able to run in a 24x7 automated mode
// (e.g. AttributeFinderEngine).
const unsigned int gnOCR_ON_SERVER_FEATURE				= gnBASE_OFFSET + 21;

// This package is for all components that must be licensed
// for client-side "swiping" or other user-event-triggered
// OCR functionality (e.g. ESOCR, SpotRecognitionWindow, RuleTester).
const unsigned int gnOCR_ON_CLIENT_FEATURE				= gnBASE_OFFSET + 22;

// This package is for all components that must be licensed
// on the server machine where FLEX Index is running.
// - All FLEX Index + AttributeFinderEngine
// - NOT Pagination file processor
const unsigned int gnFLEXINDEX_SERVER_OBJECTS			= gnBASE_OFFSET + 23;

// This package is for simple rule objects to support Waukesha County 
// rule writing.  Core objects and Ruleset Editor UI are licensed with 
// separate IDs.
// https://extract.atlassian.net/browse/ISSUE-12454
// gnSIMPLE_RULE_OBJECTS used to be a separate licensable component, but is now one and the same
// with gnRULE_WRITING_CORE_OBJECTS. Because of the way GenerateDotNetLicenseIDFiles generates an
// enum by assiging the ID as the explicit value, gnSIMPLE_RULE_OBJECTS cannot remain a separate
// entry here alongside gnRULE_WRITING_CORE_OBJECTS.
// const unsigned int gnSIMPLE_RULE_OBJECTS				= gnBASE_OFFSET + 24;
const unsigned int gnRULE_WRITING_CORE_OBJECTS			= gnBASE_OFFSET + 24;

// This package is for all components that are only licensed on
// a FLEX Index or ID Shield server machine
// (e.g. AdjustImageResolution, EmailFile, SplitFile).
const unsigned int gnFLEXINDEX_IDSHIELD_SERVER_CORE		= gnBASE_OFFSET + 25;

// This package is for objects to support simple rule writing by ID Shield 
// customers.
const unsigned int gnIDSHIELD_RULE_WRITING_OBJECTS		= gnBASE_OFFSET + 26;

// This package is for objects to support simple rule writing by FLEX Index 
// customers.
const unsigned int gnFLEXINDEX_RULE_WRITING_OBJECTS		= gnBASE_OFFSET + 27;

// This item is for objects that are specific to LabDE.
const unsigned int gnLABDE_CORE_OBJECTS					= gnBASE_OFFSET + 28;

// This item is for general-purpose Data Entry components.
const unsigned int gnDATA_ENTRY_CORE_COMPONENTS			= gnBASE_OFFSET + 29;


//-------------------------------------------------------------------------------------------------
// Constants for individual objects
//-------------------------------------------------------------------------------------------------
// Special ID for Pagination File Processor allowing this item to be 
// disabled and unavailable for standard FLEX Index and ID Shield customers.
const unsigned int gnPAGINATION_FILEPROCESSOR_OBJECT	= gnBASE_OFFSET + 43;

// Special ID for Automatic Redaction allowing for use only in ID Shield 
// Server packages.
const unsigned int gnIDSHIELD_AUTOREDACTION_OBJECT		= gnBASE_OFFSET + 44;

// Special ID for Redaction Verification allowing for use only in ID Shield 
// Client packages.
const unsigned int gnIDSHIELD_VERIFICATION_OBJECT		= gnBASE_OFFSET + 45;

// Special ID for RuleSet Editor UI allowing for use in Simple Rule 
// Writing and Rule Development Toolkit packages.
const unsigned int gnRULESET_EDITOR_UI_OBJECT			= gnBASE_OFFSET + 46;

// Special ID for File Action Manager and associated applications
// to also include general-purpose file processors.
const unsigned int gnFILE_ACTION_MANAGER_OBJECTS		= gnBASE_OFFSET + 47;

// Special ID for RunRules application to be available on Servers 
// and for limited use ( text input only ) for FLEX Index Client.
const unsigned int gnRUN_RULES_OBJECT					= gnBASE_OFFSET + 48;

// Special ID for ID Shield Office application.
const unsigned int gnID_SHIELD_OFFICE_OBJECT			= gnBASE_OFFSET + 49;

// Special ID for the LabDE verification interface.
const unsigned int gnLABDE_VERIFICATION_UI_OBJECT		= gnBASE_OFFSET + 50;

// Special ID for the pagination UI (Pagination Utility)
const unsigned int gnPAGINATION_UI_OBJECT				= gnBASE_OFFSET + 51;

// Special ID for the Enhance OCR task and rule object
const unsigned int gnENHANCE_OCR						= gnBASE_OFFSET + 52;


//-------------------------------------------------------------------------------------------------
// Constants for individually licensed items
//-------------------------------------------------------------------------------------------------
// Special ID for Anti-Aliasing within the Spot Recognition Window.
// This ID is included in FlexIndexIDShieldCore package in Packages.dat.
const unsigned int gnANTI_ALIASING_FEATURE				= gnBASE_OFFSET + 70;

// Special ID for ignoring an RSD file requirement for a rule execution counter. This license
// is not for general use. This package NOT available for Rule Writers and also NOT available for
// external use.
const unsigned int gnIGNORE_RULE_EXECUTION_COUNTER_DECREMENTS	= gnBASE_OFFSET + 72;

// Special ID for reading and writing PDF files within the Spot Recognition Window.
const unsigned int gnPDF_READWRITE_FEATURE				= gnBASE_OFFSET + 73;

// Special ID for reading and writing annotations.  This package is useful 
// for ID Shield customers who: have images with pre-existing annotations that 
// should be retained, would like to save automatic and/or manual redactions 
// as annotations, or both.
// This package is also required for the LeadTools Document Toolkit used to 
// properly deal with certain rotated images.
const unsigned int gnANNOTATION_FEATURE					= gnBASE_OFFSET + 76;

// Special ID for image cleanup functionality in FLEX Index / ID Shield.
const unsigned int gnIMAGE_CLEANUP_ENGINE_FEATURE		= gnBASE_OFFSET + 77;

// Special ID for handwriting recognition within SSOCR2
const unsigned int gnHANDWRITING_RECOGNITION_FEATURE	= gnBASE_OFFSET + 78;

// Special ID for creating searchable PDFs using the RecAPI OCR engine
const unsigned int gnCREATE_SEARCHABLE_PDF_FEATURE		= gnBASE_OFFSET + 79;

// Special ID for Scansoft OEM OCR license - required for ESConvertUSSToTXT
// This is needed because providing full-document OCR capability is prohibited 
// in our contract with Scansoft.  A special agreement was reached whereby 
// a customer that desires this functionality will pay a significant royalty 
// that Extract sends to Scansoft.  The royalty is approximately the same 
// as the cost to purchase a Scansoft SDK license.
const unsigned int gnSCANSOFT_OEM_OCR_FEATURE			= gnBASE_OFFSET + 80;

// Special ID for the Inlite Check21 engine in FLEX Index / ID Shield.
const unsigned int gnMICR_FINDING_ENGINE_FEATURE		= gnBASE_OFFSET + 81;

// Special ID to modify PDF files (remove annotations or add hyperlinks)
const unsigned int gnMODIFY_PDF							= gnBASE_OFFSET + 82;

// Special ID for the FTP/SFTP file transfer features that use edtFTPnet/PRO SDK
const unsigned int gnFTP_SFTP_FILE_TRANSFER				= gnBASE_OFFSET + 83;

// Special ID for the search capability of the FAM File Inspector
const unsigned int gnFILE_INSPECTOR_SEARCH				= gnBASE_OFFSET + 84;

// For the /RemovePages option on ImageFormatConverter.
const unsigned int gnREMOVE_IMAGE_PAGES					= gnBASE_OFFSET + 85;

// Enables logging of input and events in the data entry verification UI.
const unsigned int gnDATA_ENTRY_VERIFICATION_LOGGING	= gnBASE_OFFSET + 86;

// Enables PDF Read functionallity with out PDF Write
// This should be included in any license that includes gnPDF_READWRITE_FEATURE 
const unsigned int gnPDF_READ_ONLY						= gnBASE_OFFSET + 87;

// Disables the use of the semaphore for restricting the leadtools to 2 or 4 cores based on licens
// This is to allow licensing all the cores on a machine
const unsigned int gnLEADTOOLS_ALL_CORES				= gnBASE_OFFSET + 88;

// Adds +2 core license to base leadtools license
const unsigned int gnLEADTOOLS_2_EXTRA_CORES			= gnBASE_OFFSET + 89;

// Adds +4 core license to base leadtools license
const unsigned int gnLEADTOOLS_4_EXTRA_CORES			= gnBASE_OFFSET + 90;

//-------------------------------------------------------------------------------------------------
// Constants for items licensed for FLEX Index / ID Shield integration
//-------------------------------------------------------------------------------------------------
// IDs for FLEX Index & ID Shield integration with Laserfiche:

// IDS for LF: Verification only
const unsigned int gnLASERFICHE_VERIFICATION			= gnBASE_OFFSET + 101;

// IDS for LF: On-demand redaction and submission:
const unsigned int gnLASERFICHE_DESKTOP_REDACTION		= gnBASE_OFFSET + 102;

// IDS for LF: Background service:
const unsigned int gnLASERFICHE_SERVICE_REDACTION		= gnBASE_OFFSET + 103;

//-------------------------------------------------------------------------------------------------
// Constants for items licensed for NetDMS integration
//-------------------------------------------------------------------------------------------------
const unsigned int gnNETDMS_COMPONENTS					= gnBASE_OFFSET + 110;
