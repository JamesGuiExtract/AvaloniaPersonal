<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls"
    Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WatchFolderWithIdShield.aspx.cs"
    Inherits="Extract.SharePoint.Redaction.Layouts.WatchFolderWithIdShield" DynamicMasterPageFile="~masterurl/default.master" %>

<script runat="server">
    public void CheckAddedModified(object source, ServerValidateEventArgs e)
    {
        e.IsValid = checkAdded.Checked || checkModified.Checked;
    }

    public void CheckOutputOptions(object source, ServerValidateEventArgs e)
    {
        if (radioCustomOutput.Checked)
        {
            e.IsValid = !string.IsNullOrEmpty(textCustomOut.Text);
            return;
        }

        string text = string.Empty;
        if (radioSubfolder.Checked)
        {
            text = textSubfolder.Text;
        }
        else if (radioSameFolder.Checked)
        {
            text = textPreSuffix.Text;
        }
        else if (radioParallel.Checked)
        {
            text = textParallel.Text;
        }

        e.IsValid = !string.IsNullOrEmpty(text) && text.IndexOfAny(new char[] { '/', '\\' }) == -1;
    }
    
</script>
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
    <asp:HiddenField ID="hiddenOutputLocation" runat="server" />
    <asp:HiddenField ID="hiddenSiteLocation" runat="server" />
    <asp:Panel ID="panelCannotWatch" runat="server" Visible="false">
        <asp:Label ID="labelCannotWatch" runat="server" ForeColor="Black" />
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
                    <asp:CheckBox ID="checkRecursively" runat="server" ForeColor="Black"
                        Text="Recursively process files from any sub folders"
                        AutoPostBack="false" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:CheckBox ID="checkAdded" runat="server" ForeColor="Black" Text="Process any files that are added"
                        AutoPostBack="false" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:CheckBox ID="checkModified" runat="server" ForeColor="Black" Text="Process any files that are modified"
                        AutoPostBack="false" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:CustomValidator runat="server" ControlToValidate="textCurrentFolderName"
                        ErrorMessage="At least one of added or modified must be checked."
                        OnServerValidate="CheckAddedModified" />
                </td>
            </tr>
            <tr>
                <td>
                </td>
            </tr>
        </table>
    </asp:Panel>
    <br />
    <asp:Panel ID="panelOutputSettings" runat="server" GroupingText="Output settings"
        ForeColor="Black">
        <table>
            <tr>
                <td>
                    <asp:RadioButton ID="radioParallel" runat="server" GroupName="RadioOutputPath" Text="Use same filename in a parallel folder "
                        ForeColor="Black" OnCheckedChanged="RadioButtonChanged" AutoPostBack="true" />
                    <asp:DropDownList ID="dropFolderName" runat="server" ForeColor="Black" OnSelectedIndexChanged="PrefixSuffixFolderDropDownChanged"
                        AutoPostBack="true">
                        <asp:ListItem>Prefixed</asp:ListItem>
                        <asp:ListItem>Suffixed</asp:ListItem>
                    </asp:DropDownList>
                    <asp:TextBox ID="textParallel" runat="server" Width="125"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:RadioButton ID="radioSubfolder" runat="server" GroupName="RadioOutputPath" Text="Use sub folder called "
                        ForeColor="Black" OnCheckedChanged="RadioButtonChanged" AutoPostBack="true" />
                    <asp:TextBox ID="textSubfolder" runat="server" Width="125"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:RadioButton ID="radioSameFolder" runat="server" GroupName="RadioOutputPath"
                        Text="Use same folder, but " ForeColor="Black" OnCheckedChanged="RadioButtonChanged"
                        AutoPostBack="true" />
                    <asp:DropDownList ID="dropFileName" runat="server" ForeColor="Black" OnSelectedIndexChanged="PrefixSuffixDropDownChanged"
                        AutoPostBack="true">
                        <asp:ListItem>Prefix</asp:ListItem>
                        <asp:ListItem>Suffix</asp:ListItem>
                    </asp:DropDownList>
                    <asp:Label ID="Label2" runat="server" Text=" filename with " ForeColor="Black" />
                    <asp:TextBox ID="textPreSuffix" runat="server" Width="125"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:RadioButton ID="radioCustomOutput" runat="server" GroupName="RadioOutputPath"
                        Text="Use custom output location" ForeColor="Black" OnCheckedChanged="RadioButtonChanged"
                        AutoPostBack="true" />
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:TextBox ID="textCustomOut" runat="server" Width="415" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:CustomValidator runat="server" ControlToValidate="textCurrentFolderName" ErrorMessage="Output setting must not be blank or contain either '/' or '\' (unless using custom location)"
                        OnServerValidate="CheckOutputOptions" />
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
<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea"
    runat="server">
    My Application Page
</asp:Content>
