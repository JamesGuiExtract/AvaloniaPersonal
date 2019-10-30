using Extract.Utilities;
using System;
using System.IO;
using System.Linq;
using Tesseract;

namespace Extract.AttributeFinder
{
    public class TableFinder
    {
        static readonly string _TESSDATA_DIR = Path.Combine(FileSystemMethods.CommonComponentsPath, "tessdata");
        private TesseractEngine _engine;

        public TableFinder()
        {
            _engine = new TesseractEngine(_TESSDATA_DIR, "eng", EngineMode.Default);
            _engine.DefaultPageSegMode = PageSegMode.SingleColumn;
            _engine.SetVariable("textord_tablefind_recognize_tables", true);
        }

        public void GetTables(string imagePath)
        {
            using (var images = PixArray.LoadMultiPageTiffFromFile(imagePath))
            {
                foreach (var (img, i) in images.Select((x, i) => (x, i)))
                {
                    if (i != 5)
                    {
                        continue;
                    }

                    using (var page = _engine.Process(img))
                    {
                        //var text = page.GetText();
                        //Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

                        //Console.WriteLine("Text (GetText): \r\n{0}", text);
                        Console.WriteLine("Text (iterator):");
                        using (var iter = page.GetIterator())
                        {
                            iter.Begin();

                            do
                            {
                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                            if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                            {
                                                Console.WriteLine($"<BLOCK:{iter.BlockType}>");
                                            }

                                            Console.Write(iter.GetText(PageIteratorLevel.Word));
                                            Console.Write(" ");

                                            if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                                            {
                                                Console.WriteLine();
                                            }
                                        } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                                        if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                                        {
                                            Console.WriteLine();
                                        }
                                    } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                                } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                            } while (iter.Next(PageIteratorLevel.Block));
                        }
                    }
                }
            }
        }
    }
}
