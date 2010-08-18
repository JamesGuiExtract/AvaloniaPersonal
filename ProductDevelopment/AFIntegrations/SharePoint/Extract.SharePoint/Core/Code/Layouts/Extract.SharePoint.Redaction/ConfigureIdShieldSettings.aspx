<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConfigureIdShieldSettings.aspx.cs" Inherits="Extract.SharePoint.Redaction.Layouts.ConfigureIdShieldSettings" DynamicMasterPageFile="~masterurl/default.master" %>

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
<asp:Panel ID="panelSettings" runat="server" GroupingText="ID Shield settings" ForeColor="Black">
<table>
    <tr>
        <td>
            <asp:Label ID="Label1" runat="server" ForeColor="Black"
                Text="SharePoint working folder" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:TextBox ID="textFolder" runat="server" Width="450" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:RequiredFieldValidator runat="server" ControlToValidate="textFolder"
                ErrorMessage="A folder must be specified." />
        </td>
    </tr>
    <tr>
        <td>
            <asp:Label ID="labelExceptionServer" runat="server" ForeColor="Black"
                Text="IP address for server running Extract exception service" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:TextBox ID="textExceptionIpAddress" runat="server" Width="450" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:RegularExpressionValidator runat="server" ControlToValidate="textExceptionIpAddress"
                ValidationExpression="^[\s\S]{0}$|^(\d+\.){3}\d+$"
                ErrorMessage="Must be blank or valid ip address specification." />
        </td>
    </tr>
</table>
</asp:Panel>
<br />
<asp:Panel ID="panelButtons" runat="server" HorizontalAlign="Right">
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
