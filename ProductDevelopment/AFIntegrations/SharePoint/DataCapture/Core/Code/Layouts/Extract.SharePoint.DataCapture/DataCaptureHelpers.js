/// <reference name="MicrosoftAjax.js" />
/// <reference path="file://C:/Program Files/Common Files/Microsoft Shared/Web Server Extensions/14/TEMPLATE/LAYOUTS/SP.core.debug.js" />
/// <reference path="file://C:/Program Files/Common Files/Microsoft Shared/Web Server Extensions/14/TEMPLATE/LAYOUTS/SP.debug.js" />

// Displays a failure message if the asynchronous calls fail
function OnFailure(sender, args)
{
    alert('request failed ' + args.get_message() + '\n' + args.get_stackTrace());
}

// Gets the site data asynchronously and if successful displays the
// Watch folder configuration page in a SP modal dialog box
function showProcessFolderConfiguration()
{
    // Get the current context, web, site and folder
    this.context = new SP.ClientContext.get_current();
    this.listid = SP.ListOperation.Selection.getSelectedList();
    this.site = context.get_site();
    context.load(this.site, "Id");

    // Execute the asynch query to get the data from SP
    context.executeQueryAsync(Function.createDelegate(this, this.OnShowProcessSuccess),
        Function.createDelegate(this, this.OnFailure));
}

// Displays a notification box to the user as well as refreshing the current page after the dialog closes.
function onProcessSuccessCallback(result, target) {

    if (result == SP.UI.DialogResult.OK) {
        SP.UI.Notify.addNotification('Folder settings updated.');
    }
    SP.UI.ModalDialog.RefreshPage(SP.UI.DialogResult.OK);
}

// Called asynchronously, displays the SP modal dialog box with the
// Watch folder configuration page in a SP modal dialog box
function OnShowProcessSuccess(sender, args)
{
    var siteId = this.site.get_id();
    var folder = getFolderFromUrl(location.href);
    var options =
    {
        url: '/_layouts/Extract.SharePoint.DataCapture/ProcessFolderSettings.aspx?'
            + 'listidvalue=' + listid + '&folder=' + folder + '&siteid=' + siteId,
        title: 'Process Current Folder',
        allowMaximize: false,
        showClose: true,
        dialogReturnValueCallback: onProcessSuccessCallback
    };

    SP.UI.ModalDialog.showModalDialog(options);
}

// Gets the site data asynchronously and if successful displays the
// Unwatch folder configuration page in a SP modal dialog box
function unwatchCaptureFolder()
{
    // Get the current context, web, site and folder
    this.context = new SP.ClientContext.get_current();
    this.site = context.get_site();
    context.load(this.site, "Id");

    // Execute the asynch query to get the data from SP
    context.executeQueryAsync(Function.createDelegate(this, this.OnShowUnwatchCaptureSuccess),
        Function.createDelegate(this, this.OnFailure));
}

// Called asynchronously, displays the SP modal dialog box with the
// Unwatch folder configuration page in a SP modal dialog box
function OnShowUnwatchCaptureSuccess(sender, args)
{
    var siteId = this.site.get_id();
    var folder = getFolderFromUrl(location.href);
    var options =
    {
        url: '/_layouts/Extract.SharePoint.DataCapture/RemoveFolderSettings.aspx?'
            + 'folder=' + folder + '&siteid=' + siteId,
        title: 'Unwatch Current Folder',
        allowMaximize: false,
        showClose: true
    };

    SP.UI.ModalDialog.showModalDialog(options);
}

// Function called to select for data capture
function processSelectedForDataCapture() {
    processSelected("ExtractDataCaptureStatus");
}