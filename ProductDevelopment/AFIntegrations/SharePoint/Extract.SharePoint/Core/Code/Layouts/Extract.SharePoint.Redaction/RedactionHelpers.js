
function showConfigureIdShield()
{
    var options =
    {
        url: '/_layouts/Extract.SharePoint.Redaction/ConfigureIdShieldSettings.aspx',
        title: 'Configure ID Shield',
        allowMaximize: false,
        showClose: true,
        dialogReturnValueCallback: demoCallback
    };

    SP.UI.ModalDialog.showModalDialog(options);
}

function showWatchFolderConfiguration()
{
    var folder = getFolderFromUrl(location.href);

    var options =
    {
        url: '/_layouts/Extract.SharePoint.Redaction/WatchFolderWithIdShield.aspx?'
            + 'folder=' + folder,
        title: 'Watch Current Folder',
        allowMaximize: false,
        showClose: true,
        dialogReturnValueCallback: demoCallback
    };

    SP.UI.ModalDialog.showModalDialog(options);
}

function unwatchFolder()
{
    var folder = getFolderFromUrl(location.href);

    var options =
    {
        url: '/_layouts/Extract.SharePoint.Redaction/RemoveWatchFolderWithIdShield.aspx?'
            + 'folder=' + folder,
        title: 'Unwatch Current Folder',
        allowMaximize: false,
        showClose: true,
        dialogReturnValueCallback: demoCallback
    };

    SP.UI.ModalDialog.showModalDialog(options);
}

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
            folder = temp.substr(index + 1);
        }
    }

    return folder;
}

function demoCallback(dialogResult, returnValue)
{
}