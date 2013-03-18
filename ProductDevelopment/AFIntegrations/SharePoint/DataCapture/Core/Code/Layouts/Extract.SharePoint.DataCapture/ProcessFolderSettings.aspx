<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProcessFolderSettings.aspx.cs" Inherits="Extract.SharePoint.DataCapture.Layouts.ProcessFolderSettings" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <script type="text/javascript">
        function HandleCancelClicked()
        {
            SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.cancel,
        'Cancel clicked');
        }

</script>

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:HiddenField ID="hiddenSiteId" runat="server" />
    <asp:HiddenField ID="hiddenFolderId" runat="server" />
    <asp:HiddenField ID="hiddenListId" runat="server" />
    <asp:HiddenField ID="hiddenListName" runat="server" />
    <asp:Panel ID="panelMessage" runat="server" Visible="false">
        <asp:Label ID="labelMessage" runat="server" ForeColor="Black" />
    </asp:Panel>
    <asp:Panel ID="panelFileSpecification" runat="server" GroupingText="Input file specification"
        ForeColor="Black">
        <table>
            <tr>
                <td>
                    <asp:Label ID="labelFolderName" runat="server" Text="Current folder:" ForeColor="Black" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="textCurrentFolderName" runat="server" ReadOnly="true" Width="450" />
                </td>
            </tr>
            <tr>
                <td>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="labelFileExtension" runat="server" Text="File extension specification"
                        ForeColor="Black" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="textFileExtension" runat="server" Width="450" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="textFileExtension"
                        ErrorMessage="File extension is required." />
                </td>
            </tr>
            <tr>
                <td>
                </td>
            </tr>
        </table>
    </asp:Panel>
    <br />
    <asp:Panel ID="panelFolderSettings" runat="server" GroupingText="Folder settings"
        ForeColor="Black">
        <table>
            <tr>
                <td>
                    <asp:CheckBox ID="checkReprocess" runat="server" ForeColor="Black"
                        Text="Reprocess previously processed files" AutoPostBack="false" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:CheckBox ID="checkRecursively" runat="server" ForeColor="Black"
                        Text="Recursively process files from any sub folders"
                        AutoPostBack="false" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:CheckBox ID="checkAdded" runat="server" ForeColor="Black" Text="Process any files that are added"
                        OnCheckedChanged="HandleCheckAddedChanged" AutoPostBack="true" />
                </td>
            </tr>
            <tr>
                <td>
                    <%-- Indent the checkbox --%>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                    <asp:CheckBox ID="checkDoNotProcessExisting" runat="server" ForeColor="Black"
                        Text="Do not process existing files" AutoPostBack="false" />
                </td>
            </tr>
            <tr>
                <td>
                </td>
            </tr>
        </table>
    </asp:Panel>
    <br />
    <asp:Panel ID="panelButtons" runat="server" HorizontalAlign="Right" >
        <asp:Button ID="buttonOk" runat="server" Text="OK" OnClick="HandleOkButtonClick" />
        <asp:Button ID="buttonCancel" runat="server" Text="Cancel" OnClientClick="HandleCancelClicked()" />
    </asp:Panel>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Application Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
My Application Page
</asp:Content>
