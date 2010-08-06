<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WatchFolderWithIdShield.aspx.cs" Inherits="Extract.SharePoint.Layouts.Extract.SharePoint.Redaction.WatchFolderWithIdShield" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:HiddenField ID="hiddenLoaded" runat="server" />
    <asp:HiddenField ID="hiddenOutputLocation" runat="server" />
    <asp:Label ID="lblFolderName" runat="server" Text="Current folder:"></asp:Label>
    <br />
    <asp:TextBox ID="txtCurrentFolderName" runat="server" ReadOnly="true"></asp:TextBox>
    <br />
    <br />
    <asp:Label ID="Label1" runat="server" Text="File extension specification"></asp:Label>
    <br />
    <asp:TextBox ID="txtFileExtension" runat="server" Width="450" />
    <br />
    <br />
    <hr noshade="true" size="2" />
    <br />
    <asp:CheckBox ID="chkRecursively" runat="server"
        Text="Recursively process files from any sub folders"/>
    <br />
    <asp:CheckBox ID="chkAdded" runat="server" Text="Process any files that are added" />
    <br />
    <asp:CheckBox ID="chkModified" runat="server" Text="Process any files that are modified" />
    <br />
    <br />
    <hr noshade="true" size="2" />
    <br />
    <asp:RadioButton ID="radParallel" runat="server" GroupName="RadioOutputPath"
        Text="Use same filename in a parallel folder "
        OnCheckedChanged="RadioButtonChanged"
        AutoPostBack="true"
        />
    <asp:DropDownList ID="dropFolderName" runat="server"
        OnSelectedIndexChanged="PrefixSuffixFolderDropDownChanged" AutoPostBack="true">
        <asp:ListItem>Prefixed</asp:ListItem>
        <asp:ListItem>Suffixed</asp:ListItem>
    </asp:DropDownList>
    <asp:TextBox ID="txtParallel" runat="server" Width="125"></asp:TextBox>
    <br />
    <br />
    <asp:RadioButton ID="radSubFolder" runat="server" GroupName="RadioOutputPath"
        Text="Use parallel folder called "
        OnCheckedChanged="RadioButtonChanged"
        AutoPostBack="true"
    />
    <asp:TextBox ID="txtSubFolder" runat="server" Width="125"></asp:TextBox>
    <br />
    <br />
    <asp:RadioButton ID="radSameFolder" runat="server" GroupName="RadioOutputPath"
        Text="Use same folder, but "
        OnCheckedChanged="RadioButtonChanged"
        AutoPostBack="true"
    />
    <asp:DropDownList ID="dropFilename" runat="server"
        OnSelectedIndexChanged="PrefixSuffixDropdownChanged" AutoPostBack="true">
        <asp:ListItem>Prefix</asp:ListItem>
        <asp:ListItem>Suffix</asp:ListItem>
    </asp:DropDownList>
    <asp:Label ID="Label2" runat="server" Text=" filename with "></asp:Label>
    <asp:TextBox ID="txtPreSuffix" runat="server" Width="125"></asp:TextBox>
    <br />
    <br />
    <asp:RadioButton ID="radCustomOutput" runat="server" GroupName="RadioOutputPath" 
        Text="Use custom output location"
        OnCheckedChanged="RadioButtonChanged"
        AutoPostBack="true"
        />
    <br />
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:TextBox ID="txtCustomOut" runat="server" Width="415" />
    <br />
    <br />
    <asp:Button ID="btnOk" runat="server" Text="OK" onclick="btnOkClick"/>
    <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancelClick" />
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Application Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
My Application Page
</asp:Content>
