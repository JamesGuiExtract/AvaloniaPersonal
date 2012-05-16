VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "ArcGISDisplayAdapter"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
Option Explicit

Implements IDisplayAdapter
Implements IArcGISDependentComponent

Private m_bSupportsPartCreation As Long
Private m_bSupportsSketchCreation As Long
Private m_pApplication As esriFramework.IApplication
Private m_pEditor As esriEditor.IEditor
Private m_pLastTool As esriFramework.ICommandItem
'contains all segments within the current sketch
Private m_pCurrentSketch As esriGeometry.IGeometryCollection
'current part of the sketch
Private m_pPart As esriGeometry.ICurve
'if this is the start of a sketch
Private m_bStartSketch As Boolean
'if target layer is polygon layer
Private m_bPolygonSketch As Boolean
' If Ground-To-Grid is on
Private m_dDistanceFactor As Double
Private m_dAngleOffset As Double
' whether or not the ground to grid correction is checked in the editor option
Private m_bGroundToGridChecked As Boolean

' define a set of workspace type
Enum WorkspaceType
    enumNone = -1
    enumGeodatabase = 0
    enumCoverage = 1
    enumPersonalGeodatabase = 2
    enumShape = 3
End Enum

Private Sub Class_Initialize()
On Error GoTo ErrHandler
    'ArcMap supports both part and sketch creation
    m_bSupportsPartCreation = 1
    m_bSupportsSketchCreation = 1
    m_bStartSketch = True
Exit Sub
ErrHandler:
    handleError "ELI01723"
End Sub

Private Sub Class_Terminate()
On Error GoTo ErrHandler
    Set m_pPart = Nothing
    Set m_pCurrentSketch = Nothing
Exit Sub
ErrHandler:
    handleError "ELI01750"
End Sub

Private Sub IArcGISDependentComponent_setApplicationHook(ByRef hook As Object)
On Error GoTo ErrHandler
    Dim pId As New esriSystem.UID
    Set m_pApplication = hook
    'assign progId to the pId
    pId = "esriEditor.editor"
    'get the clsid
    Set m_pEditor = m_pApplication.FindExtensionByCLSID(pId)
    Set m_pLastTool = m_pApplication.CurrentTool
    
Exit Sub
ErrHandler:
    handleError "ELI01724"
End Sub

Private Function IDisplayAdapter_AddCurveSegment(ByVal ipIArcSegment As UCLID_FEATUREMGMTLib.IArcSegment) As String
On Error GoTo ErrHandler
    'check to see if this is the beginning of current sketch
    If m_bStartSketch Then
        init
    End If

    Dim pUCLIDStartPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
    Dim pUCLIDMidPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
    Dim pUCLIDEndPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
    Set pUCLIDStartPoint = New UCLID_MEASUREMENTSLib.CartographicPoint
    Set pUCLIDMidPoint = Nothing
    Set pUCLIDEndPoint = Nothing

    'get last point of from current part
    Dim dX As Double
    Dim dY As Double
    Dim dRes As Double
    dRes = IDisplayAdapter_GetLastPoint(dX, dY)
    If dRes = 1 Then
        pUCLIDStartPoint.InitPointInXY dX, dY
    Else 'throw exception
        Err.Raise Err.Number, "", "No start point available for this segment!"
    End If

    Dim ipConvertedArcSeg As UCLID_FEATUREMGMTLib.IArcSegment
    Set ipConvertedArcSeg = GroundToGridConversionCurve(ipIArcSegment)
    ' retrieve/calculate the mid and end point for the arc
    ipConvertedArcSeg.getCoordsFromParams pUCLIDStartPoint, pUCLIDMidPoint, pUCLIDEndPoint
    If Not pUCLIDMidPoint Is Nothing And Not pUCLIDEndPoint Is Nothing Then
        Dim pFromPnt As esriGeometry.IPoint
        Dim pMidPnt As esriGeometry.IPoint
        Dim pToPnt As esriGeometry.IPoint
        Set pFromPnt = New esriGeometry.Point
        Set pMidPnt = New esriGeometry.Point
        Set pToPnt = New esriGeometry.Point
        'construct three points of esri type
        pUCLIDStartPoint.GetPointInXY dX, dY
        pFromPnt.PutCoords dX, dY
        pUCLIDMidPoint.GetPointInXY dX, dY
        pMidPnt.PutCoords dX, dY
        pUCLIDEndPoint.GetPointInXY dX, dY
        pToPnt.PutCoords dX, dY

        'construct a circular arc
        Dim pCircArc As esriGeometry.ICircularArc
        Dim pConstCurve As esriGeometry.IConstructCircularArc

        Set pCircArc = New esriGeometry.CircularArc
        'QI pConstCurve to the circular arc that is going to be constructed
        Set pConstCurve = pCircArc
        'construct from three points
        pConstCurve.ConstructThreePoints pFromPnt, pMidPnt, pToPnt, False

        'add the arc to the current part
        addSegment pCircArc

        Set pFromPnt = Nothing
        Set pMidPnt = Nothing
        Set pToPnt = Nothing
    End If
    Set pUCLIDStartPoint = Nothing

Exit Function
ErrHandler:
    handleError "ELI01725"
End Function

Private Function IDisplayAdapter_AddLineSegment(ByVal ipILineSegment As UCLID_FEATUREMGMTLib.ILineSegment) As String
On Error GoTo ErrHandler
    'check to see if this is the beginning of current sketch
    If m_bStartSketch Then
        init
    End If

    Dim pUCLIDStartPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
    Dim pUCLIDEndPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
    Set pUCLIDStartPoint = New UCLID_MEASUREMENTSLib.CartographicPoint
    Set pUCLIDEndPoint = Nothing

    ' get last point of from current part
    Dim dX As Double
    Dim dY As Double
    Dim dRes As Double
    dRes = IDisplayAdapter_GetLastPoint(dX, dY)
    If dRes = 1 Then
        pUCLIDStartPoint.InitPointInXY dX, dY
    Else        'throw exception
        Err.Raise Err.Number, "", "No start point is available for this segment!"
    End If

    Dim ipConvertedLineSeg As UCLID_FEATUREMGMTLib.ILineSegment
    ' if the line segment is calculated from deflection/internal angle,
    ' then no ground-to-grid correction shall be applied
    If ipILineSegment.isFromDeflectionInternalAngle Then
        Set ipConvertedLineSeg = ipILineSegment
    Else
        Set ipConvertedLineSeg = GroundToGridConversionLine(ipILineSegment)
    End If
    ' retrieve/calculate end point from ILineSegment
    ipConvertedLineSeg.getCoordsFromParams pUCLIDStartPoint, pUCLIDEndPoint

    If Not pUCLIDEndPoint Is Nothing Then
        Dim pLine As esriGeometry.ILine
        Dim pFromPnt As esriGeometry.IPoint
        Dim pToPnt As esriGeometry.IPoint
        Set pLine = New esriGeometry.Line
        Set pFromPnt = New esriGeometry.Point
        Set pToPnt = New esriGeometry.Point

        pUCLIDStartPoint.GetPointInXY dX, dY
        'construct the from and to points out of X, Y coords
        pFromPnt.PutCoords dX, dY
        pUCLIDEndPoint.GetPointInXY dX, dY
        pToPnt.PutCoords dX, dY
        'create a line out of the from and to points
        pLine.PutCoords pFromPnt, pToPnt

        'add this line segment to the current part
        addSegment pLine

        Set pFromPnt = Nothing
        Set pToPnt = Nothing
    End If

    Set pUCLIDStartPoint = Nothing

Exit Function
ErrHandler:
    handleError "ELI01726"

End Function

Private Sub IDisplayAdapter_DeleteCurrentSketch()
On Error GoTo ErrHandler
    Dim pEditSketch As esriEditor.IEditSketch
    Dim pMxDoc As esriArcMapUI.IMxDocument
    Set pEditSketch = m_pEditor
    Set pMxDoc = m_pApplication.Document

    'remove current sketch from display
    Dim ipDeleteSketchCommand As esriFramework.ICommandItem
    Dim pCommandBars As esriFramework.ICommandBars
    Dim pUID As esriSystem.IUID
    Set pUID = New esriSystem.UID
    ' use esri's delete sketch command to fulfil the task since
    ' there's no specific method available for deleting a sketch
    pUID.Value = "esriEditor.DeleteSketchCommand"
    Set pCommandBars = m_pApplication.Document.CommandBars
    Set ipDeleteSketchCommand = pCommandBars.Find(pUID)
    If Not ipDeleteSketchCommand Is Nothing Then
        ipDeleteSketchCommand.Execute
    End If

    Set m_pPart = Nothing
    Set m_pCurrentSketch = Nothing
    m_bStartSketch = True

    pMxDoc.ActiveView.Refresh

