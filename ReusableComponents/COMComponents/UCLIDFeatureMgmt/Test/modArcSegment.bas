Attribute VB_Name = "modArcSegment"
Option Explicit
Public Sub testArcSegment()

    testSetGetParameters
    testGetSegmentType
    testSetDefaultParamsFromCoords
    testGetCoordsFromParams
    
End Sub

Private Sub testGetSegmentType()
    
    ' allocate random arc segment
    Dim pMidPoint As ICartographicPoint
    Dim pEndPoint As ICartographicPoint
    Dim pParams As IIUnknownVector
    Dim pArc As IArcSegment
    Set pArc = createRandomArcSegment(pParams, pMidPoint, pEndPoint)
    
    ' get the segment interface
    Dim pSegment As ISegment
    Set pSegment = pArc
    
    Dim strTestName As String
    strTestName = "ArcSegment/ISegment.getSegmentType()"
    If (pSegment.getSegmentType <> kArc) Then
        frmProgress.recordFailedTest strTestName, "ELI01547"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
End Sub


Private Sub testSetDefaultParamsFromCoords()
    
    ' allocate random arc segment
    Dim pMidPoint As ICartographicPoint
    Dim pEndPoint As ICartographicPoint
    Dim pParams As IIUnknownVector
    Dim pArc As IArcSegment
    Set pArc = createRandomArcSegment(pParams, pMidPoint, pEndPoint)
    
    Dim pStartPoint As ICartographicPoint
    Set pStartPoint = New CartographicPoint
    pStartPoint.dX = 0
    pStartPoint.dY = 0
    pArc.setDefaultParamsFromCoords pStartPoint, pMidPoint, pEndPoint
    
    ' get the parameters from the arc, and ensure that they
    ' are what they should be
    Dim pRetrievedParams As IIUnknownVector
    Set pRetrievedParams = pArc.getParameters
    
    ' test to see if the two vectors are of the same size
    Dim strTestName As String
    strTestName = "ArcSegment.setDefaultParamsFromCoords() - 1"
    If (pRetrievedParams.Size() <> pParams.Size()) Then
        frmProgress.recordFailedTest strTestName, "ELI01592"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    ' ensure that the contents of the two vectors are the same
    Dim i As Integer
    For i = 0 To pParams.Size() - 1
        Dim pExpectedParam As IParameterTypeValuePair
        Dim pActualParam As IParameterTypeValuePair
        Set pExpectedParam = pParams.at(i)
        Set pActualParam = pRetrievedParams.at(i)
        
        strTestName = "ArcSegment.setDefaultParamsFromCoords() - 2"
        If (pExpectedParam.valueIsEqualTo(pActualParam)) Then
            frmProgress.recordPassedTest strTestName
        Else
            frmProgress.recordFailedTest strTestName, "ELI01593"
        End If
    Next i
    
End Sub


Private Sub testSetGetParameters()

    ' allocate random arc segment
    Dim pMidPoint As ICartographicPoint
    Dim pEndPoint As ICartographicPoint
    Dim pParams As IIUnknownVector
    Dim pArc As IArcSegment
    Set pArc = createRandomArcSegment(pParams, pMidPoint, pEndPoint)
    
    
    ' get the parameters from the arc
    Dim pRetrievedParams As IIUnknownVector
    Set pRetrievedParams = pArc.getParameters
   
    ' check to see that the number of parameters is valid
    Dim strTestName As String
    strTestName = "ArcSegment.getParameters().size()"
    If (pRetrievedParams.Size <> pParams.Size) Then
        frmProgress.recordFailedTest strTestName, "ELI01544"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    ' check to see if each of the parameters are valid
    Dim i As Integer
    For i = 0 To pParams.Size - 1
        'get the expected parameter and the retrieved parameter
        Dim pExpectedParam As IParameterTypeValuePair
        Set pExpectedParam = pParams.at(i)
        Dim pRetrievedParam As IParameterTypeValuePair
        Set pRetrievedParam = pRetrievedParams.at(i)
        
        'compare the parameters
        strTestName = "ArcSegment.getParameters().at(" & i & ")"
        If (pExpectedParam.valueIsEqualTo(pRetrievedParam)) Then
            frmProgress.recordPassedTest strTestName
        Else
            frmProgress.recordFailedTest strTestName, "ELI01570"
        End If
    Next i
    
    ' release the variant collection objects
    Set pParams = Nothing
    Set pRetrievedParams = Nothing
    
    ' release the arc object
    Set pArc = Nothing
End Sub

Private Sub testGetCoordsFromParams()

    ' allocate random arc segment
    Dim pMidPoint As ICartographicPoint
    Dim pEndPoint As ICartographicPoint
    Dim pParams As IIUnknownVector
    Dim pArc As IArcSegment
    Set pArc = createRandomArcSegment(pParams, pMidPoint, pEndPoint)
    
    Dim pStartPoint As ICartographicPoint
    Dim pRetrievedMidPoint As ICartographicPoint
    Dim pRetrievedEndPoint As ICartographicPoint
    Set pStartPoint = New CartographicPoint
    pStartPoint.dX = 0
    pStartPoint.dY = 0
    
    ' get the mid and end points from the arc
    pArc.getCoordsFromParams pStartPoint, pRetrievedMidPoint, pRetrievedEndPoint
    
    Dim strTestName As String
    strTestName = "ArcSegment.getCoordsFromParams() - 1"
    ' test to see if the retrieved midpoint is correct
    If (pRetrievedMidPoint.valueIsEqualTo(pMidPoint)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01594"
    End If
    
    ' test to see if the retrieved endpoint is correct
    strTestName = "ArcSegment.getCoordsFromParams() - 2"
    If (pRetrievedEndPoint.valueIsEqualTo(pEndPoint)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01595"
    End If
End Sub
