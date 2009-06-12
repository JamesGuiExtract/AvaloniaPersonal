//////////////////////////////////////////
// Constants for DataAreaDlg Shortcut keys
//////////////////////////////////////////

// Key to navigate to previous item (plus SHIFT)
const unsigned int	guiPREVIOUS_ITEM = VK_TAB;
const unsigned int	guiPREVIOUS_ITEM_VK = VK_SHIFT;

// Key to navigate to next item
const unsigned int	guiNEXT_ITEM = VK_TAB;
const unsigned int	guiNEXT_ITEM_VK = 0;

// Key to navigate to previous document (plus CONTROL+SHIFT)
// Note that guiPREVIOUS_DOC_VK must be the sum of two integer values and 
// not an OR'd combination.
const unsigned int	guiPREVIOUS_DOC = VK_TAB;
const unsigned int	guiPREVIOUS_DOC_VK = VK_SHIFT + VK_CONTROL;

// Key to navigate to next document (plus CONTROL)
const unsigned int	guiNEXT_DOC = VK_TAB;
const unsigned int	guiNEXT_DOC_VK = VK_CONTROL;

// Key to save current redactions and go on to the next file
const unsigned int	guiSAVE = 'S';
const unsigned int	guiSAVE_VK = VK_CONTROL;

// Key to toggle redaction setting for the currently selected attribute
const unsigned int	guiTOGGLE_REDACTION = ' ';
const unsigned int	guiTOGGLE_REDACTION_VK = 0;

// Key to turn on auto-zoom
const unsigned int	guiAUTO_ZOOM = VK_F2;
const unsigned int	guiAUTO_ZOOM_VK = 0;

// Key to show the type column drop down list [p16 #2836]
const unsigned int guiSHOW_DROP_DOWN_LIST = 'T';
const unsigned int guiSHOW_DROP_DOWN_LIST_VK = 0;

// Key to bring up the exemption codes dialog to apply to the selected item
const unsigned int guiAPPLY_EXEMPTION = 'E';
const unsigned int guiAPPLY_EXEMPTION_VK = 0;

// Key to apply the last applied exemption codes to the selected item
const unsigned int guiLAST_EXEMPTION = 'L';
const unsigned int guiLAST_EXEMPTION_VK = VK_CONTROL;

// Key to bring up the exemption codes dialog to apply to the all items
const unsigned int guiALL_EXEMPTION = 'E';
const unsigned int guiALL_EXEMPTION_VK = VK_CONTROL;