Exit Sub
ErrHandler:
    handleError "ELI01727"
End Sub

Private Sub IDisplayAdapter_EraseLastSegment()
On Error GoTo ErrHandler
    'if the current sketch is still under construction
    If Not m_pCurrentSketch Is Nothing Then
        'remove the last segment
        Dim pMxDoc As esriArcMapUI.IMxDocument
        Dim pEditSketch As esriEditor.IEditSketch
        Dim pSegCol As esriGeometry.ISegmentCollection

        Set pEditSketch = m_pEditor
        Set pMxDoc = m_pApplication.Document
        Set m_pCurrentSketch = pEditSketch.Geometry

        'if current sketch has at least one part
        If Not m_pCurrentSketch.GeometryCount = 0 Then
            'QI m_pPart to the last part (i.e. the current part) of the current sketch
            'so that we can continue working on the current part without losing any previous information
            Set m_pPart = m_pCurrentSketch.Geometry(m_pCurrentSketch.GeometryCount - 1)
        End If

        'QI pSegCol to the current part of the sketch
        Set pSegCol = m_pPart
        Dim iSegmentCount As Long
        iSegmentCount = pSegCol.SegmentCount
        
        ' get the starting point for the current sketch part
        Dim pPointCol As esriGeometry.IPointCollection
        Dim pStartPoint As esriGeometry.IPoint
        Set pPointCol = m_pPart
        Set pStartPoint = New esriGeometry.Point
        pStartPoint.PutCoords pPointCol.Point(0).X, pPointCol.Point(0).Y
        
        'last segment of the sketch
        If iSegmentCount > 0 Then
            'get the operation stack
            Dim pOperationStack As esriSystemUI.IOperationStack
            Set pOperationStack = pMxDoc.OperationStack

            Dim iNumOfSegmentsToErase As Long
            If m_bPolygonSketch Then
                ' remove last two segments if it's polygon layer
                iNumOfSegmentsToErase = 2
            Else
                iNumOfSegmentsToErase = 1
            End If
            'remove last segment
            pSegCol.RemoveSegments iSegmentCount - iNumOfSegmentsToErase, iNumOfSegmentsToErase, False
            
            ' if currently no segment left in this part, then add the starting
            ' point for this part.
            ' Note that the starting point is removed in addSegment
            If pSegCol.SegmentCount = 0 Then
                pPointCol.AddPoint pStartPoint
            End If
            
            If m_bPolygonSketch Then
                Dim pRing As esriGeometry.IRing
                Set pRing = pSegCol
                ' when Close is called upon the ring, a segment is added
                ' to the ring from FromPoint to the ToPoint of the ring
                pRing.Close
            End If

            'remove this segment related operation from the operation stack
            If pOperationStack.Count > 0 Then
                pOperationStack.Remove pOperationStack.Count - 1
            End If

            pEditSketch.RefreshSketch
            'refresh ArcMap display window
            pMxDoc.ActiveView.Refresh
        End If

    End If
Exit Sub
ErrHandler:
    handleError "ELI01728"
End Sub

Private Sub IDisplayAdapter_FinishCurrentPart()
On Error GoTo ErrHandler
    Dim pEditSketch As esriEditor.IEditSketch
    Set pEditSketch = m_pEditor

    pEditSketch.RefreshSketch
    pEditSketch.FinishSketchPart

    Set m_pPart = Nothing
    If m_bPolygonSketch Then
        Set m_pPart = New esriGeometry.Ring
    Else
        Set m_pPart = New esriGeometry.Path
    End If

Exit Sub
ErrHandler:
    handleError "ELI01729"
End Sub

Private Function IDisplayAdapter_FinishCurrentSketch() As String
On Error GoTo ErrHandler
    Dim pEditSketch As esriEditor.IEditSketch
    Set pEditSketch = m_pEditor

    pEditSketch.FinishSketch

    Set m_pPart = Nothing
    Set m_pCurrentSketch = Nothing
    m_bStartSketch = True

    'retrieve FeatureId for the current sketch
    Dim pMxDoc As esriArcMapUI.IMxDocument
    Dim pEnumFeature As esriGeoDatabase.IEnumFeature
    Dim pFeature As esriGeoDatabase.IFeature
    Set pMxDoc = m_pApplication.Document
    ' refresh the view
    pMxDoc.ActiveView.Refresh
    
    Set pEnumFeature = pMxDoc.FocusMap.FeatureSelection
    'set pFeature equal to the first element in the feature selection set
    pEnumFeature.Reset
    Set pFeature = pEnumFeature.Next
    If Not pFeature Is Nothing Then
        If pFeature.HasOID Then
            IDisplayAdapter_FinishCurrentSketch = LongToString(pFeature.OID)
        End If
    Else
        IDisplayAdapter_FinishCurrentSketch = ""
    End If
    
    ' inverse each segment if current task is "Create 2-Point Line Features"
    Dim ipEditTask As esriEditor.IEditTask
    Set ipEditTask = m_pEditor.CurrentTask
    If Not ipEditTask Is Nothing Then
        If ipEditTask.Name = "Create 2-Point Line Features" Then
            ' find the Inverse command and execute it
            Dim ipInverseCommand As esriFramework.ICommandItem
            Dim pCommandBars As esriFramework.ICommandBars
            Dim pUID As esriSystem.IUID
            Set pUID = New esriSystem.UID
            ' use esri's delete sketch command to fulfil the task since
            ' there's no specific method available for deleting a sketch
            pUID.Value = "esriEditor.InverseCommand"
            Set pCommandBars = m_pApplication.Document.CommandBars
            Set ipInverseCommand = pCommandBars.Find(pUID)
            If Not ipInverseCommand Is Nothing Then
                ipInverseCommand.Execute
            End If
        End If
    End If
Exit Function
ErrHandler:
    handleError "ELI01730"
End Function

Private Function IDisplayAdapter_GetCurrentDistanceUnit() As UCLID_DISTANCECONVERTERLib.EDistanceUnitType
On Error GoTo ErrHandler
    Dim pMxDocument As esriArcMapUI.IMxDocument
    Dim pMap As esriCarto.IMap
    Set pMxDocument = m_pApplication.Document
    Set pMap = pMxDocument.FocusMap

    Dim eDistanceUnit As UCLID_DISTANCECONVERTERLib.EDistanceUnitType
    eDistanceUnit = kUnknownUnit

    If pMap.MapUnits = esriSystem.esriInches Then
        eDistanceUnit = kInches
    ElseIf pMap.MapUnits = esriSystem.esriFeet Then
        eDistanceUnit = kFeet
    ElseIf pMap.MapUnits = esriSystem.esriYards Then
        eDistanceUnit = kYards
    ElseIf pMap.MapUnits = esriSystem.esriMiles Then
        eDistanceUnit = kMiles
    ElseIf pMap.MapUnits = esriSystem.esriCentimeters Then
        eDistanceUnit = kCentimeters
    ElseIf pMap.MapUnits = esriSystem.esriMeters Then
        eDistanceUnit = kMeters
    ElseIf pMap.MapUnits = esriSystem.esriKilometers Then
        eDistanceUnit = kKilometers
    End If

    IDisplayAdapter_GetCurrentDistanceUnit = eDistanceUnit
   
Exit Function
ErrHandler:
    handleError "ELI02969"
End Function

