// Helper functions used to enable/disable ribbon buttons

// Returns true iff 1 item is selected
function singleSelectEnabled()
{
    var items = SP.ListOperation.Selection.getSelectedItems();
    var count = CountDictionary(items);
    return (count == 1);
}

// Returns true if at least 1 item is selected
function atLeastOneSelectEnabled()
{
    var items = SP.ListOperation.Selection.getSelectedItems();
    var count = CountDictionary(items);
    return (count >= 1);
}

// Returns true if no items are selected
function noSelectionEnabled()
{
    var items = SP.ListOperation.Selection.getSelectedItems();
    var count = CountDictionary(items);
    return (count == 0);
}
                    