using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Extract.FileActionManager.FileProcessors
{
    partial class PaginationTaskForm
    {
        /// <summary>
        /// Represents the saved state of the <see cref="PaginationTaskForm"/> user interface.
        /// <para><b>Note</b></para>
        /// Though the methods SaveState and RestoreSavedState allow for explicitly loading and saving
        /// the UI state information, this class will automatically save the form's state when the
        /// managed <see cref="Form"/>'s <see cref="Form.Closing"/> event is raised and restore the
        /// saved state when the managed <see cref="Form"/>'s <see cref="Form.Load"/> event is raised.
        /// </summary>
        class FormStateManager : Extract.Utilities.Forms.FormStateManager
        {
            #region Fields

            /// <summary>
            /// The <see cref="PaginationTaskForm"/> for which the UI state is being managed.
            /// </summary>
            PaginationTaskForm _paginationTaskForm;

            /// <summary>
            /// The distance of the splitter from the top of the data window in client pixels.
            /// </summary>
            int _splitterDistance;

            #endregion Fields

            #region Constructors

            /// <overloads>
            /// Initializes a new instance of the <see cref="FormStateManager"/> class.
            /// </overloads>
            /// <summary>
            /// Initializes a new instance of the <see cref="FormStateManager"/> class.
            /// <para><b>Note</b></para>
            /// <see cref="FormStateManager"/> should not be created or used in design time.
            /// </summary>
            /// <param name="paginationTaskForm">The <see cref="Form"/> whose state is to be managed.
            /// </param>
            /// <param name="persistenceFileName">The name of the file to which form properties will be
            /// maintained.</param>
            /// <param name="mutexName">Name for the mutex used to serialize persistence of the
            /// control and form layout.</param>
            /// <param name="manageToolStrips">If <see langword="true"/>, the form's
            /// <see cref="ToolStrip"/> will be persisted.</param>
            /// <param name="fullScreenTabText">If not <see langword="null"/> or empty, an
            /// AutoHideScreenTab will be displayed with the provided text that, if clicked, will exit
            /// full screen mode. If <see langword="null"/> full screen mode will not be supported and
            /// the persistence xml file will not contain the FullScreen attribute.
            /// </param>
            /// <throws><see cref="ExtractException"/> if instantiated at design-time.</throws>
            public FormStateManager(PaginationTaskForm paginationTaskForm, string persistenceFileName,
                string mutexName, bool manageToolStrips, string fullScreenTabText)
                : base(paginationTaskForm, persistenceFileName, mutexName, manageToolStrips,
                    fullScreenTabText)
            {
                try
                {
                    _paginationTaskForm = paginationTaskForm;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI40073", ex);
                }
            }

            #endregion Constructors

            #region Overrides

            /// <summary>
            /// Saves the <see cref="PaginationTaskForm"/>'s current UI state to disk.
            /// </summary>
            public override void SaveState()
            {
                try
                {
                    _splitterDistance = _paginationTaskForm._splitContainer.SplitterDistance;

                    base.SaveState();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI40074", ex);
                }
            }

            /// <summary>
            /// Restores the state of the user interface of the <see cref="PaginationTaskForm"/>
            /// using this <see cref="FormStateManager"/> instance's current properties.
            /// </summary>
            public override void RestoreState()
            {
                try
                {
                    base.RestoreState();

                    if (_splitterDistance > 0)
                    {
                        _paginationTaskForm._splitContainer.SplitterDistance = _splitterDistance;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.AsExtractException("ELI40075", ex);
                }
            }

            /// <summary>
            /// Creates an <see cref="XmlNode"/> that represents the managed UI properties of the
            /// <see cref="PaginationTaskForm"/>.
            /// </summary>
            /// <param name="document">The <see cref="XmlDocument"/> to use when creating the
            /// <see cref="XmlNode"/>.</param>
            /// <returns>An  <see cref="XmlNode"/>that represents the managed UI properties of the
            /// <see cref="PaginationTaskForm"/>.</returns>
            [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes",
                MessageId = "System.Xml.XmlNode")]
            public override XmlNode ToXmlNode(XmlDocument document)
            {
                try
                {
                    XmlNode parentNode = base.ToXmlNode(document);

                    // Get the data window as XML
                    XmlElement dataWindow = document.CreateElement("DataWindow");

                    // Get the splitter as XML
                    XmlElement splitter = document.CreateElement("Splitter");
                    splitter.SetAttribute("Distance", _splitterDistance.ToString(
                        CultureInfo.InvariantCulture));

                    // Append the XML together
                    dataWindow.AppendChild(splitter);
                    parentNode.AppendChild(dataWindow);

                    return parentNode;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI40076", ex);
                }
            }

            /// <summary>
            /// Loads the managed UI properties from the specified XML.
            /// </summary>
            /// <param name="xmlSource">The XML from which the managed UI properties are loaded.</param>
            /// <returns>An <see cref="IXPathNavigable"/> for the XML relating to the
            /// <see cref="FormStateManager"/> class's managed properties.</returns>
            public override IXPathNavigable LoadFromXml(IXPathNavigable xmlSource)
            {
                try
                {
                    IXPathNavigable formXml = base.LoadFromXml(xmlSource);

                    // Get the form memento
                    XPathNavigator xmlNavigator = formXml.CreateNavigator();

                    XPathNavigator dataWindow = xmlNavigator.SelectSingleNode("DataWindow");

                    // Get the splitter distance
                    if (dataWindow != null)
                    {
                        XPathNavigator splitter = dataWindow.SelectSingleNode("Splitter");
                        if (splitter != null)
                        {
                            _splitterDistance = GetAttribute<int>(splitter, "Distance",
                                _paginationTaskForm._splitContainer.SplitterDistance);
                        }
                    }

                    return formXml;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI40077", ex);
                }
            }

            #endregion Overrides
        }
    }
}