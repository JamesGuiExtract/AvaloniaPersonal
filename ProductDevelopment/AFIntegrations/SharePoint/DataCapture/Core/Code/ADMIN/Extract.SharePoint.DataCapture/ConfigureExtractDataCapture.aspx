<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConfigureExtractDataCapture.aspx.cs" Inherits="Extract.SharePoint.DataCapture.Administration.Layouts.ConfigureExtractDataCapture" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
<asp:Panel id="panelLogo" runat="server" forecolor="Black" groupingtext="Extract Data Capture: Unknown" >
    <asp:Image runat="server" imageurl="/_layouts/images/Extract.SharePoint.DataCapture/ExtractDataCaptureLogo.jpg"
         imagealign="middle" />
</asp:Panel>
<asp:Panel id="panelSettings" runat="server" groupingtext="Extract Data Capture settings" forecolor="Black" >
<asp:HiddenField id="hiddenSerializedSettings" runat="server" />
<table>
    <tr>
        <td>
            <asp:Label id="Label1" runat="server" forecolor="Black"
                text="SharePoint working folder" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:TextBox id="textFolder" runat="server" width="450" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:Label id="labelExceptionServer" runat="server" forecolor="Black"
                text="IP address for server running Extract exception service" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:TextBox id="textExceptionIpAddress" runat="server" width="450" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:RegularExpressionValidator runat="server" controltovalidate="textExceptionIpAddress"
                validationexpression="^[\s\S]{0}$|^(\d+\.){3}\d+$"
                errormessage="Must be blank or valid ip address specification." />
        </td>
    </tr>
    <tr>
        <td>
            <asp:Label id="labelTimeToWait" runat="server" forecolor="Black"
                text="Delay in minutes before exporting 'To be queued' files." />
        </td>
    </tr>
    <tr>
        <td>
            <asp:TextBox id="textTimeToWait" runat="server" width="20" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:RangeValidator runat="server" controltovalidate="textTimeToWait"
                minimumvalue="1" maximumvalue="60" type="Integer"
                errormessage="Value must be an between 1 and 60." />
        </td>
    </tr>
    <tr>
        <td>
            <asp:Label id="Label42" runat="server" forecolor="Black"
                text="Random folder name length (0 for no random folder)" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:DropDownList id="dropRandomFolderLength" runat="server" forecolor="Black">
                <asp:ListItem>0</asp:ListItem> <%--Indicates no random folder--%>
                <asp:ListItem>1</asp:ListItem>
                <asp:ListItem>2</asp:ListItem>
                <asp:ListItem>3</asp:ListItem>
            </asp:DropDownList>
        </td>
    </tr>
    <tr>
        <td align="right">
            <asp:Button id="buttonSave" runat="server" text="Save" onclick="HandleSaveButtonClick" />
        </td>
    </tr>
</table>
</asp:Panel>
<br />
<asp:Panel id="panelWatchedFolders" runat="server" groupingtext="Extract Data Capture folder settings"
    forecolor="Black">
    <table>
        <tr>
            <td>
                <asp:DropDownList id="dropWatchedSites" runat="server" autopostback="true"
                    onselectedindexchanged="HandleWatchedSitesChanged" width="450"/>
            </td>
        </tr>
        <tr>
            <td>
                <asp:ListBox id="listWatchedFolders" runat="server" width="450" 
                    autopostback="true" onselectedindexchanged="HandleWatchListSelectionChanged" /> 
            </td>
        </tr>
        <tr>
            <td>
                <asp:TextBox id="textSiteSettingsList" runat="server" autopostback="false" width="450" />
            </td>
        </tr>
        <tr align="right">
            <td>
                <asp:Button id="buttonRefreshSettings" runat="server" text="Refresh"
                    onclick="HandleRefreshSettings" />
                <asp:Button id="buttonRemoveWatching" runat="server" text="Remove Watching"
                    onclick="HandleRemoveWatching" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:TextBox id="textWatchFolderSettings" runat="server" textmode="multiline"
                    width="450" rows="10" readonly="true" />
            </td>
        </tr>
    </table>
</asp:Panel>
<br />

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Data Capture Configuration Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Data Capture Configuration Page
</asp:Content>
