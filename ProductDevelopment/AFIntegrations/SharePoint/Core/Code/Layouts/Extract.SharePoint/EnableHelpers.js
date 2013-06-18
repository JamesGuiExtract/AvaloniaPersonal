/// <reference name="MicrosoftAjax.js" />
/// <reference path="file://C:/Program Files/Common Files/Microsoft Shared/Web Server Extensions/14/TEMPLATE/LAYOUTS/SP.core.debug.js" />
/// <reference path="file://C:/Program Files/Common Files/Microsoft Shared/Web Server Extensions/14/TEMPLATE/LAYOUTS/SP.debug.js" />

// Helper functions used to enable/disable ribbon buttons

var inProgress = false;
var selectedListID;

// variable to hold the Process Selected wait dialog
var waitProcessSelected;

// Returns true iff 1 item is selected
function singleSelectEnabled()
{
    var items = SP.ListOperation.Selection.getSelectedItems();
    var count = CountDictionary(items);
    return (count == 1);
}

// Returns true iff 1 document is selected
function singleDocSelectEnabled()
{
    var items = SP.ListOperation.Selection.getSelectedItems();
    var count = CountDictionary(items);
    if (count != 1)
    {
        return false;
    }
    var itemType = items[0].fsObjType;
    return itemType == 0;  // 0 indicates file, 1 indicates folder
}

// Returns true if at least 1 item is selected
function atLeastOneSelectEnabled()
{
    var items = SP.ListOperation.Selection.getSelectedItems();
    var count = CountDictionary(items);
    return (count >= 1);
}

// Returns true if there is at least 1 item selected
// and all selected items are documents
function atLeastOneDocSelectEnabled()
{
    var items = SP.ListOperation.Selection.getSelectedItems();
    var count = CountDictionary(items);
    if (count < 1)
    {
        return false;
    }

    for (i = 0; i < count; i++)
    {
        var itemType = items[i].fsObjType;
        if (itemType != 0) // 0 indicates file, 1 indicates folder
        {
            return false;
        }
    }

    return true;
}

// Returns true if no items are selected
function noSelectionEnabled()
{
    var items = SP.ListOperation.Selection.getSelectedItems();
    var count = CountDictionary(items);
    return (count == 0);
}

// Builds the server relative folder path for the folder url
function getFolderFromUrl(url, siteRoot)
{
    // Look for ?RootFolder=
    var index = url.indexOf('?RootFolder=');
    var folder = '';
    if (index != -1)
    {
        // Folder is string following RootFolder=
        folder = url.substr(index + 12);
    }
    else
    {
        // RootFolder not found, search for the Forms/AllItems.aspx index
        index = url.indexOf('/Forms/AllItems');
        if (index != -1)
        {
            var temp = url.substring(0, index);
            index = temp.lastIndexOf('/');
            folder = temp.substr(index);
        }
    }

    return folder;
}

// Function to set all of the selected items to "ToBeQueued"
function processSelected(columnName) {
    if (inProgress) {
        alert("Another request is in progress. Try again later!");
    }
    else {
        try {
            inProgress = true;
            var context = SP.ClientContext.get_current();
            var web = context.get_web();
            var selectedItems = SP.ListOperation.Selection.getSelectedItems();
            selectedListID = SP.ListOperation.Selection.getSelectedList();
            for (var i = 0; i < selectedItems.length; i++) {
                try {
                    var listItem = web.get_lists().getById(selectedListID).getItemById(selectedItems[i].id);
                    var file = listItem.get_file();
                    if (file != null) {
                        file.checkOut();
                        listItem.set_item(columnName, "To Be Queued");
                        if (columnName == "IDShieldStatus") {
                            listItem.set_item("IDSReference", "");
                        }
                        listItem.update();
                        file.checkIn("IDS Status changed");
                    }
                }
                catch (ef) {
                    alert("Error:" + ef);
                }
            }
            context.executeQueryAsync(Function.createDelegate(this, itemsQueued), Function.createDelegate(this, itemQueuingFailed));
            waitProcessSelected = SP.UI.ModalDialog.showWaitScreenWithNoClose('Process Selected...', 'Please wait while files are set to be queued', 76, 330);
        }
        catch (e) {
            alert("Error:" + e);
            inProgress = false;
        }
    }
}

// Function that is called if items are queued sucessfully
function itemsQueued(sender, args) {
    inProgress = false;
    if (waitProcessSelected != null){
        waitProcessSelected.close();
        waitPrcoessSelected = null;
    };
    window.location.href = window.location.href;
}

// Function that is called if items fail to be queued
function itemQueuingFailed() {
    alert("Queuing failed: " + args.get_message());
    inProgress = false;
    if (waitProcessSelected != null){
        waitProcessSelected.close();
        waitPrcoessSelected = null;
    };
        
    window.location.href = window.location.href;
}


// This function was to be used for Verify button but so far 
// have not gotten it to work.
//function singleDocSelectedAndQueuedForVerification(columnName) {
//    var items = SP.ListOperation.Selection.getSelectedItems();
//    var count = CountDictionary(items);
//    if (count != 1) {
//        return false;
//    }

//    var listID = SP.ListOperation.Selection.getSelectedList();
//    var listItem = web.get_lists().getById(listID).getItemById(items[0].id);
//    var status = listItem.get_item(columnName);

//    return status == 'Queued For Verification';
//}