Private Function IDisplayAdapter_GetFeatureGeometry(ByVal strFeatureID As String) As UCLID_FEATUREMGMTLib.IFeature
On Error GoTo ErrHandler
'TODO: what to do with strFeatureID? Now we only select one feature at a time and
'traverse the feature to get each segment info, and then store them in uclid IFeature.
'strFeatureID is not used here so far since the feature id is only unique at the layer level,
'not the whole map level.
    Dim pMxDoc As esriArcMapUI.IMxDocument
    Set pMxDoc = m_pApplication.Document
    Set IDisplayAdapter_GetFeatureGeometry = Nothing
    Dim pGeometry As esriGeometry.IGeometry
    Dim ipIFeature As UCLID_FEATUREMGMTLib.IFeature
    Set ipIFeature = New UCLID_FEATUREMGMTLib.Feature
    
    ' if the caller is asking for current sketch
    Dim pEditSketch As esriEditor.IEditSketch
    Set pEditSketch = m_pEditor

    ' ***** IMPORTANT!!! *******
    ' For each sketch that is about to be finished, it will be more accurate to
    ' obtain the current sketch info than to obtain info from the feature that
    ' will be created from the current sketch.
    ' This will preserve as much original measurement as we can for each feature.
    ' Therefore, we will have the "conventional" that if the strFeatureID equals "Sketch",
    ' the current geometry of the current sketch shall be read
    ' **************************
    If strFeatureID = "Sketch" And Not pEditSketch Is Nothing Then
        If Not pEditSketch.Geometry.IsEmpty Then
            Set pGeometry = pEditSketch.Geometry
        End If
    'only get the feature that is currently selected
    ElseIf pMxDoc.FocusMap.SelectionCount = 1 Then
        Dim pEnumFeature As esriGeoDatabase.IEnumFeature
        Dim pFeature As esriGeoDatabase.IFeature
        Set pEnumFeature = pMxDoc.FocusMap.FeatureSelection
        ' get the selected feature
        Set pFeature = pEnumFeature.Next
        
        Set pGeometry = pFeature.Shape
    End If

    If pGeometry Is Nothing Then
        Exit Function
    End If
    
    If pGeometry.IsEmpty Then
        Exit Function
    End If
        
    'if the feature is of type polyline or polygon
    If pGeometry.GeometryType = esriGeometryPolyline Then
        ipIFeature.setFeatureType kPolyline
        GetFeature pGeometry, ipIFeature
    ElseIf pGeometry.GeometryType = esriGeometryPolygon Then
        ipIFeature.setFeatureType kPolygon
        
        'preserve the original orientation for the polygon
        Dim pPolygon As esriGeometry.IPolygon
        Set pPolygon = pGeometry
        GetFeature pGeometry, ipIFeature
    End If
    If Not ipIFeature Is Nothing Then
        Set IDisplayAdapter_GetFeatureGeometry = ipIFeature
    Else
        Dim ex As ICOMUCLIDException
        Set ex = New COMUCLIDException
        ex.AddHistoryRecord "ELI01709", "Failed to get IFeature from the current selection."
        ex.Display
    End If
        
Exit Function
ErrHandler:
    handleError "ELI01731"
End Function

Private Function IDisplayAdapter_GetLastPoint(dX As Double, dY As Double) As Long
On Error GoTo ErrHandler
    IDisplayAdapter_GetLastPoint = 0
    
    Dim pEditSketch As esriEditor.IEditSketch
    Set pEditSketch = m_pEditor
    If Not pEditSketch.Geometry.IsEmpty Then
        Dim pSegCol As esriGeometry.ISegmentCollection
        Set m_pCurrentSketch = pEditSketch.Geometry
        Set m_pPart = m_pCurrentSketch.Geometry(m_pCurrentSketch.GeometryCount - 1)
        If Not m_pPart Is Nothing Then
            Dim pPoint As esriGeometry.IPoint
            Dim pSegmentCol As esriGeometry.ISegmentCollection
            Dim numOfSegments As Long
            Set pSegmentCol = m_pPart
            numOfSegments = pSegmentCol.SegmentCount
            If numOfSegments > 0 Then
                If m_bPolygonSketch Then
                    If numOfSegments > 1 Then
                        Set pPoint = pSegmentCol.Segment(numOfSegments - 2).ToPoint
                    ElseIf numOfSegments = 1 Then
                        Set pPoint = pSegmentCol.Segment(numOfSegments - 1).ToPoint
                    Else
                        Set pPoint = Nothing
                    End If
                Else
                    Set pPoint = m_pPart.ToPoint
                End If
            ElseIf numOfSegments = 0 Then
                Set pPoint = m_pPart.ToPoint
            End If
            If Not pPoint Is Nothing Then
                dX = pPoint.X
                dY = pPoint.Y
                IDisplayAdapter_GetLastPoint = 1
            End If
        End If
    End If
Exit Function
ErrHandler:
    handleError "ELI01732"
End Function

Private Function IDisplayAdapter_GetLastSegmentTanOutAsPolarAngleInRadians(pdTangentOutAngle As Double) As Boolean
On Error GoTo ErrHandler
    IDisplayAdapter_GetLastSegmentTanOutAsPolarAngleInRadians = False
    'if the current sketch is still under construction
    If Not m_pCurrentSketch Is Nothing Then
        'remove the last segment
        Dim pMxDoc As esriArcMapUI.IMxDocument
        Dim pEditSketch As esriEditor.IEditSketch
        Dim pSegCol As esriGeometry.ISegmentCollection

        Set pEditSketch = m_pEditor
        Set pMxDoc = m_pApplication.Document
        Set m_pCurrentSketch = pEditSketch.Geometry

        'if current sketch has at least one part
        If Not m_pCurrentSketch.GeometryCount = 0 Then
            'QI m_pPart to the last part (i.e. the current part) of the current sketch
            'so that we can continue working on the current part without losing any previous information
            Set m_pPart = m_pCurrentSketch.Geometry(m_pCurrentSketch.GeometryCount - 1)
        End If

        'QI pSegCol to the current part of the sketch
        Set pSegCol = m_pPart
        Dim iSegmentCount As Long
        iSegmentCount = pSegCol.SegmentCount

        'last segment of the sketch
        If iSegmentCount > 0 Then
            Dim pTangentLine As esriGeometry.ILine
            Set pTangentLine = New esriGeometry.Line
            Dim iNum As Integer
            ' for polygon layer, the second to last segment is the one
            If m_bPolygonSketch Then
                iNum = 2
            Else
                iNum = 1
            End If
            ' get the tangent-out line from the segment
            pSegCol.Segment(iSegmentCount - iNum).QueryTangent esriExtendTangentAtTo, 1, True, 20, pTangentLine
            If Not pTangentLine Is Nothing Then
                ' Get the polar angle value of the line in radians
                pdTangentOutAngle = pTangentLine.Angle
                IDisplayAdapter_GetLastSegmentTanOutAsPolarAngleInRadians = True
            End If
            Set pTangentLine = Nothing
        End If
    End If

Exit Function
ErrHandler:
    ' return false since can't get tangent out. Do not raise error
    IDisplayAdapter_GetLastSegmentTanOutAsPolarAngleInRadians = False
End Function

Private Sub IDisplayAdapter_Redo()
On Error GoTo ErrHandler
    ' find Redo command from ArcMap toolbar
    Dim ipRedoCommand As esriFramework.ICommandItem
    Dim pCommandBars As esriFramework.ICommandBars
    Dim pUID As esriSystem.IUID
    Set pUID = New esriSystem.UID
    ' use esri's undo command to fulfil the task
    pUID.Value = "esriArcMapUI.MxEditMenuItem"
    ' Redo is of subtype 2
    pUID.SubType = 2
    Set pCommandBars = m_pApplication.Document.CommandBars
    Set ipRedoCommand = pCommandBars.Find(pUID)
    If Not ipRedoCommand Is Nothing Then
        ' make sure the command is enabled
        Dim ipCommand As esriSystemUI.ICommand
        Set ipCommand = ipRedoCommand.Command
        If ipCommand.Enabled = True Then
            ipRedoCommand.Execute
        End If
    End If

Exit Sub
ErrHandler:
    handleError "ELI06264"
End Sub

Private Sub IDisplayAdapter_Reset()
On Error GoTo ErrHandler:
    'reset sketch
    Set m_pPart = Nothing
    Set m_pCurrentSketch = Nothing
    m_bStartSketch = True

    'refresh the view
    Dim pMxDoc As esriArcMapUI.IMxDocument
    Set pMxDoc = m_pApplication.Document
    pMxDoc.ActiveView.Refresh

Exit Sub
ErrHandler:
    handleError "ELI01734"
End Sub

Private Sub IDisplayAdapter_SelectDefaultTool()
'On Error GoTo ErrHandler
On Error GoTo ResetLastTool
    ' select the SelectTool, therefore deselect the current tool
    Dim pCommandBars As esriFramework.ICommandBars
    Dim pUID As esriSystem.IUID
    Set pUID = New esriSystem.UID
    pUID.Value = "esriArcMapUI.SelectTool"
    Set pCommandBars = m_pApplication.Document.CommandBars
    If m_pLastTool Is Nothing Then
        Set m_pLastTool = pCommandBars.Find(pUID)
        If Not m_pLastTool Is Nothing Then
            'select the edit tool
            m_pLastTool.Execute
        End If
    Else
        Dim pCommand As esriSystemUI.ICommand
        Set pCommand = m_pLastTool.Command
        ' if last tool is now disabled, then select SelectTool
        If Not pCommand.Enabled Then
            Set m_pLastTool = pCommandBars.Find(pUID)
            If Not m_pLastTool Is Nothing Then
                'select the edit tool
                m_pLastTool.Execute
            End If
        ElseIf m_pLastTool.Name <> "IcoMapDrawingControl" Then
            m_pLastTool.Execute
        End If
    End If
    
