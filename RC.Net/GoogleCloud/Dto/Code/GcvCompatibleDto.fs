// Records that copy the format described here: https://cloud.google.com/vision/docs/reference/rest/v1/AnnotateImageResponse#TextAnnotation
module Extract.GoogleCloud.Dto.Dto

open Newtonsoft.Json
open System

/// Possible breaks according to GCV
type BreakType =
    | UNKNOWN = 0
    | SPACE = 1
    | SURE_SPACE = 2
    | EOL_SURE_SPACE = 3
    | HYPHEN = 4
    | LINE_BREAK = 5

/// Describes spaces and line breaks
type DetectedBreak =
    { ``type``: BreakType
      isPrefix: Nullable<bool> }

/// Language information
type DetectedLanguage =
    { languageCode: string
      confidence: single }

/// Used to add info about spaces and line breaks to symbols
type TextProperty =
    { detectedLanguages: DetectedLanguage array
      detectedBreak: DetectedBreak }

/// Point described by pixels
type Vertex = { x: int; y: int }

/// Point described by page fractions
type NormalizedVertex = { x: float; y: float }

/// The kind of a Block
type BlockType =
    | Unknown = 0
    | FlowingText = 1
    | HeadingText = 2
    | PullOutText = 3
    | Equation = 4
    | InlineEquation = 5
    | Table = 6
    | VerticalText = 7
    | CaptionText = 8
    | FlowingImage = 9
    | HeadingImage = 10
    | PullOutImage = 11
    | HorizontalLine = 12
    | VerticalLine = 13
    | Noise = 14
    | Count = 15

/// Polygon described by four points
type BoundingPoly =
    { vertices: Vertex array
      normalizedVertices: NormalizedVertex array }

/// Lowest structure of a Page
type Symbol =
    { property: TextProperty
      boundingBox: BoundingPoly
      confidence: single
      text: string }

/// Second-lowest structure of a Page
type Word =
    { property: TextProperty
      boundingBox: BoundingPoly
      symbols: Symbol array
      confidence: single
      text: string }

/// Third-lowest structure of a Page
type Paragraph =
    { property: TextProperty
      boundingBox: BoundingPoly
      words: Word array
      confidence: single
      text: string }

/// Fourth-lowest structure on a Page
type Block =
    { property: TextProperty
      boundingBox: BoundingPoly
      paragraphs: Paragraph array
      blockType: BlockType
      text: string
      confidence: float }

/// Page object. Like GCV Page except with a (not serialized) pageNumber field
type Page =
    { property: TextProperty option
      [<JsonIgnore>]
      pageNumber: int
      width: int
      height: int
      blocks: Block array }

/// Top-level object. One TextAnnotation per-page
type TextAnnotation =
    { // Zero or one page per TextAnnotation
      pages: Page array
      // Page text. Required by SpatialString load
      text: string }
