Attribute VB_Name = "Licensing"
Option Explicit
' Defines License Manager object
Private m_pLM As UCLID_COMLMLib.UCLIDComponentLM

Public Sub InitLM()
    On Error GoTo errHandler
    ' Create License Manager object
    If (m_pLM Is Nothing) Then
        Set m_pLM = New UCLID_COMLMLib.UCLIDComponentLM
    End If
    Exit Sub
    
errHandler:
    DisplayUCLIDException "ELI-InitLM"
End Sub


Public Sub InitInputFunnelLicense()
    On Error GoTo errHandler
    
    InitLM
    
    ' Initialize licensing from specified LIC file
    ' NOTE: Passwords are specific to the individual LIC file
    m_pLM.InitializeFromFile "LicenseFile.lic", 1, 2, 3, 4
        
    ' Turn off lock constraints, if appropriate
    ' NOTE: Password is also specific to the individual LIC file
    ' m_pLM.IgnoreLockConstraints 1234
    Exit Sub
    
errHandler:
    DisplayUCLIDException "ELI-Licensing"
End Sub

Public Function ComponentIsLicensed(ID As Long) As Boolean
    On Error GoTo errHandler
    
    InitLM

    ComponentIsLicensed = m_pLM.IsLicensed(ID)
    
    Exit Function
    
errHandler:
    DisplayUCLIDException "ELI-ComponentIsLicensed"
End Function

