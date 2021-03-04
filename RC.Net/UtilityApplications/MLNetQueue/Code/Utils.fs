module Utils

open System.IO

module FsPickler =
  open MBrace.FsPickler
  let serializer = BinarySerializer();

  let toBinFile (fileName: string) (x: 'a) =
      use stream = new FileStream(fileName, FileMode.Create, FileAccess.Write)
      serializer.Serialize(stream, x)

  let ofBinFile<'a> (fileName: string): 'a = 
    use stream = new FileStream(fileName, FileMode.Open, FileAccess.Read)
    serializer.Deserialize<'a>(stream)
(************************************************************************************************************************)


