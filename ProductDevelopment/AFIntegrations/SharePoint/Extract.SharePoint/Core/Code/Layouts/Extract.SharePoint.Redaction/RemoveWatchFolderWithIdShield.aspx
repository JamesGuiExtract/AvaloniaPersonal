<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RemoveWatchFolderWithIdShield.aspx.cs" Inherits="Extract.SharePoint.Redaction.Layouts.RemoveWatchFolderWithIdShield" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

<script type="text/javascript">
    function HandleNoClicked()
    {
        SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.cancel,
        'Cancel clicked');
    }
</script>

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
<asp:Panel ID="panelTop" runat="server" GroupingText="Folder settings" ForeColor="Black">
<table>
    <tr>
        <td>
            <asp:Label ID="labelFolder" runat="server" ForeColor="Black" Text="Current folder:" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:TextBox ID="textFolder" runat="server" ReadOnly="true" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:Label ID="labelMessage" runat="server" ForeColor="Black" />
        </td>
    </tr>
    <tr><td /></tr>
</table>
</asp:Panel>
<br />
<asp:Panel ID="panelButtons" runat="server" HorizontalAlign="Right">
    <asp:Button ID="buttonYes" runat="server" Text="Yes" onclick="HandleYesButtonClick"/>
    <asp:Button ID="buttonNo" runat="server" Text="No" OnClientClick="HandleNoClicked()" />
</asp:Panel>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Application Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
My Application Page
</asp:Content>
