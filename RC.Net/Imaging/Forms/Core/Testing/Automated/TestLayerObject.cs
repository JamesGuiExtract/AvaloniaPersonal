using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Extract.Imaging.Forms.Test
{
    public partial class TestImageViewer
    {
        /// <summary>
        /// Tests exception thrown when trying to link an object to itself.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TestAddLinkThrowsExceptionIfLinkingToSelf()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Create a highlight
            Highlight highlight = new Highlight(imageViewer, "Test",
                new RasterZone(10, 400, 500, 400, 200, 1));

            // Check that an exception is thrown when trying to link an object to itself
            Assert.Throws(typeof(ExtractException),
                delegate() { highlight.AddLink(highlight); });
        }

        /// <summary>
        /// Tests exception thrown when trying to link an object to a null object.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TestAddLinkThrowsExceptionIfLinkingToNull()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Create a highlight
            Highlight highlight = new Highlight(imageViewer, "Test",
                new RasterZone(10, 400, 500, 400, 200, 1));

            // Check that an exception is thrown when trying to link an object to null
            Assert.Throws(typeof(ExtractException),
                delegate() { highlight.AddLink(null); });
        }

        /// <summary>
        /// Tests adding a link between two layer objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TestAddLinkTwoObjects()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Create two highlights, one on page 1 and one on page 2
            Highlight highlight1 = new Highlight(imageViewer, "Test",
                new RasterZone(10, 400, 500, 400, 200, 1));
            Highlight highlight2 = new Highlight(imageViewer, "Test",
                new RasterZone(10, 400, 500, 400, 200, 2));

            // Link the highlights
            highlight1.AddLink(highlight2);

            // Check that the highlights are linked to each other A <-> B
            Assert.That(highlight1.NextLink.Id == highlight2.Id
                && highlight2.PreviousLink.Id == highlight1.Id);
        }

        /// <summary>
        /// Tests adding a link between three layer objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TestAddLinkThreeObjects()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Create three highlights, one on page 1, one on page 2, and one on page 3
            Highlight highlight1 = new Highlight(imageViewer, "Test",
                new RasterZone(10, 400, 500, 400, 200, 1));
            Highlight highlight2 = new Highlight(imageViewer, "Test",
                new RasterZone(10, 400, 500, 400, 200, 2));
            Highlight highlight3 = new Highlight(imageViewer, "Test",
                new RasterZone(10, 400, 500, 400, 200, 3));

            // Link the highlights
            highlight1.AddLink(highlight2);
            highlight2.AddLink(highlight3);

            // Check that the highlights are linked A <-> B <-> C
            Assert.That(highlight1.NextLink.Id == highlight2.Id
                && highlight2.PreviousLink.Id == highlight1.Id
                && highlight2.NextLink.Id == highlight3.Id
                && highlight3.PreviousLink.Id == highlight2.Id);
        }

        /// <summary>
        /// Tests whether the next and previous links are modified correctly when
        /// linking an object in the middle of already linked objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TestAddLinkInMiddleOfChain()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            Highlight[] highlights = new Highlight[4];
            for(int i=0; i < highlights.Length; i++)
            {
                highlights[i] = 
                    new Highlight(imageViewer, "Test", new RasterZone(10,400,500,400,200,i));
            }

            // Link the first to the second and the second to the fourth
            // A <-> B <-> D
            highlights[0].AddLink(highlights[1]);
            highlights[1].AddLink(highlights[3]);

            // Console output for debugging purposes
            Console.WriteLine("A <-> B <-> D");
            for(int i=0; i < highlights.Length; i++)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Highlight {0} Id:{1} Next:{2} Previous:{3}", new object[] {i, highlights[i].Id,
                    highlights[i].NextLink != null ? highlights[i].NextLink.Id : 0,
                    highlights[i].PreviousLink != null ? highlights[i].PreviousLink.Id : 0 }));
            }

            // Ensure that the links are as expected
            // A <-> B <-> D
            if (highlights[0].NextLink.Id != highlights[1].Id
                || highlights[1].PreviousLink.Id != highlights[0].Id
                || highlights[1].NextLink.Id != highlights[3].Id
                || highlights[3].PreviousLink.Id != highlights[1].Id)
            {
                throw new ExtractException("ELI22533", "Highlight links are not correct!");
            }

            // Now add the middle link to highlight0
            // Result should be A <-> B <-> C <-> D
            highlights[0].AddLink(highlights[2]);

            // Console output for debugging purposes
            Console.WriteLine("A <-> B <-> C <-> D");
            for(int i=0; i < highlights.Length; i++)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Highlight {0} Id:{1} Next:{2} Previous:{3}", new object[] {i, highlights[i].Id,
                    highlights[i].NextLink != null ? highlights[i].NextLink.Id : 0,
                    highlights[i].PreviousLink != null ? highlights[i].PreviousLink.Id : 0 }));
            }

            Assert.That(highlights[0].NextLink.Id == highlights[1].Id
                        && highlights[1].PreviousLink.Id == highlights[0].Id
                        && highlights[1].NextLink.Id == highlights[2].Id
                        && highlights[2].PreviousLink.Id == highlights[1].Id
                        && highlights[2].NextLink.Id == highlights[3].Id
                        && highlights[3].PreviousLink.Id == highlights[2].Id);
        }

        /// <summary>
        /// Tests whether the next and previous links are modified correctly when
        /// linking an object to the end of already linked objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TestAddLinkAtEndOfChain()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            Highlight[] highlights = new Highlight[4];
            for(int i=0; i < highlights.Length; i++)
            {
                highlights[i] = 
                    new Highlight(imageViewer, "Test", new RasterZone(10,400,500,400,200,i));
            }

            // Link the first to the second and the second to the third
            // A <-> B <-> C
            highlights[0].AddLink(highlights[1]);
            highlights[1].AddLink(highlights[2]);

            // Console output for debugging purposes
            Console.WriteLine("A <-> B <-> C");
            for(int i=0; i < highlights.Length; i++)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Highlight {0} Id:{1} Next:{2} Previous:{3}", new object[] {i, highlights[i].Id,
                    highlights[i].NextLink != null ? highlights[i].NextLink.Id : 0,
                    highlights[i].PreviousLink != null ? highlights[i].PreviousLink.Id : 0 }));
            }

            // Ensure that the links are as expected
            // A <-> B <-> C
            if (highlights[0].NextLink.Id != highlights[1].Id
                || highlights[1].PreviousLink.Id != highlights[0].Id
                || highlights[1].NextLink.Id != highlights[2].Id
                || highlights[2].PreviousLink.Id != highlights[1].Id)
            {
                throw new ExtractException("ELI22528", "Highlight links are not correct!");
            }

            // Now add the end link to highlight0
            // Result should be A <-> B <-> C <-> D
            highlights[0].AddLink(highlights[3]);

            // Console output for debugging purposes
            Console.WriteLine("A <-> B <-> C <-> D");
            for(int i=0; i < highlights.Length; i++)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Highlight {0} Id:{1} Next:{2} Previous:{3}", new object[] {i, highlights[i].Id,
                    highlights[i].NextLink != null ? highlights[i].NextLink.Id : 0,
                    highlights[i].PreviousLink != null ? highlights[i].PreviousLink.Id : 0 }));
            }

            Assert.That(highlights[0].NextLink.Id == highlights[1].Id
                        && highlights[1].PreviousLink.Id == highlights[0].Id
                        && highlights[1].NextLink.Id == highlights[2].Id
                        && highlights[2].PreviousLink.Id == highlights[1].Id
                        && highlights[2].NextLink.Id == highlights[3].Id
                        && highlights[3].PreviousLink.Id == highlights[2].Id);
        }

        /// <summary>
        /// Tests whether the next and previous links are modified correctly when
        /// linking an object to the beginning of already linked objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TestAddLinkAtBeginningOfChain1()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            Highlight[] highlights = new Highlight[4];
            for(int i=0; i < highlights.Length; i++)
            {
                highlights[i] = 
                    new Highlight(imageViewer, "Test", new RasterZone(10,400,500,400,200,i));
            }

            // Link the second to the third and the third to the fourth
            // B <-> C <-> D
            highlights[1].AddLink(highlights[2]);
            highlights[2].AddLink(highlights[3]);

            // Console output for debugging purposes
            Console.WriteLine("B <-> C <-> D");
            for(int i=0; i < highlights.Length; i++)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Highlight {0} Id:{1} Next:{2} Previous:{3}", new object[] {i, highlights[i].Id,
                    highlights[i].NextLink != null ? highlights[i].NextLink.Id : 0,
                    highlights[i].PreviousLink != null ? highlights[i].PreviousLink.Id : 0 }));
            }

            // Ensure that the links are as expected
            // B <-> C <-> D
            if (highlights[1].NextLink.Id != highlights[2].Id
                || highlights[2].PreviousLink.Id != highlights[1].Id
                || highlights[2].NextLink.Id != highlights[3].Id
                || highlights[3].PreviousLink.Id != highlights[2].Id)
            {
                throw new ExtractException("ELI22529", "Highlight links are not correct!");
            }

            // Now add the beginning link to highlight3
            // Result should be A <-> B <-> C <-> D
            highlights[3].AddLink(highlights[0]);

            // Console output for debugging purposes
            Console.WriteLine("A <-> B <-> C <-> D");
            for(int i=0; i < highlights.Length; i++)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Highlight {0} Id:{1} Next:{2} Previous:{3}", new object[] {i, highlights[i].Id,
                    highlights[i].NextLink != null ? highlights[i].NextLink.Id : 0,
                    highlights[i].PreviousLink != null ? highlights[i].PreviousLink.Id : 0 }));
            }

            Assert.That(highlights[0].NextLink.Id == highlights[1].Id
                        && highlights[1].PreviousLink.Id == highlights[0].Id
                        && highlights[1].NextLink.Id == highlights[2].Id
                        && highlights[2].PreviousLink.Id == highlights[1].Id
                        && highlights[2].NextLink.Id == highlights[3].Id
                        && highlights[3].PreviousLink.Id == highlights[2].Id);
        }

        /// <summary>
        /// Tests whether the next and previous links are modified correctly when
        /// linking an object to the beginning of already linked objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TestAddLinkAtBeginningOfChain2()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            Highlight[] highlights = new Highlight[4];
            for(int i=0; i < highlights.Length; i++)
            {
                highlights[i] = 
                    new Highlight(imageViewer, "Test", new RasterZone(10,400,500,400,200,i));
            }

            // Link the second to the third and the third to the fourth
            // B <-> C <-> D
            highlights[1].AddLink(highlights[2]);
            highlights[2].AddLink(highlights[3]);

            // Console output for debugging purposes
            Console.WriteLine("B <-> C <-> D");
            for(int i=0; i < highlights.Length; i++)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Highlight {0} Id:{1} Next:{2} Previous:{3}", new object[] {i, highlights[i].Id,
                    highlights[i].NextLink != null ? highlights[i].NextLink.Id : 0,
                    highlights[i].PreviousLink != null ? highlights[i].PreviousLink.Id : 0 }));
            }

            // Ensure that the links are as expected
            // B <-> C <-> D
            if (highlights[1].NextLink.Id != highlights[2].Id
                || highlights[2].PreviousLink.Id != highlights[1].Id
                || highlights[2].NextLink.Id != highlights[3].Id
                || highlights[3].PreviousLink.Id != highlights[2].Id)
            {
                throw new ExtractException("ELI22539", "Highlight links are not correct!");
            }

            // Now add the beginning link to highlight3
            // Result should be A <-> B <-> C <-> D
            highlights[0].AddLink(highlights[3]);

            // Console output for debugging purposes
            Console.WriteLine("A <-> B <-> C <-> D");
            for(int i=0; i < highlights.Length; i++)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Highlight {0} Id:{1} Next:{2} Previous:{3}", new object[] {i, highlights[i].Id,
                    highlights[i].NextLink != null ? highlights[i].NextLink.Id : 0,
                    highlights[i].PreviousLink != null ? highlights[i].PreviousLink.Id : 0 }));
            }

            Assert.That(highlights[0].NextLink.Id == highlights[1].Id
                        && highlights[1].PreviousLink.Id == highlights[0].Id
                        && highlights[1].NextLink.Id == highlights[2].Id
                        && highlights[2].PreviousLink.Id == highlights[1].Id
                        && highlights[2].NextLink.Id == highlights[3].Id
                        && highlights[3].PreviousLink.Id == highlights[2].Id);
        }

        /// <summary>
        /// Tests whether the next and previous links are modified correctly when
        /// linking an object that has its own links to an already linked chain
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TestAddLinkedChainToOtherChain()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            Highlight[] highlights = new Highlight[4];
            for(int i=0; i < highlights.Length; i++)
            {
                highlights[i] = 
                    new Highlight(imageViewer, "Test", new RasterZone(10,400,500,400,200,i));
            }

            // Link the first to the third and the second to the fourth
            // A <-> C  and B <-> D
            highlights[0].AddLink(highlights[2]);
            highlights[1].AddLink(highlights[3]);

            // Console output for debugging purposes
            Console.WriteLine("A <-> C and B <-> D");
            for(int i=0; i < highlights.Length; i++)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Highlight {0} Id:{1} Next:{2} Previous:{3}", new object[] {i, highlights[i].Id,
                    highlights[i].NextLink != null ? highlights[i].NextLink.Id : 0,
                    highlights[i].PreviousLink != null ? highlights[i].PreviousLink.Id : 0 }));
            }

            // Ensure that the links are as expected
            // A <-> C and B <-> D
            if (highlights[0].NextLink.Id != highlights[2].Id
                || highlights[2].PreviousLink.Id != highlights[0].Id
                || highlights[1].NextLink.Id != highlights[3].Id
                || highlights[3].PreviousLink.Id != highlights[1].Id)
            {
                throw new ExtractException("ELI22559", "Highlight links are not correct!");
            }

            // Now add the beginning link to highlight3
            // Result should be A <-> B <-> C <-> D
            highlights[0].AddLink(highlights[3]);

            // Console output for debugging purposes
            Console.WriteLine("A <-> B <-> C <-> D");
            for(int i=0; i < highlights.Length; i++)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Highlight {0} Id:{1} Next:{2} Previous:{3}", new object[] {i, highlights[i].Id,
                    highlights[i].NextLink != null ? highlights[i].NextLink.Id : 0,
                    highlights[i].PreviousLink != null ? highlights[i].PreviousLink.Id : 0 }));
            }

            Assert.That(highlights[0].NextLink.Id == highlights[1].Id
                        && highlights[1].PreviousLink.Id == highlights[0].Id
                        && highlights[1].NextLink.Id == highlights[2].Id
                        && highlights[2].PreviousLink.Id == highlights[1].Id
                        && highlights[2].NextLink.Id == highlights[3].Id
                        && highlights[3].PreviousLink.Id == highlights[2].Id);
        }

        /// <summary>
        /// Tests removing the links on a <see cref="LayerObject"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TestRemovingLink()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Create three highlights, one on page 1, one on page 2, and one on page 3
            Highlight highlight1 = new Highlight(imageViewer, "Test",
                new RasterZone(10, 400, 500, 400, 200, 1));
            Highlight highlight2 = new Highlight(imageViewer, "Test",
                new RasterZone(10, 400, 500, 400, 200, 2));
            Highlight highlight3 = new Highlight(imageViewer, "Test",
                new RasterZone(10, 400, 500, 400, 200, 3));

            // Link the highlights
            highlight1.AddLink(highlight2);
            highlight2.AddLink(highlight3);

            // Ensure that the highlights are linked as expected A <-> B <-> C
            if (highlight1.NextLink.Id != highlight2.Id
                || highlight2.PreviousLink.Id != highlight1.Id
                || highlight2.NextLink.Id != highlight3.Id
                || highlight3.PreviousLink.Id != highlight2.Id)
            {
                throw new ExtractException("ELI22540", "Highlight links are not correct!");
            }

            // Remove the links from highlight2
            highlight2.RemoveLinks();

            // Check that highlights 1 and 3 are linked and highlight2 has no links
            Assert.That(highlight1.NextLink.Id == highlight3.Id
                        && highlight3.PreviousLink.Id == highlight1.Id
                        && !highlight2.IsLinked);
        }
    }
}
