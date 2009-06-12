Attribute VB_Name = "modCommaDelimitedFeatureAttributeDataInterpreter"
Public Sub testCommaDelimitedFeatureAttributeDataInterpreter()

    On Error GoTo HandleError

    ' create random feature
    Dim pFeature As IFeature
    Set pFeature = createRandomFeature()
    
    Dim pDataInterpreter As IFeatureAttributeDataInterpreter
    Set pDataInterpreter = New CommaDelimitedFeatureAttributeDataInterpreter
    Dim strData As String
    strData = pDataInterpreter.getAttributeDataFromFeature(pFeature)
    Dim pFeature2 As IFeature
    Set pFeature2 = pDataInterpreter.getFeatureFromAttributeData(strData)
    
    ' test to see if the two features are equal
    Dim strTestName As String
    strTestName = "CommaDelimitedFeatureAttributeDataInterpreter"
    If (pFeature.valueIsEqualTo(pFeature2)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01626"
    End If
    Exit Sub
    
HandleError:
    HandleError
End Sub