Exit Sub
'ErrHandler:
'    handleError "ELI01735"
ResetLastTool:
    Set m_pLastTool = m_pApplication.Document.CommandBars.Find("esriArcMapUI.SelectTool")
End Sub

Private Sub IDisplayAdapter_SelectFeatures(ByVal strCommonSourceDoc As String)
On Error GoTo ErrHandler
    Dim pMxDocument As esriArcMapUI.IMxDocument
    Dim pMap As esriCarto.IMap
    Dim pEnumLayer As esriCarto.IEnumLayer
    Dim pLayer As esriCarto.ILayer
    Dim pId As New esriSystem.UID
    'contains feature ids for those features that have strCommonSourceDoc as hyperlink
    Dim featureIDs As Collection

    Set pMxDocument = m_pApplication.Document
    Set pMap = pMxDocument.FocusMap
    Set featureIDs = New Collection

    Dim iNumOfLayers As Long
    iNumOfLayers = pMap.LayerCount

    Dim selectionSet As esriSelectionResultEnum
    selectionSet = esriSelectionResultNew
 
    'loop through all feature layers to get OIDs of those features
    'which have strCommonSourceDoc as one of their hyper links
    Dim i As Long
    For i = 0 To iNumOfLayers - 1
        Set pLayer = pMap.Layer(i)
        
        ' what's the workspace
        Dim pFeatureLayer As esriCarto.IFeatureLayer
        Set pFeatureLayer = pLayer
        
        Dim pHyperlinkContainer As esriCarto.IHyperlinkContainer
        Dim iHyperlinkCount As Long

        Dim pFeatureSelection As esriCarto.IFeatureSelection
        Set pFeatureSelection = pLayer

        'clean the featureIDs collection first
        Dim j As Long
        j = featureIDs.Count
        Do While j > 0
            featureIDs.Remove j
            j = j - 1
        Loop

        'before select any feature, clear all selections
        pFeatureSelection.Clear
        pMxDocument.ActiveView.Refresh

        Set pHyperlinkContainer = pLayer
        iHyperlinkCount = pHyperlinkContainer.HyperlinkCount

        'iterate through all hyperlinks for this feature layer
        For j = 0 To iHyperlinkCount - 1
            Dim pHyperlink As esriCarto.IHyperlink
            Set pHyperlink = pHyperlinkContainer.Hyperlink(j)
            If StrComp(pHyperlink.Link, strCommonSourceDoc, 1) = 0 Then
                featureIDs.Add pHyperlink.FeatureId
            End If
        Next j
        If featureIDs.Count > 0 Then
            Dim strWhereClause As String
            strWhereClause = ""
            'now if we collected all object ids belong to those features that have strCommonSourceDoc
            'as their hyperlink, then filter out these features
            For j = 1 To featureIDs.Count
                Dim pFeatureClass As esriGeoDatabase.IFeatureClass
                Set pFeatureClass = pFeatureLayer.FeatureClass
                Dim strOIDName As String
                strOIDName = pFeatureClass.OIDFieldName
                If j > 1 Then
                    strWhereClause = strWhereClause & " OR "
                End If
                strWhereClause = strWhereClause & strOIDName & " = " & featureIDs.Item(j)
            Next j

            If strWhereClause <> "" Then
                'create a query filter
                Dim pQFilter As esriGeoDatabase.IQueryFilter
                Set pQFilter = New esriGeoDatabase.QueryFilter
                pQFilter.WhereClause = strWhereClause
    
                pFeatureSelection.SelectFeatures pQFilter, selectionSet, False
                pMxDocument.ActiveView.PartialRefresh esriViewGeoSelection, Nothing, Nothing
            End If

        End If

        selectionSet = esriSelectionResultAdd
    Next i

    Set featureIDs = Nothing

Exit Sub
ErrHandler:
    handleError "ELI01736"
End Sub

Private Sub IDisplayAdapter_SelectIcoMapTool()
On Error GoTo ErrHandler
'**** TODO: make sure it's in editing mode
    'select the edit tool, therefore deselect the current tool
    Dim pCommandItem As esriFramework.ICommandItem
    Dim pCommandBars As esriFramework.ICommandBars
    Dim pUID As esriSystem.IUID
    Set pUID = New esriSystem.UID
    pUID.Value = "{D80801D0-7AC8-11D5-817F-0050DAD4FF55}"
    
    Set pCommandBars = m_pApplication.Document.CommandBars
    
    ' before select icomap tool, set m_pLastTool to current tool
    On Error GoTo ResetLastTool
    Set m_pLastTool = m_pApplication.CurrentTool
    GoTo NextLine
ResetLastTool:
    Set m_pLastTool = m_pApplication.Document.CommandBars.Find("esriArcMapUI.SelectTool")

NextLine:
    ' find icomap tool
    Set pCommandItem = pCommandBars.Find(pUID)
    If Not pCommandItem Is Nothing Then
        'select icomap tool
        pCommandItem.Execute
    End If
Exit Sub
ErrHandler:
    handleError "ELI01820"
End Sub

Private Sub IDisplayAdapter_SetFeatureGeometry(ByVal strFeatureID As String, ByVal ipIFeature As UCLID_FEATUREMGMTLib.IFeature)
On Error GoTo ErrHandler
    Dim pMxDoc As IMxDocument
    Set pMxDoc = m_pApplication.Document
    'only get the feature that is currently selected
    If pMxDoc.FocusMap.SelectionCount = 1 Then
        Dim pEnumFeature As esriGeoDatabase.IEnumFeature
        Dim pFeature As esriGeoDatabase.IFeature
        Set pEnumFeature = pMxDoc.FocusMap.FeatureSelection
        ' get the selected feature
        Set pFeature = pEnumFeature.Next
                
        m_pEditor.StartOperation
        
        If ipIFeature Is Nothing Then
            pFeature.Delete
        Else
            Dim pGeometry As esriGeometry.IGeometry
            Set pGeometry = pFeature.Shape
            'if the feature is of type polyline or polygon
            If pGeometry.GeometryType = esriGeometryPolyline Then
                ipIFeature.setFeatureType kPolyline
                SetFeature pGeometry, ipIFeature
                Set pFeature.Shape = pGeometry
                pFeature.Store
            ElseIf pGeometry.GeometryType = esriGeometryPolygon Then
                ipIFeature.setFeatureType kPolygon
                SetFeature pGeometry, ipIFeature
                Set pFeature.Shape = pGeometry
                pFeature.Store
            End If
        End If
            
        m_pEditor.StopOperation "Modified selected feature."
        
        'refresh the view
        pMxDoc.ActiveView.Refresh
          
    End If
Exit Sub
ErrHandler:
    handleError "ELI01737"
End Sub

Private Sub IDisplayAdapter_SetStartPointForNextPart(ByVal dX As Double, ByVal dY As Double)
On Error GoTo ErrHandler
    Dim pEditSketch As esriEditor.IEditSketch
    Dim startPoint As esriGeometry.IPoint
    
    Set pEditSketch = m_pEditor
    Set startPoint = New esriGeometry.Point
    startPoint.PutCoords dX, dY
    pEditSketch.AddPoint startPoint, True

    Set startPoint = Nothing
    
Exit Sub
ErrHandler:
    handleError "ELI01738"

End Sub

Private Property Get IDisplayAdapter_SupportsPartCreation() As Long
On Error GoTo ErrHandler
    IDisplayAdapter_SupportsPartCreation = m_bSupportsPartCreation
Exit Property
ErrHandler:
    handleError "ELI01739"
End Property

Private Property Get IDisplayAdapter_SupportsSketchCreation() As Long
On Error GoTo ErrHandler
    IDisplayAdapter_SupportsSketchCreation = m_bSupportsSketchCreation
Exit Property
ErrHandler:
    handleError "ELI01740"
End Property

Private Sub IDisplayAdapter_Undo()
On Error GoTo ErrHandler
    ' find Undo command from ArcMap toolbar
    Dim ipUndoCommand As esriFramework.ICommandItem
    Dim pCommandBars As esriFramework.ICommandBars
    Dim pUID As esriSystem.IUID
    Set pUID = New esriSystem.UID
    ' use esri's undo command to fulfil the task
    pUID.Value = "esriArcMapUI.MxEditMenuItem"
    ' Undo is of subtype 1
    pUID.SubType = 1
    Set pCommandBars = m_pApplication.Document.CommandBars
    Set ipUndoCommand = pCommandBars.Find(pUID)
    If Not ipUndoCommand Is Nothing Then
        ' make sure the command is enabled
        Dim ipCommand As esriSystemUI.ICommand
        Set ipCommand = ipUndoCommand.Command
        If ipCommand.Enabled = True Then
            ipUndoCommand.Execute
        End If
    End If
    
