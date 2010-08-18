<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WatchFolderWithIdShield.aspx.cs" Inherits="Extract.SharePoint.Redaction.Layouts.WatchFolderWithIdShield" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:HiddenField ID="hiddenLoaded" runat="server" />
    <asp:HiddenField ID="hiddenOutputLocation" runat="server" />
    <asp:Label ID="labelFolderName" runat="server" Text="Current folder:"></asp:Label>
    <br />
    <asp:TextBox ID="textCurrentFolderName" runat="server" ReadOnly="true"></asp:TextBox>
    <br />
    <br />
    <asp:Label ID="Label1" runat="server" Text="File extension specification"></asp:Label>
    <br />
    <asp:TextBox ID="textFileExtension" runat="server" Width="450" />
    <br />
    <br />
    <hr noshade="noshade" size="2" />
    <br />
    <asp:CheckBox ID="checkRecursively" runat="server"
        Text="Recursively process files from any sub folders"/>
    <br />
    <asp:CheckBox ID="checkAdded" runat="server" Text="Process any files that are added" />
    <br />
    <asp:CheckBox ID="checkModified" runat="server" Text="Process any files that are modified" />
    <br />
    <br />
    <hr noshade="noshade" size="2" />
    <br />
    <asp:RadioButton ID="radioParallel" runat="server" GroupName="RadioOutputPath"
        Text="Use same filename in a parallel folder "
        OnCheckedChanged="RadioButtonChanged"
        AutoPostBack="true"
        />
    <asp:DropDownList ID="dropFolderName" runat="server"
        OnSelectedIndexChanged="PrefixSuffixFolderDropDownChanged" AutoPostBack="true">
        <asp:ListItem>Prefixed</asp:ListItem>
        <asp:ListItem>Suffixed</asp:ListItem>
    </asp:DropDownList>
    <asp:TextBox ID="textParallel" runat="server" Width="125"></asp:TextBox>
    <br />
    <br />
    <asp:RadioButton ID="radioSubfolder" runat="server" GroupName="RadioOutputPath"
        Text="Use sub folder called "
        OnCheckedChanged="RadioButtonChanged"
        AutoPostBack="true"
    />
    <asp:TextBox ID="textSubfolder" runat="server" Width="125"></asp:TextBox>
    <br />
    <br />
    <asp:RadioButton ID="radioSameFolder" runat="server" GroupName="RadioOutputPath"
        Text="Use same folder, but "
        OnCheckedChanged="RadioButtonChanged"
        AutoPostBack="true"
    />
    <asp:DropDownList ID="dropFileName" runat="server"
        OnSelectedIndexChanged="PrefixSuffixDropDownChanged" AutoPostBack="true">
        <asp:ListItem>Prefix</asp:ListItem>
        <asp:ListItem>Suffix</asp:ListItem>
    </asp:DropDownList>
    <asp:Label ID="Label2" runat="server" Text=" filename with "></asp:Label>
    <asp:TextBox ID="textPreSuffix" runat="server" Width="125"></asp:TextBox>
    <br />
    <br />
    <asp:RadioButton ID="radioCustomOutput" runat="server" GroupName="RadioOutputPath" 
        Text="Use custom output location"
        OnCheckedChanged="RadioButtonChanged"
        AutoPostBack="true"
        />
    <br />
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:TextBox ID="textCustomOut" runat="server" Width="415" />
    <br />
    <br />
    <asp:Button ID="buttonOk" runat="server" Text="OK" onclick="HandleOkButtonClick"/>
    <asp:Button ID="buttonCancel" runat="server" Text="Cancel" OnClick="HandleCancelButtonClick" />
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Application Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
My Application Page
</asp:Content>
