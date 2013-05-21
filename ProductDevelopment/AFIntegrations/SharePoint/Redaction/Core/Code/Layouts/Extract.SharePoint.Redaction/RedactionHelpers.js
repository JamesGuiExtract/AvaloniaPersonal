/// <reference name="MicrosoftAjax.js" />
/// <reference path="file://C:/Program Files/Common Files/Microsoft Shared/Web Server Extensions/14/TEMPLATE/LAYOUTS/SP.core.debug.js" />
/// <reference path="file://C:/Program Files/Common Files/Microsoft Shared/Web Server Extensions/14/TEMPLATE/LAYOUTS/SP.debug.js" />

// Displays a failure message if the asynchronous calls fail
function OnFailure(sender, args)
{
    alert('request failed ' + args.get_message() + '\n' + args.get_stackTrace());
}

// Displays the configure ID Shield page in a SP modal dialog box
function showConfigureIdShield()
{
    var options =
    {
        url: '/_layouts/Extract.SharePoint.Redaction/ConfigureIdShieldSettings.aspx',
        title: 'Configure ID Shield',
        allowMaximize: false,
        showClose: true
    };

    SP.UI.ModalDialog.showModalDialog(options);
}


// Gets the site data asynchronously and if successful displays the
// Redact now page in a SP modal dialog box
function showRedactNowHelper() {

    // Get the current context, web and site
    singleFileContext();

    // Execute the asynch query to get the data from SP
    context.executeQueryAsync(Function.createDelegate(this, this.OnShowRedactNowHelperSuccess),
        Function.createDelegate(this, this.OnFailure));
}

function singleFileContext() {
    // Get the current context, web and site
    this.context = new SP.ClientContext.get_current();
    this.web = context.get_web();
    this.site = context.get_site();

    // Get the list id for the selected list
    this.listid = SP.ListOperation.Selection.getSelectedList();

    // Get the selected list
    var list = this.web.get_lists().getById(listid);

    // Get the selected list item
    var fileid = SP.ListOperation.Selection.getSelectedItems()[0].id;
    this.listItem = list.getItemById(fileid);

    // Load the items from the web context
    context.load(this.site, 'Id');
    context.load(this.web);
    context.load(this.listItem, 'UniqueId');
}

// Called asynchronously, displays the SP modal dialog box with the
// Redact now helper page in a SP modal dialog box
function OnShowRedactNowHelperSuccess(sender, args)
{
    var siteId = this.site.get_id();
    var fileid = this.listItem.get_item('UniqueId');
    var options =
    {
        url: '/_layouts/Extract.SharePoint.Redaction/RedactNowHelper.aspx?'
            + 'listidvalue=' + listid + '&fileid=' + fileid + '&siteid=' + siteId,
        title: 'Redact Now',
        allowMaximize: false,
        showClose: true
    };

    SP.UI.ModalDialog.showModalDialog(options);
}
// Gets the site data asynchronously and if successful displays the
// Watch folder configuration page in a SP modal dialog box
function showWatchFolderConfiguration()
{
    // Get the current context, web, site and folder
    this.context = new SP.ClientContext.get_current();
    this.listid = SP.ListOperation.Selection.getSelectedList();
    this.site = context.get_site();
    context.load(this.site, "Id");

    // Execute the asynch query to get the data from SP
    context.executeQueryAsync(Function.createDelegate(this, this.OnShowWatchSuccess),
        Function.createDelegate(this, this.OnFailure));
}

// Displays a notification box to the user as well as refreshing the current page after the dialog closes.
function onWatchSuccessCallback(result, target) {

    if (result == SP.UI.DialogResult.OK) {
        SP.UI.Notify.addNotification('Folder settings updated.');
    }
    SP.UI.ModalDialog.RefreshPage(SP.UI.DialogResult.OK);
}

// Called asynchronously, displays the SP modal dialog box with the
// Watch folder configuration page in a SP modal dialog box
function OnShowWatchSuccess(sender, args)
{
    var siteId = this.site.get_id();
    var folder = getFolderFromUrl(location.href);
    var options =
    {
        url: '/_layouts/Extract.SharePoint.Redaction/WatchFolderWithIdShield.aspx?'
            + 'listidvalue=' + listid + '&folder=' + folder + '&siteid=' + siteId,
        title: 'Process Current Folder',
        allowMaximize: false,
        showClose: true,
        dialogReturnValueCallback: onWatchSuccessCallback
    };

    SP.UI.ModalDialog.showModalDialog(options);
}

// Gets the site data asynchronously and if successful displays the
// Unwatch folder configuration page in a SP modal dialog box
function unwatchFolder()
{
    // Get the current context, web, site and folder
    this.context = new SP.ClientContext.get_current();
    this.site = context.get_site();
    context.load(this.site, "Id");

    // Execute the asynch query to get the data from SP
    context.executeQueryAsync(Function.createDelegate(this, this.OnShowUnwatchSuccess),
        Function.createDelegate(this, this.OnFailure));
}

// Called asynchronously, displays the SP modal dialog box with the
// Unwatch folder configuration page in a SP modal dialog box
function OnShowUnwatchSuccess(sender, args)
{
    var siteId = this.site.get_id();
    var folder = getFolderFromUrl(location.href);
    var options =
    {
        url: '/_layouts/Extract.SharePoint.Redaction/RemoveWatchFolderWithIdShield.aspx?'
            + 'folder=' + folder + '&siteid=' + siteId,
        title: 'Unwatch Current Folder',
        allowMaximize: false,
        showClose: true
    };

    SP.UI.ModalDialog.showModalDialog(options);
}

// Function to set for redaction
function ProcessSelectedForRedaction() {
    processSelected("IDShieldStatus");
}

// Used to initialize verification
function showVerifyNowHelper() {

    // Get the current context, web and site
    singleFileContext();

    // Execute the asynch query to get the data from SP
    context.executeQueryAsync(Function.createDelegate(this, this.OnShowVerifyNowHelperSuccess),
        Function.createDelegate(this, this.OnFailure));
}

// Called asynchronously, displays the SP modal dialog box with the
// Redact now helper page in a SP modal dialog box
function OnShowVerifyNowHelperSuccess(sender, args) {
    var siteId = this.site.get_id();
    var fileid = this.listItem.get_item('UniqueId');
    var options =
    {
        url: '/_layouts/Extract.SharePoint.Redaction/VerifyNow.aspx?'
            + 'listidvalue=' + listid + '&fileid=' + fileid + '&siteid=' + siteId,
        title: 'Verify',
        allowMaximize: false,
        showClose: true
    };

    SP.UI.ModalDialog.showModalDialog(options);
}