Exit Sub
ErrHandler:
    handleError "ELI06263"
End Sub

'===============================================================
'=================== Internal MISC functions ===================
'===============================================================
Private Sub init()
On Error GoTo ErrHandler
    resetGeometry
    m_bStartSketch = False

Exit Sub
ErrHandler:
    handleError "ELI01741"
End Sub
Private Sub resetGeometry()
'resets m_pPart
On Error GoTo ErrHandler
    Dim pEditSketch As esriEditor.IEditSketch
    Set pEditSketch = m_pEditor
    If pEditSketch.GeometryType = esriGeometryPolyline Then
        m_bPolygonSketch = False
        Set m_pPart = New Path
    ElseIf pEditSketch.GeometryType = esriGeometryPolygon Then
        m_bPolygonSketch = True
        Set m_pPart = New Ring
    End If

Exit Sub
ErrHandler:
    handleError "ELI01742"
End Sub

Private Sub addSegment(ByRef inSegment As esriGeometry.ISegment)
On Error GoTo ErrHandler
    Dim pMxDoc As esriArcMapUI.IMxDocument
    Dim pEditSketch As esriEditor.IEditSketch
    Dim pSegCol As esriGeometry.ISegmentCollection
    Dim pRefresh As esriGeoDatabase.IInvalidArea

    Set pEditSketch = m_pEditor
    Set pMxDoc = m_pApplication.Document
    Set m_pCurrentSketch = pEditSketch.Geometry
    Set pRefresh = New esriCarto.InvalidArea
    'get the old sketch region for refreshing after updating
    pRefresh.Add pEditSketch.Geometry

    Dim bOnlyOnePart As Boolean
    Dim numOfParts As Integer
    'when start a sketch, there's only one part
    bOnlyOnePart = True
    numOfParts = m_pCurrentSketch.GeometryCount
    ' if number of parts is 0 or 1 (not the beginning of a part), set m_pPart as current geometry
    ' from edit sketch.
    If numOfParts = 0 Or numOfParts = 1 Then
        Set m_pPart = pEditSketch.Geometry
    ElseIf numOfParts > 1 Then
        bOnlyOnePart = False
        Set m_pPart = m_pCurrentSketch.Geometry(m_pCurrentSketch.GeometryCount - 1)
    End If
    
    'add the operation (i.e. adding a segment) to the operation stack
    Dim pOperationStack As esriSystemUI.IOperationStack
    Set pOperationStack = pMxDoc.OperationStack

    Dim pSkOp As esriEditor.ISketchOperation
    Set pSkOp = New esriEditor.SketchOperation
    'mark the start of this operation
    pSkOp.Start m_pEditor

    'QI pSegCol to the current part of the sketch
    Set pSegCol = m_pPart

    ' If it's polygon layer, make sure all rings are closed after each segment is added
    If m_bPolygonSketch Then
        Dim pRing As esriGeometry.IRing
        Dim pPolygon As esriGeometry.IPolygon
        Dim iSegmentCount As Long
        iSegmentCount = pSegCol.SegmentCount
        If iSegmentCount <= 1 Then
            Dim pPointCol As esriGeometry.IPointCollection
            pSegCol.addSegment inSegment
            If bOnlyOnePart Then
                Set pPolygon = pSegCol
                Set pPointCol = pPolygon
                pPolygon.Close
            Else
                Set pRing = pSegCol
                Set pPointCol = pRing
                pRing.Close
            End If
            ' Remove the very first point that we added as the starting
            ' point for the current sketch part
            pPointCol.RemovePoints 0, 1
        ElseIf iSegmentCount > 1 Then
            ' if segment count is more than 1, let's remove the
            ' last segment from the ring first
            pSegCol.RemoveSegments iSegmentCount - 1, 1, False
            pSegCol.addSegment inSegment
            If bOnlyOnePart Then
                Set pPolygon = pSegCol
                ' if the polygon is already closed, only add the point to
                ' the end of the segment (which moves the red dot from start
                ' point to the end point)
                If pPolygon.IsClosed Then
                    pEditSketch.AddPoint inSegment.ToPoint, False
                Else
                    pPolygon.Close
                End If
            Else
                Set pRing = pSegCol
                ' if the polygon is already closed, only add the point to
                ' the end of the segment (which moves the red dot from start
                ' point to the end point)
                If pRing.IsClosed Then
                    pEditSketch.AddPoint inSegment.ToPoint, False
                Else
                    pRing.Close
                End If
            End If
        End If
        Set pRing = Nothing
    Else
        Set pPointCol = m_pPart
        If pSegCol.SegmentCount = 0 And pPointCol.PointCount = 1 Then
            ' remove the redundant starting point
            pPointCol.RemovePoints 0, 1
        End If
        pSegCol.addSegment inSegment
    End If

    'put the operation onto the operation stack
    pSkOp.Finish pEditSketch.Geometry.Envelope

    'refresh the sketch for display
    pRefresh.Add m_pPart
    Set pRefresh.Display = m_pEditor.Display
    pRefresh.Invalidate esriDisplay.esriNoScreenCache
    pEditSketch.RefreshSketch

    Set pSegCol = Nothing
    Set pRefresh = Nothing

Exit Sub
ErrHandler:
    handleError "ELI01743"
End Sub
'============================= Get Feature, Part, Segment =======================================
Private Sub GetFeature(ByRef pGeometry As esriGeometry.IGeometry, ByRef pUCLIDFeature As UCLID_FEATUREMGMTLib.IFeature)
On Error GoTo ErrHandler
    Dim pGeometryCollection As esriGeometry.IGeometryCollection
    Dim pUCLIDEnumPart As UCLID_FEATUREMGMTLib.IEnumPart
    Dim pUCLIDPart As UCLID_FEATUREMGMTLib.IPart
    Set pGeometryCollection = pGeometry
    
    'A feature can have one or more parts
    Dim iNumOfParts As Long
    iNumOfParts = pGeometryCollection.GeometryCount
    If iNumOfParts > 0 Then
        Dim index As Long
        For index = 0 To iNumOfParts - 1
            Dim pCurve As esriGeometry.ICurve
            Set pCurve = pGeometryCollection.Geometry(index)
            Set pUCLIDPart = New UCLID_FEATUREMGMTLib.Part
            GetPart pCurve, pUCLIDPart
            If Not pUCLIDPart Is Nothing Then
                pUCLIDFeature.addPart pUCLIDPart
            End If
            Set pUCLIDPart = Nothing
        Next index
    End If
    
Exit Sub
ErrHandler:
    handleError "ELI01744"
End Sub
Private Sub GetPart(ByRef pCurve As esriGeometry.ICurve, ByRef pUCLIDPart As UCLID_FEATUREMGMTLib.IPart)
On Error GoTo ErrHandler
    ' First of all, set the starting point for the uclid part
    Dim pUCLIDStartPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
    Set pUCLIDStartPoint = New UCLID_MEASUREMENTSLib.CartographicPoint
    pUCLIDStartPoint.InitPointInXY pCurve.FromPoint.X, pCurve.FromPoint.Y
    pUCLIDPart.setStartingPoint pUCLIDStartPoint
    
    ' Traverse the curve (could be a path on polyline layer or a ring on polygon layer),
    ' get each segment info and then add them to the uclid part
    Dim iNumOfSegments As Long
    Dim pSegmentCollection As esriGeometry.ISegmentCollection
    Set pSegmentCollection = pCurve
    iNumOfSegments = pSegmentCollection.SegmentCount
    If iNumOfSegments > 0 Then
        Dim i As Long
        For i = 0 To iNumOfSegments - 1
            Dim pSegment As esriGeometry.ISegment
            Dim pUCLIDSegment As UCLID_FEATUREMGMTLib.ISegment
            Set pSegment = pSegmentCollection.Segment(i)
            If pSegment.GeometryType = esriGeometryLine Then
                Set pUCLIDSegment = New UCLID_FEATUREMGMTLib.LineSegment
            ElseIf pSegment.GeometryType = esriGeometryCircularArc Then
                Set pUCLIDSegment = New UCLID_FEATUREMGMTLib.ArcSegment
            End If
            If Not pUCLIDSegment Is Nothing Then
                'if this is a line segment and start point equals end point, skip it
                If Not (pSegment.GeometryType = esriGeometryLine _
                    And Abs(pSegment.FromPoint.X - pSegment.ToPoint.X) <= 0.000000001 _
                    And Abs(pSegment.FromPoint.Y - pSegment.ToPoint.Y) <= 0.000000001) Then
                    ' get the segment
                    GetSegment pSegment, pUCLIDSegment
                    ' Add the segment to the uclid part
                    pUCLIDPart.addSegment pUCLIDSegment
                End If
            End If
            
            Set pUCLIDSegment = Nothing
        Next i
    End If
    
