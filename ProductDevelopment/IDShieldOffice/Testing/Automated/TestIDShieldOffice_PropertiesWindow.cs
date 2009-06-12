using Extract.Imaging.Forms;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using TD.SandDock;

namespace IDShieldOffice.Test
{
    public partial class TestIDShieldOffice
    {
        #region Object Properties Window

        #region Bates Numbers

        /// <summary>
        /// Test the ability to display all properties of Bates numbers in property grid.
        /// </summary>
        [Test]
        [Category("Automated")]
        public void Automated_PropertiesWindowDisplaysBatesNumber()
        {
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            imageViewer.OpenImage(_testImages.GetFile(_FIND_TEXT_TEST), false);

            // Ensure the image opened
            Assert.That(imageViewer.IsImageAvailable, "Could not open test image.");

            // Apply Bates #
            _idShieldOfficeForm.ApplyBatesNumbers();
            
            // Select Bates Number object on the first page.
            TextLayerObject batesNumber = 
                (TextLayerObject) imageViewer.LayerObjects.GetSortedCollection()[0];
            batesNumber.Selected = true;

            // Get the grid items
            PropertyGrid propertyGrid = FormMethods.GetFormComponent<PropertyGrid>(_idShieldOfficeForm, "PropertyGrid");
            GridItemCollection gridItems = GetGridItems(propertyGrid);

            // Confirm that value of AnchorAlignment in the properties windows is RightTop.
            AssertGridItemMatch(gridItems, "AnchorAlignment", "RightTop");
            
            // Confirm that value of font in the properties windows is Arial, 30pt, style=Bold.
            AssertGridItemMatch(gridItems, "Font", "Arial, 30pt, style=Bold");
            
            // Confirm that value of IsLinked in the properties windows is True.
            AssertGridItemMatch(gridItems, "IsLinked", "True");

            // Confirm that value of PageNumber in the properties windows is 1.
            AssertGridItemMatch(gridItems, "PageNumber", "1");

            // Confirm that value of Text in the properties windows is the same as in the page.
            AssertGridItemMatch(gridItems, "Text", batesNumber.Text);

            // Change Bates number's font.
            batesNumber.Font = new Font("Times New Roman", 20F);

            // Confirm that font of Bates Numbers is changed.
            AssertGridItemMatch(gridItems, "Font", "Times New Roman, 20pt");
        }

        #endregion Bates Numbers

        #region Helper Methods

        /// <summary>
        /// Gets the grid item collection from the specified <see cref="PropertyGrid"/>.
        /// </summary>
        /// <param name="propertyGrid">The property grid from which to retrieve grid items.</param>
        /// <returns>The grid item collection from <paramref name="propertyGrid"/>.</returns>
        static GridItemCollection GetGridItems(PropertyGrid propertyGrid)
        {
            FieldInfo fieldInfo = typeof(PropertyGrid).GetField("currentPropEntries",
                BindingFlags.Instance | BindingFlags.NonPublic);
            GridItemCollection gridItems = fieldInfo.GetValue(propertyGrid) as GridItemCollection;
            Assert.That(gridItems != null, "Could not get grid items");
            return gridItems;
        }

        /// <summary>
        /// Checks whether the grid items contains the specified property and corresponding value.
        /// </summary>
        /// <param name="gridItems">The grid items to search.</param>
        /// <param name="propertyName">The name of the property for which to search.</param>
        /// <param name="value">The value of the property to be considered a success.</param>
        static void AssertGridItemMatch(GridItemCollection gridItems, string propertyName,
            string value)
        {
            // Check for the label that matches the property
            foreach (GridItem gridItem in gridItems)
            {
                foreach (GridItem subgridItem in gridItem.GridItems)
                {
                    if (subgridItem.Label == propertyName)
                    {
                        Assert.That(GetTextValue(subgridItem) == value,
                            "Incorrect \"" + propertyName + "\" value.");
                        return;
                    }
                }
            }

            // If we reached this point, the search failed.
            Assert.That(false, "Unable to find \"" + propertyName + "\".");
        }

        /// <summary>
        /// Gets the text of value cell of the specified grid item.
        /// </summary>
        /// <param name="gridItem">The grid item from which to retrive the text.</param>
        /// <returns>The text of value cell of <paramref name="gridItem"/>.</returns>
        static string GetTextValue(GridItem gridItem)
        {
            // Get the first derived type (it is internal, so it must be accessed indirectly).
            Type derivedType = GetDerivedType(gridItem);
            Assert.That(derivedType != null, "Could not get text value from property grid.");

            // Invoke the GetPropertyTextValue
            MethodInfo methodInfo = derivedType.GetMethod("GetPropertyTextValue", new Type[0]);
            return (string)methodInfo.Invoke(gridItem, null);
        }

        /// <summary>
        /// Gets the immediately derived type of the current instance.
        /// </summary>
        /// <typeparam name="T">The type from which to find the derived type.</typeparam>
        /// <param name="instance">The instance from which to find the derived type.</param>
        /// <returns>The first derived class of <paramref name="instance"/>.</returns>
        static Type GetDerivedType<T>(T instance)
        {
            // Get the lowest level derived type of this instance
            Type derivedType = instance.GetType();
            if (derivedType == typeof(T))
            {
                return null;
            }

            // Iterate through each parent type until the we are 
            // one derived class from the original instance.
            derivedType = derivedType.BaseType;
            while (derivedType != null && derivedType.BaseType != typeof(T))
            {
                derivedType = derivedType.BaseType;
            }

            // Return the found type
            return derivedType;
        }

        #endregion Helper Methods

        #endregion Object Properties Window
    }
}
