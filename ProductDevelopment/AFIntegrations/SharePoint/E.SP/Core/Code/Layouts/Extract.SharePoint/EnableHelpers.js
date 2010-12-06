/// <reference name="MicrosoftAjax.js" />
/// <reference path="file://C:/Program Files/Common Files/Microsoft Shared/Web Server Extensions/14/TEMPLATE/LAYOUTS/SP.core.debug.js" />
/// <reference path="file://C:/Program Files/Common Files/Microsoft Shared/Web Server Extensions/14/TEMPLATE/LAYOUTS/SP.debug.js" />

// Helper functions used to enable/disable ribbon buttons

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
                    