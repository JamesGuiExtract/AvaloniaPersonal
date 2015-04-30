//--------------------------------------------------------------------------------------------------
// Script commands specific for RutgersXMLFix
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Removes excess FullText nodes from XML created using the 'Convert VOA to XML' FAM task.
//
// Usage:   RutgersXMLFix <SourceDocName>
//--------------------------------------------------------------------------------------------------

function main(args) {
  var xmlFile = fso.getFile(args[0]).Path;
  var xmlContents = fso.OpenTextFile(xmlFile, 1);

  // Remove the FullText tags and save the edited text.
  writeText(xmlFile, xmlContents.ReadAll().replace(/<FullText>N\/A<\/FullText>/g, "").replace(/<\/?FullText>/g, ""));
}
