namespace Extract.GoogleCloud.Dto

/// The levels that can be iterated (each representing one or more symbols)
type PageIteratorLevel =
    | None = 0
    | Block = 1
    | Para = 2
    | TextLine = 3
    | Word = 4
    | Symbol = 5

/// Struct used to represent bounding boxes
[<Struct>]
type Rect =
    { Left: int
      Right: int
      Top: int
      Bottom: int }
    member x.Width = x.Right - x.Left
    member x.Height = x.Bottom - x.Top

/// Interface designed after the iterator exposed by Tesseract to support reusable converter code (convert output of different OCR engines to GCV-compatible JSON)
type IRecognizedCharactersIterator =
    abstract Next: level: PageIteratorLevel -> bool
    abstract Next: parentLevel: PageIteratorLevel * level: PageIteratorLevel -> bool
    abstract Prev: parentLevel: PageIteratorLevel * level: PageIteratorLevel -> bool
    abstract IsAtBeginningOf: level: PageIteratorLevel -> bool
    abstract IsAtFinalOf: parentLevel: PageIteratorLevel * level: PageIteratorLevel -> bool
    abstract HasNext: parentLevel: PageIteratorLevel * level: PageIteratorLevel -> bool
    abstract TryGetBoundingBox: level: PageIteratorLevel * bounds: outref<Rect> -> bool
    abstract GetConfidence: level: PageIteratorLevel -> single
    abstract GetText: level: PageIteratorLevel -> string
