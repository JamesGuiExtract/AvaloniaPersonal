//////////////////////////////////////////////////////
// Constants for Spot Recognition Window shortcut keys
//////////////////////////////////////////////////////

// Key to refresh current image
const unsigned int	guiSRW_REFRESH_FILE = VK_F5;

// Key to Zoom In one step
const unsigned int	guiSRW_ZOOM_IN = VK_F7;

// Key to Zoom Out one step
const unsigned int	guiSRW_ZOOM_OUT = VK_F8;

// Key to Zoom Extents
// Removed per P13 #3937 - WEL 11/21/06
// Functionality replaced with Fit To Page
const unsigned int	guiSRW_ZOOM_EXTENTS = 'X';

// Key to Zoom Previous
const unsigned int	guiSRW_ZOOM_PREVIOUS = 'R';

// Key to Fit To Page
const unsigned int	guiSRW_FIT_PAGE = 'P';

// Key to Fit To Width
const unsigned int	guiSRW_FIT_WIDTH = 'W';

// Key to turn on Pan
const unsigned int	guiSRW_ACTIVATE_PAN = 'A';

// Key to turn on Highlighting (Redacting)
const unsigned int	guiSRW_HIGHLIGHT = 'H';

// Key to navigate to previous page
const unsigned int	guiSRW_PREVIOUS_PAGE = VK_F3;

// Key to navigate to next page
const unsigned int	guiSRW_NEXT_PAGE = VK_F4;

// Key to navigate to previous page
const unsigned int	guiSRW_PREVIOUS_PAGE2 = VK_PRIOR;

// Key to navigate to next page
const unsigned int	guiSRW_NEXT_PAGE2 = VK_NEXT;

// Key to navigate to first page
// NOTE: Handler is checking for a control key modifier as well (See [FlexIDSCore:3443])
const unsigned int	guiSRW_FIRST_PAGE = VK_HOME;

// Key to navigate to last page
// NOTE: Handler is checking for a control key modifier as well (See [FlexIDSCore:3443])
const unsigned int	guiSRW_LAST_PAGE = VK_END;

// Key to activate Zoom (same as pressing Zoom button on toolbar)
const unsigned int	guiSRW_ZOOM = 'Z';

// Key to Open sub-image (same as pressing Sub-image button on toolbar)
const unsigned int	guiSRW_SUBIMAGE = 'I';

// Key to Delete entities
const unsigned int	guiSRW_DELETE_ENTITIES = 'D';

// Key to activate select highlight tool
const unsigned int guiSRW_SELECT_ENTITIES = VK_ESCAPE;