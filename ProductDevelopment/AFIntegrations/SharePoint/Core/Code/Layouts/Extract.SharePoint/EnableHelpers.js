/// <reference name="MicrosoftAjax.js" />
/// <reference path="file://C:/Program Files/Common Files/Microsoft Shared/Web Server Extensions/14/TEMPLATE/LAYOUTS/SP.core.debug.js" />
/// <reference path="file://C:/Program Files/Common Files/Microsoft Shared/Web Server Extensions/14/TEMPLATE/LAYOUTS/SP.debug.js" />

// Helper functions used to enable/disable ribbon buttons

var selectedListID;

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

// Variables that are only used as part of process selected.
var inProgress = false;
// variable to hold the Process Selected wait dialog
var waitProcessSelected;
var numberToProcess;
var numberProcessed;
var statusColumnName;

// Function to set all of the selected items to "ToBeQueued"
function processSelected(columnName) {
    statusColumnName = columnName;
    if (inProgress) {
        alert("Another request is in progress. Try again later!");
    }
    else {
        try {
            inProgress = true;
            waitProcessSelected = SP.UI.ModalDialog.showWaitScreenWithNoClose('Process Selected...', 'Please wait while files are set to be queued', 76, 330);
            var context = SP.ClientContext.get_current();
            var web = context.get_web();
            var selectedItems = SP.ListOperation.Selection.getSelectedItems();
            var selectedListID = SP.ListOperation.Selection.getSelectedList();

            this.numberToProcess = selectedItems.length;
            this.numberProcessed = 0;
            for (var i = 0; i < selectedItems.length; i++) {
                try {
                    var listItem = web.get_lists().getById(selectedListID).getItemById(selectedItems[i].id);
                    context.load(listItem);

                    // Setup call back with the current list item
                    var itemSuccessCallback = Function.createCallback(processSingleItemSuccess, listItem);
                    context.executeQueryAsync(Function.createDelegate(this, itemSuccessCallback), Function.createDelegate(this, processSingleFailed));
                }
                catch (ef) {
                    alert("Error:" + ef.message);
                }
            }
        }
        catch (e) {
            alert("Error: " + e.message);
            cleanupAfterProcessing();
        }
    }
}

// function that sets the satus to "To Be Queued" if query was successful
function processSingleItemSuccess(sender, args, listItem) {
    try {
        // get the current status for the item
        var currStatus = listItem.get_item(statusColumnName);

        // Get the file from the Item
        var file = listItem.get_file();

        // Only change the status if file is currently not processing or queued for verification
        if ((currStatus != "Queued For Processing") && (currStatus != "Queued For Verification")) {
            // Check out the file
            file.checkOut();

            // Set the status
            listItem.set_item(statusColumnName, "To Be Queued");

            // if this is the IDShieldStatus column need to cleare the IDSReference column
            if (statusColumnName == "IDShieldStatus") {
                listItem.set_item("IDSReference", "");
            }

            // update the listItem
            listItem.update();

            // check in the change
            file.checkIn(statusColumnName + " changed");
            var context = SP.ClientContext.get_current();
            context.executeQueryAsync(Function.createDelegate(this, singleSuccess), Function.createDelegate(this, processSingleFailed));
        }
        else {
            alert("Unable to change status of file if status is \"" + currStatus.toString() + "\"");
        }
    }
    catch (e) {
        alert("Unable to update the " + statusColumnName + " column.");
    }
    numberProcessed++;
    if (numberProcessed >= numberToProcess) {
        cleanupAfterProcessing();
    }

}

// function for successful running 2nd async query
function singleSuccess(sender, args) {
    numberProcessed++
    if (numberProcessed >= numberToProcess) {
        cleanupAfterProcessing();
    }
}

// function for failure of Async query
function processSingleFailed(sender, args) {
    numberProcessed++;
    if (numberProcessed >= numberToProcess) {
        cleanupAfterProcessing();
    }
    alert("Failed: " + args.get_message());
}

// function that resets the inProcess flag, clears the wait page and refreshes the page
function cleanupAfterProcessing() {
    inProgress = false;
    if (waitProcessSelected != null) {
        waitProcessSelected.close();
        waitPrcoessSelected = null;
    };
    window.location.href = window.location.href;
}


// The following function was found http://jeffywang.blogspot.com/2012/10/how-to-check-whether-current-user.html
// Sample usage 
//        ExecuteOrDelayUntilScriptLoaded(function () {
//            isCurrentUserInGroup('Approvers',
//                                 function () {
//                                     alert('User is in the group.');
//                                 },
//                                 function () {
//                                     alert('User is NOT in the group.');
//                                 },
//                                 function () {
//                                     alert('Group does NOT exist.');
//                                 },
//                                 function (sender, args) {
//                                     alert('Request failed.' + args.get_message() + '\n' + args.get_stackTrace());
//                                 });
//        }, "sp.js");
//
function isCurrentUserInGroup(groupName, functionUserInGroup, functionUserNotInGroup, functionGroupNotFound, functionOnError) {
    var ctx = new SP.ClientContext.get_current();
    //Get info of current user
    var currentUser = ctx.get_web().get_currentUser();
    ctx.load(currentUser, 'Id', 'LoginName');
    //Retrieve all site groups
    var groups = ctx.get_web().get_siteGroups();
    ctx.load(groups);

    ctx.executeQueryAsync(function (sender, args) {
        //See whether the group exists
        var groupFound = false;
        var groupEnumerator = groups.getEnumerator();
        while (groupEnumerator.moveNext() && !groupFound) {
            var group = groupEnumerator.get_current();
            if (group.get_title() == groupName) {
                //Found the group, now try to load users of this group
                groupFound = true;
                var users = group.get_users();
                ctx.load(users);
                ctx.executeQueryAsync(function (sender, args) {
                    //See whether the user exists
                    var userFound = false;
                    var userEnumerator = users.getEnumerator();
                    while (userEnumerator.moveNext() && !userFound) {
                        var user = userEnumerator.get_current();
                        if (user.get_id() == currentUser.get_id()) {
                            //User exists in the group
                            userFound = true;

                            if (functionUserInGroup != null)
                                functionUserInGroup();
                        }
                    }

                    //User doesn't exist in the group
                    if (!userFound && (functionUserNotInGroup != null))
                        functionUserNotInGroup();

                }, function (sender, args) {
                    if (functionOnError != null)
                        functionOnError(sender, args);

                });
            }
        }

        //Group doesn't exist
        if (!groupFound && (functionGroupNotFound != null))
            functionGroupNotFound();

    }, function (sender, args) {
        if (functionOnError != null)
            functionOnError(sender, args);

    })

}