Exit Sub
ErrHandler:
    handleError "ELI01745"
End Sub
Private Sub GetSegment(ByRef pSegment As esriGeometry.ISegment, ByRef pUCLIDSegment As UCLID_FEATUREMGMTLib.ISegment)
On Error GoTo ErrHandler
    If Not pSegment Is Nothing And Not pUCLIDSegment Is Nothing Then
        Dim pUCLIDStartPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
        Dim pUCLIDEndPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
        Set pUCLIDStartPoint = New UCLID_MEASUREMENTSLib.CartographicPoint
        Set pUCLIDEndPoint = New UCLID_MEASUREMENTSLib.CartographicPoint
        pUCLIDStartPoint.InitPointInXY pSegment.FromPoint.X, pSegment.FromPoint.Y
        pUCLIDEndPoint.InitPointInXY pSegment.ToPoint.X, pSegment.ToPoint.Y
        
        If pSegment.GeometryType = esriGeometryLine Then
            ' Set uclid line segment using starting and ending point
            Dim pUCLIDLineSegment As UCLID_FEATUREMGMTLib.ILineSegment
            Set pUCLIDLineSegment = pUCLIDSegment
            pUCLIDLineSegment.setParamsFromCoords pUCLIDStartPoint, pUCLIDEndPoint
        ElseIf pSegment.GeometryType = esriGeometryCircularArc Then
            ' Retrieve center point of the arc
            Dim pUCLIDCenterPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
            Dim pCircularArc As esriGeometry.ICircularArc
            Set pCircularArc = pSegment
            Set pUCLIDCenterPoint = New CartographicPoint
            pUCLIDCenterPoint.InitPointInXY pCircularArc.CenterPoint.X, pCircularArc.CenterPoint.Y
            
            Dim pUCLIDArcSegment As UCLID_FEATUREMGMTLib.IArcSegment
            Set pUCLIDArcSegment = pUCLIDSegment
            ' Create a vector for parameters
            Dim pVecParams As IIUnknownVector
            Set pVecParams = New IUnknownVector

            Dim dX As Double
            Dim dY As Double
            ' Set uclid arc segment using start, center, and end point
            ' Create type value pair for each parameter
            Dim pValueTypePair1 As UCLID_FEATUREMGMTLib.IParameterTypeValuePair
            Set pValueTypePair1 = New UCLID_FEATUREMGMTLib.ParameterTypeValuePair
            ' Set starting point
            pValueTypePair1.eParamType = kArcStartingPoint
            pUCLIDStartPoint.GetPointInXY dX, dY
            ' make the point x and y value into a string, ex. "12.34,34.221"
            pValueTypePair1.strValue = CStr(dX) & "," & CStr(dY)
            pVecParams.PushBack pValueTypePair1
            
            Dim pValueTypePair2 As UCLID_FEATUREMGMTLib.IParameterTypeValuePair
            Set pValueTypePair2 = New UCLID_FEATUREMGMTLib.ParameterTypeValuePair
            ' Set center point
            pValueTypePair2.eParamType = kArcCenter
            pUCLIDCenterPoint.GetPointInXY dX, dY
            pValueTypePair2.strValue = CStr(dX) & "," & CStr(dY)
            pVecParams.PushBack pValueTypePair2

            Dim pValueTypePair3 As UCLID_FEATUREMGMTLib.IParameterTypeValuePair
            Set pValueTypePair3 = New UCLID_FEATUREMGMTLib.ParameterTypeValuePair
            ' Set end point
            pValueTypePair3.eParamType = kArcEndingPoint
            pUCLIDEndPoint.GetPointInXY dX, dY
            pValueTypePair3.strValue = CStr(dX) & "," & CStr(dY)
            pVecParams.PushBack pValueTypePair3
            
            ' Set concavity
            Dim pValueTypePair4 As UCLID_FEATUREMGMTLib.IParameterTypeValuePair
            Set pValueTypePair4 = New UCLID_FEATUREMGMTLib.ParameterTypeValuePair
            pValueTypePair4.eParamType = kArcConcaveLeft
            pValueTypePair4.strValue = IIf(pCircularArc.IsCounterClockwise, "1", "0")
            pVecParams.PushBack pValueTypePair4
                        
            pUCLIDArcSegment.setParameters pVecParams
        
            Set pUCLIDCenterPoint = Nothing
            Set pVecParams = Nothing
        End If
        
        Set pUCLIDStartPoint = Nothing
        Set pUCLIDEndPoint = Nothing
    End If

Exit Sub
ErrHandler:
    handleError "ELI01746"
End Sub

'============================== Set Feature, Part, Segment =====================================
Private Sub SetFeature(ByRef pGeometry As esriGeometry.IGeometry, ByRef pUCLIDFeature As UCLID_FEATUREMGMTLib.IFeature)
On Error GoTo ErrHandler
    Dim pGeometryCollection As esriGeometry.IGeometryCollection
    Set pGeometryCollection = pGeometry
    
    ' traverse each part in this uclid feature
    Dim pEnumPart As UCLID_FEATUREMGMTLib.IEnumPart
    Dim pUCLIDPart As UCLID_FEATUREMGMTLib.IPart
    Dim pNewGeometry As esriGeometry.IGeometry
    Dim pNewGeometryCollection As esriGeometry.IGeometryCollection
    ' Create a new geometry
    If pUCLIDFeature.getFeatureType = kPolyline Then
        Set pNewGeometry = New esriGeometry.Polyline
    ElseIf pUCLIDFeature.getFeatureType = kPolygon Then
        Set pNewGeometry = New esriGeometry.Polygon
    End If '' TODO: if it's neither polyline, nor is it polygon, what to do?
    
    Set pNewGeometryCollection = pNewGeometry
    
    ' Get each part from the uclid feature
    Set pEnumPart = pUCLIDFeature.getParts
    pEnumPart.Reset
    Set pUCLIDPart = pEnumPart.Next
    Do While Not pUCLIDPart Is Nothing
        Dim pCurve As esriGeometry.ICurve
        If pUCLIDFeature.getFeatureType = kPolyline Then
            Set pCurve = New esriGeometry.Path
        ElseIf pUCLIDFeature.getFeatureType = kPolygon Then
            Set pCurve = New esriGeometry.Ring
        Else
            Exit Do
        End If
        ' Set the part based upon uclid part
        SetPart pCurve, pUCLIDPart
        If Not pCurve Is Nothing Then
            ' Add each part to the new geometry collection
            pNewGeometryCollection.AddGeometry pCurve
            If pUCLIDFeature.getFeatureType = kPolygon Then
                If Not pCurve.IsClosed Then
                    Dim pRing As esriGeometry.IRing
                    Set pRing = pCurve
                    pRing.Close
                End If
            End If
        End If
        Set pUCLIDPart = pEnumPart.Next
    Loop
    
    If pUCLIDFeature.getNumParts > 0 Then
        ' Replace the old geometry with the new one
        pGeometryCollection.RemoveGeometries 0, pGeometryCollection.GeometryCount
        pGeometryCollection.SetGeometryCollection pNewGeometryCollection
    End If
Exit Sub
ErrHandler:
    handleError "ELI01747"
End Sub
Private Sub SetPart(ByRef pCurve As esriGeometry.ICurve, ByRef pUCLIDPart As UCLID_FEATUREMGMTLib.IPart)
On Error GoTo ErrHandler
    Dim pSegmentCollection As esriGeometry.ISegmentCollection
    Set pSegmentCollection = pCurve
    
    ' Iterate through the part for segments
    Dim pEnumSegment As UCLID_FEATUREMGMTLib.IEnumSegment
    Dim pUCLIDSegment As UCLID_FEATUREMGMTLib.ISegment
    Dim pUCLIDStartPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
    
    ' Set the start point to the start point of the part first
    Set pUCLIDStartPoint = pUCLIDPart.getStartingPoint
    Set pEnumSegment = pUCLIDPart.getSegments
    pEnumSegment.Reset
    Set pUCLIDSegment = pEnumSegment.Next
    Do While Not pUCLIDSegment Is Nothing
        Dim pSegment As esriGeometry.ISegment
        If pUCLIDSegment.getSegmentType = kLine Then
            Set pSegment = New esriGeometry.Line
        ElseIf pUCLIDSegment.getSegmentType = kArc Then
            Set pSegment = New esriGeometry.CircularArc
        Else
            Exit Do
        End If
        ' Set segment based upon uclid segment
        SetSegment pSegment, pUCLIDSegment, pUCLIDStartPoint
        If Not pSegment Is Nothing Then
            ' Add the new segment to the part
            pSegmentCollection.addSegment pSegment
            ' reset the start point equal the end point of the last segment
            pUCLIDStartPoint.InitPointInXY pSegment.ToPoint.X, pSegment.ToPoint.Y
        Else
            Exit Do
        End If
        
        Set pSegment = Nothing
        Set pUCLIDSegment = pEnumSegment.Next
    Loop
    
