Attribute VB_Name = "modParameterTypeValuepair"
Option Explicit
Public Sub testParameterTypeValuePair()
     
    ' create sample data
    Dim ptvp As IParameterTypeValuePair
    Dim eParamType As ECurveParameterType
    Dim strValue As String
    Set ptvp = createRandomParameterTypeValuePair(eParamType, strValue)
    
    ' test retrieval of the attributes
    Dim strTestName As String
    strTestName = "ParameterTypeValuePair.eParamType"
    If (ptvp.eParamType <> eParamType) Then
        frmProgress.recordFailedTest strTestName, "ELI01531"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    strTestName = "ParameterTypeValuePair.strValue"
    If (ptvp.strValue <> strValue) Then
        frmProgress.recordFailedTest strTestName, "ELI01532"
    Else
        frmProgress.recordPassedTest strTestName
    End If
   
End Sub
