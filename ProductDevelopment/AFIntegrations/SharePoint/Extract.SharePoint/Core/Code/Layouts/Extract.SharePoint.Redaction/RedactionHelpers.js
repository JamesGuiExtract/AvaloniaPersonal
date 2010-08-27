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
// Watch folder configuration page in a SP modal dialog box
function showWatchFolderConfiguration()
{
    // Get the current context, web, site and folder
    this.context = new SP.ClientContext.get_current();
    this.site = context.get_site();
    context.load(this.site);

    // Execute the asynch query to get the data from SP
    context.executeQueryAsync(Function.createDelegate(this, this.OnShowWatchSuccess),
        Function.createDelegate(this, this.OnFailure));
}

// Called asynchronously, displays the SP modal dialog box with the
// Watch folder configuration page in a SP modal dialog box
function OnShowWatchSuccess(sender, args)
{
    var siteRoot = this.site.get_serverRelativeUrl();
    var folderUrl = getFolderFromUrl(location.href);
    var options =
    {
        url: '/_layouts/Extract.SharePoint.Redaction/WatchFolderWithIdShield.aspx?'
            + 'folder=' + folderUrl + '&siteroot=' + siteRoot,
        title: 'Watch Current Folder',
        allowMaximize: false,
        showClose: true
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
    context.load(this.site);

    // Execute the asynch query to get the data from SP
    context.executeQueryAsync(Function.createDelegate(this, this.OnShowUnwatchSuccess),
        Function.createDelegate(this, this.OnFailure));
}

// Called asynchronously, displays the SP modal dialog box with the
// Unwatch folder configuration page in a SP modal dialog box
function OnShowUnwatchSuccess(sender, args)
{
    var siteRoot = this.site.get_serverRelativeUrl();
    var folderUrl = getFolderFromUrl(location.href);

    var options =
    {
        url: '/_layouts/Extract.SharePoint.Redaction/RemoveWatchFolderWithIdShield.aspx?'
            + 'folder=' + folderUrl + '&siteroot=' + siteRoot,
        title: 'Unwatch Current Folder',
        allowMaximize: false,
        showClose: true
    };

    SP.UI.ModalDialog.showModalDialog(options);
}


// Builds the server relative folder path for the folder url
function getFolderFromUrl(url)
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