Exit Sub
ErrHandler:
    handleError "ELI01748"
End Sub
Private Sub SetSegment(ByRef pSegment As esriGeometry.ISegment, ByRef pUCLIDSegment As UCLID_FEATUREMGMTLib.ISegment, ByRef pUCLIDStartPoint As CartographicPoint)
On Error GoTo ErrHandler
    Dim pStartPt As esriGeometry.IPoint
    Set pStartPt = New esriGeometry.Point
    Dim dX As Double
    Dim dY As Double
    pUCLIDStartPoint.GetPointInXY dX, dY
    pStartPt.PutCoords dX, dY
    If pSegment.GeometryType = esriGeometry.esriGeometryLine Then
        Dim pLine As esriGeometry.ILine
        Set pLine = pSegment
        'using start, end points to construct the line
        
        ' Get end point of the line from uclid segment
        Dim pUCLIDEndPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
        Dim pUCLIDLine As UCLID_FEATUREMGMTLib.ILineSegment
        Set pUCLIDLine = pUCLIDSegment
        Set pUCLIDEndPoint = Nothing
        pUCLIDLine.getCoordsFromParams pUCLIDStartPoint, pUCLIDEndPoint
        ' Set end point of the line for the esri line
        Dim pEndPt As esriGeometry.IPoint
        Set pEndPt = New esriGeometry.Point
        pUCLIDEndPoint.GetPointInXY dX, dY
        pEndPt.PutCoords dX, dY
        
        pLine.PutCoords pStartPt, pEndPt
        Set pEndPt = Nothing
        
    ElseIf pSegment.GeometryType = esriGeometryCircularArc Then
        Dim pCircularArc As esriGeometry.ICircularArc
        Set pCircularArc = pSegment
        
        Dim pUCLIDArc As UCLID_FEATUREMGMTLib.IArcSegment
        Set pUCLIDArc = pUCLIDSegment
        'First, get mid, end points from uclid segment
        Dim pUCLIDMidPoint As UCLID_MEASUREMENTSLib.ICartographicPoint
        Set pUCLIDMidPoint = Nothing
        Set pUCLIDEndPoint = Nothing
        pUCLIDArc.getCoordsFromParams pUCLIDStartPoint, pUCLIDMidPoint, pUCLIDEndPoint
        
        ' construct the esri arc with start, mid and end point of the arc
        Dim pConstructArc As esriGeometry.IConstructCircularArc
        Dim pMidPt As esriGeometry.IPoint
        Set pMidPt = New esriGeometry.Point
        Set pEndPt = New esriGeometry.Point
        pUCLIDMidPoint.GetPointInXY dX, dY
        pMidPt.PutCoords dX, dY
        pUCLIDEndPoint.GetPointInXY dX, dY
        pEndPt.PutCoords dX, dY
        Set pConstructArc = pCircularArc
        pConstructArc.ConstructThreePoints pStartPt, pMidPt, pEndPt, False
        
        Set pMidPt = Nothing
        Set pEndPt = Nothing
    End If
    Set pStartPt = Nothing
Exit Sub
ErrHandler:
    handleError "ELI01749"
End Sub

'--------------------------------------------------------------------------------------------------
'Private Function GetWorkspaceDescription(ByVal pFeatureClass As esriGeoDatabase.IFeatureClass) As Integer
'On Error GoTo ErrHandler
'    Dim pWorkspaceFactory As esriCore.IWorkspaceFactory
'    Dim strWorkspaceDescription As String
'    Dim pDataset As esriCore.IDataset
'    Dim pWorkspace As esriCore.IWorkspace
'    Set pDataset = pFeatureClass
'    Set pWorkspace = pDataset.Workspace
'    Set pWorkspaceFactory = pWorkspace.WorkspaceFactory
'    strWorkspaceDescription = pWorkspaceFactory.WorkspaceDescription(False)
'
'    GetWorkspaceDescription = enumNone
'
'    If strWorkspaceDescription = "Spatial Database Connection" Then     'Geodatabase
'        GetWorkspaceDescription = enumGeodatabase
'    ElseIf strWorkspaceDescription = "ArcInfo Workspace" Then     'coverage
'        GetWorkspaceDescription = enumCoverage
'    ElseIf strWorkspaceDescription = "Personal Geodatabase" Then      'personal Geodatabase
'        GetWorkspaceDescription = enumPersonalGeodatabase
'    ElseIf strWorkspaceDescription = "Shapefiles" Then       'shapefile
'        GetWorkspaceDescription = enumShape
'    End If
'
'Exit Function
'ErrHandler:
'    handleError "ELI01749"
'
'End Function
'----------------------------------------------------------------------------------------------

Private Sub handleError(ByVal strELI As String)
    'create a COM UCLIDException object
    Dim ex As ICOMUCLIDException
    Set ex = New COMUCLIDException
    ex.CreateFromString strELI, Err.Description
    ex.AddDebugInfo "Error Number", Err.Number

    ' convert the UCLIDException to a string and throw as a COM error with
    ' description
    Dim strMsg As String
    strMsg = ex.AsStringizedByteStream
    
    Err.Raise vbObjectError, "", strMsg
End Sub
Private Function GroundToGridConversionCurve(ByVal ipArcSegment As UCLID_FEATUREMGMTLib.IArcSegment) As UCLID_FEATUREMGMTLib.IArcSegment
On Error GoTo ErrHandler
    ' check registry if GroundToGridConversion is enabled
    If Not isGroundToGridOn Then
        Set GroundToGridConversionCurve = ipArcSegment
        Exit Function
    End If
    
    ' create a new arc segment
    Dim ipConvertedArcSegment As New UCLID_FEATUREMGMTLib.ArcSegment
    Dim ipParams As UCLID_COMUTILSLib.IIUnknownVector
    Dim ipNewParams As New UCLID_COMUTILSLib.IUnknownVector
    Dim nSize As Long
    Dim n As Long
    ' get all parameters stored in the vector in the form of IParameterTypeValuePair
    Set ipParams = ipArcSegment.getParameters
    nSize = ipParams.Size()
    For n = 0 To nSize - 1
        Dim ipTypeToValuePair As UCLID_FEATUREMGMTLib.IParameterTypeValuePair
        Dim ipNewTypeToValuePair As UCLID_FEATUREMGMTLib.IParameterTypeValuePair
        Dim strConvertedValue As String
        Set ipNewTypeToValuePair = New UCLID_FEATUREMGMTLib.ParameterTypeValuePair
        ' get each parameter pair
        Set ipTypeToValuePair = ipParams.At(n)
        ' get the curve parameter type
        Dim eCurveParam As UCLID_CURVEPARAMETERLib.ECurveParameterType
        eCurveParam = ipTypeToValuePair.eParamType
        ' the string value
        Dim strValue As String
        strValue = ipTypeToValuePair.strValue
    
        strConvertedValue = GroundToGridConversion(eCurveParam, strValue)
        ipNewTypeToValuePair.eParamType = eCurveParam
        ipNewTypeToValuePair.strValue = strConvertedValue
        ' store the parameter pair
        ipNewParams.PushBack ipNewTypeToValuePair
    Next n
    
    ' set parameters
    ipConvertedArcSegment.setParameters ipNewParams
    
    Set GroundToGridConversionCurve = ipConvertedArcSegment

Exit Function
ErrHandler:
    handleError "ELI07518"
