<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerifyNow.aspx.cs" Inherits="Extract.SharePoint.Redaction.Layouts.Extract.SharePoint.Redaction.VerifyNow" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

<script type="text/javascript">
    function HandleOKClicked() {
        SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.cancel,
        'Cancel clicked');
    }
</script>

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:HiddenField ID="hiddenSiteId" runat="server" />
    <asp:HiddenField ID="hiddenListId" runat="server" />
    <asp:HiddenField ID="hiddenFileId" runat="server" />
    <asp:HiddenField ID="hiddenLocalMachineIp" runat="server" />

    <asp:Panel ID="panelMessage" runat="server" Visible="false">
        <asp:Label ID="labelMessage" runat="server" ForeColor="Black" />
    </asp:Panel>
    <asp:Panel ID="panelButtons" runat="server" HorizontalAlign="Right" Visible="false">
        <asp:Button ID="buttonOk" runat="server" Text="OK" OnClientClick="HandleOKClicked()"/>
    </asp:Panel>
    <asp:Image ID="imageIdShield" runat="server" ImageUrl="/_layouts/images/Extract.SharePoint.Redaction/IdShieldLogo.jpg" ImageAlign="Middle" />
    <asp:Label ID="Label1" runat="server" Text="Please wait while verification is loaded for the selected file..." />
    <br />
    <asp:Label ID="ErrorLabel" runat="server" Text="" Visible="false" Enabled="false" ForeColor="Red" />
    <asp:Timer ID="timerClose" runat="server" Interval="500" OnTick="HandleTimerTick" Enabled="false" />

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Verify Current Selection
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Verify Current Selection
</asp:Content>
