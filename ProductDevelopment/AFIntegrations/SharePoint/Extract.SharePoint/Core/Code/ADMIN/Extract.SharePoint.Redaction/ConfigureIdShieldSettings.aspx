<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.SharePoint.ApplicationPages.Administration, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConfigureIdShieldSettings.aspx.cs" Inherits="Extract.SharePoint.Redaction.Layouts.ConfigureIdShieldSettings" MasterPageFile="~/_admin/admin.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
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
    <tr>
        <td align="right">
            <asp:Button ID="buttonSave" runat="server" Text="Save" OnClick="HandleSaveButtonClick" />
        </td>
    </tr>
</table>
</asp:Panel>
<br />
<asp:Panel ID="panelWatchedFolders" runat="server" GroupingText="ID Shield watched folders"
    ForeColor="Black">
    <table>
        <tr>
            <td>
                <asp:DropDownList ID="dropWatchedSites" runat="server" AutoPostBack="true"
                    OnSelectedIndexChanged="HandleWatchedSitesChanged" Width="450"/>
            </td>
        </tr>
        <tr>
            <td>
                <asp:ListBox ID="listWatchedFolders" runat="server" Width="450" 
                    AutoPostBack="true" OnSelectedIndexChanged="HandleWatchListSelectionChanged" /> 
            </td>
        </tr>
        <tr>
            <td align="right">
                <asp:Button ID="buttonRemoveWatching" runat="server" Text="Remove Watching"
                    OnClick="HandleRemoveWatching" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:TextBox ID="textWatchFolderSettings" runat="server" TextMode="multiline"
                    Width="450" rows="6" ReadOnly="true" />
            </td>
        </tr>
    </table>
</asp:Panel>
<br />
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
ID Shield Configuration Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
ID Shield Configuration
</asp:Content>