End Function
Private Function GroundToGridConversionLine(ByVal ipLineSegment As UCLID_FEATUREMGMTLib.ILineSegment) As UCLID_FEATUREMGMTLib.ILineSegment
On Error GoTo ErrHandler
    ' check registry if GroundToGridConversion is enabled
    If Not isGroundToGridOn Then
        Set GroundToGridConversionLine = ipLineSegment
        Exit Function
    End If
    
    ' create a new line segment
    Dim ipConvertedLineSegment As New UCLID_FEATUREMGMTLib.LineSegment
    Dim eCurveParamBearing As UCLID_CURVEPARAMETERLib.ECurveParameterType, eCurveParamDistance As UCLID_CURVEPARAMETERLib.ECurveParameterType
    eCurveParamBearing = kLineBearing
    eCurveParamDistance = kLineDistance
    Dim strBearing As String, strDistance As String
    ipLineSegment.getBearingDistance strBearing, strDistance
    strBearing = GroundToGridConversion(eCurveParamBearing, strBearing)
    strDistance = GroundToGridConversion(eCurveParamDistance, strDistance)
    ipConvertedLineSegment.setBearingDistance strBearing, strDistance

   Set GroundToGridConversionLine = ipConvertedLineSegment
Exit Function
ErrHandler:
    handleError "ELI07519"
End Function

Private Function GroundToGridConversion(ByVal eCurveParam As ECurveParameterType, ByVal strOriginValue As String) As String
On Error GoTo ErrHandler
    ' depend on the curve type, do proper ground to grid conversion
    Select Case eCurveParam
        ' bearing in the format of, for instance, N23D23'65"W
        Case kArcTangentInBearing, kArcTangentOutBearing, _
        kArcChordBearing, kArcRadialInBearing, kArcRadialOutBearing, kLineBearing
            GroundToGridConversion = convertBearing(strOriginValue)
            Exit Function
        ' distances
        Case kArcRadius, kArcLength, kArcChordLength, kArcExternalDistance, _
        kArcMiddleOrdinate, kArcTangentDistance, kLineDistance
            GroundToGridConversion = convertDistance(strOriginValue)
            Exit Function
        Case Else
            ' no conversion
    End Select
    
    GroundToGridConversion = strOriginValue
    
Exit Function
ErrHandler:
    handleError "ELI07525"
End Function
Private Function convertBearing(ByVal strBearing) As String
' This function adds ground to grid correction to the input bearing
On Error GoTo ErrHandler
    ' input bearing format is (N|S)\d{2}D\d{2}'\d{2}"(E|W) or N|E|S|W
    Dim nLen As Integer
    nLen = Len(strBearing)
    
    Dim dFinalAngle As Double
    If nLen = 1 Then
        ' if the string is in the form of N|E|S|W
        Select Case strBearing
            Case "N"
                dFinalAngle = 90#
            Case "W"
                dFinalAngle = 180#
            Case "S"
                dFinalAngle = 270#
            Case "E"
                dFinalAngle = 0#
            Case Else
                ' error
        End Select
        
    ElseIf nLen = 11 Then
        Dim strStartDirection As String, strEndDirection As String
        Dim dDegrees As Double, dMinutes As Double, dSeconds As Double, dAngle As Double
        strStartDirection = Left(strBearing, 1)
        strEndDirection = Right(strBearing, 1)
        dDegrees = Mid(strBearing, 2, 2)
        dMinutes = Mid(strBearing, 5, 2)
        dSeconds = Mid(strBearing, 8, 2)
        ' get the angle excluding the start and end direction
        dAngle = dDegrees + dMinutes / 60# + dSeconds / 3600#
        Dim dStartAngle As Double
        Select Case strStartDirection
            Case "N"
                dStartAngle = 90#
                Select Case strEndDirection
                    Case "W"
                        dFinalAngle = dStartAngle + dAngle
                    Case "E"
                        dFinalAngle = dStartAngle - dAngle
                    Case Else
                        ' error
                End Select
            Case "S"
                dStartAngle = 270#
                Select Case strEndDirection
                    Case "W"
                        dFinalAngle = dStartAngle - dAngle
                    Case "E"
                        dFinalAngle = dStartAngle + dAngle
                    Case Else
                        ' error
                End Select
            Case Else
                ' error
        End Select
    End If
    
    dFinalAngle = dFinalAngle + m_dAngleOffset
    ' convert to the bearing string format again (eg. N23D34'45"W)
    Dim strConvertedBearing As String
    Dim n As Integer
    n = Fix(dFinalAngle / 360#)
    If n > 0 Then
        ' get the angle within 0 ~ 360°
        dFinalAngle = dFinalAngle - 360# * n
    End If
    
    If dFinalAngle = 0# Then
        strConvertedBearing = "E"
    ElseIf dFinalAngle > 0# And dFinalAngle < 90# Then
        dFinalAngle = 90# - dFinalAngle
        strConvertedBearing = "N" & getDegMinSecond(dFinalAngle) & "E"
    ElseIf dFinalAngle = 90# Then
        strConvertedBearing = "N"
    ElseIf dFinalAngle > 90# And dFinalAngle < 180# Then
        dFinalAngle = dFinalAngle - 90#
        strConvertedBearing = "N" & getDegMinSecond(dFinalAngle) & "W"
    ElseIf dFinalAngle = 180# Then
        strConvertedBearing = "W"
    ElseIf dFinalAngle > 180# And dFinalAngle < 270# Then
        dFinalAngle = 270# - dFinalAngle
        strConvertedBearing = "S" & getDegMinSecond(dFinalAngle) & "W"
    ElseIf dFinalAngle = 270# Then
        strConvertedBearing = "S"
    ElseIf dFinalAngle > 270# And dFinalAngle < 360# Then
        dFinalAngle = dFinalAngle - 270#
        strConvertedBearing = "S" & getDegMinSecond(dFinalAngle) & "E"
    End If
    
    convertBearing = strConvertedBearing
    
Exit Function
ErrHandler:
    handleError "ELI07526"
End Function
Private Function convertDistance(ByVal strDistance As String) As String
' This function adds ground to grid correction to the input distance
On Error GoTo ErrHandler
    Dim nLen As Integer, i As Integer
    Dim strNumber As String, strChar As String, strUnit As String
    nLen = Len(strDistance)
    strNumber = ""
    strUnit = ""
    For i = 1 To nLen
        strChar = Mid(strDistance, i, 1)
        If IsNumeric(strChar) Or strChar = "." Then
            strNumber = strNumber + strChar
        Else
            strUnit = strUnit + strChar
        End If
    Next i
    
    Dim dNumber As Double
    dNumber = strNumber
    ' count the distance factor
    dNumber = dNumber * m_dDistanceFactor
    ' convert it back to string
    strNumber = dNumber
    convertDistance = strNumber + strUnit
Exit Function
ErrHandler:
    handleError "TODO"
End Function

Private Sub getGroundToGridParameters()
On Error GoTo ErrHandler
    m_bGroundToGridChecked = False
    m_dDistanceFactor = 1#
    m_dAngleOffset = 0#
    ' check the editor's option on ArcGIS-ArcMap
    Dim ipEditProperties As esriEditor.IEditProperties2
    Set ipEditProperties = m_pEditor
    If Not ipEditProperties Is Nothing Then
        ' if Ground-to-Grid is checked
        If ipEditProperties.UseGroundToGrid Then
            m_bGroundToGridChecked = True
            m_dDistanceFactor = ipEditProperties.DistanceCorrectionFactor
            m_dAngleOffset = ipEditProperties.AngularCorrectionOffset * 180# / (4 * Atn(1))
        End If
    End If
Exit Sub
ErrHandler:
    handleError "ELI07531"
End Sub
Private Function getDegMinSecond(ByVal dAngleValue As Double) As String
' get the angle in the form of xxDxx'xx"
    Dim nDegrees As Integer, nMinutes As Integer, nSeconds As Integer
    nDegrees = Fix(dAngleValue)
    nMinutes = Fix((dAngleValue - nDegrees) * 60)
    nSeconds = (dAngleValue - nDegrees - nMinutes / 60) * 3600

    getDegMinSecond = nDegrees & "D" & nMinutes & "'" & nSeconds & """"
End Function
Private Function isGroundToGridOn() As Boolean
On Error GoTo ErrHandler
    ' Check if the registry key for Gound-To-Grid is turned on
    Dim strValue As String, strKeyPath As String, strKeyName As String
    Dim strHKey As Long
    strHKey = HKEY_CURRENT_USER
    strKeyPath = "Software\UCLID Software\IcoMap for ArcGIS\Options\General"
    strKeyName = "GroundToGridOn"
    If Not KeyExists(strHKey, strKeyPath, strKeyName) Then
        SaveSettingString strHKey, strKeyPath, strKeyName, "0"
    End If
    
    strValue = GetSettingString(strHKey, strKeyPath, strKeyName, "0")
    ' also check if the correction is check in the editor option dlg
    getGroundToGridParameters
    isGroundToGridOn = IIf(strValue = "1" And m_bGroundToGridChecked, True, False)
Exit Function
ErrHandler:
    handleError "ELI07532"
End Function
