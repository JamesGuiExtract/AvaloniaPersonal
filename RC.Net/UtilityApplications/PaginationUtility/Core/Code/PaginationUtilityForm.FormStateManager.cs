﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using TD.SandDock;

namespace Extract.UtilityApplications.PaginationUtility
{
    partial class PaginationUtilityForm
    {
        /// <summary>
        /// Represents the saved state of the <see cref="PaginationUtilityForm"/> user interface.
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
            /// The <see cref="PaginationUtilityForm"/> for which the UI state is being managed.
            /// </summary>
            PaginationUtilityForm _paginationForm;

            /// <summary>
            /// The distance of the splitter from the top of the data window in client pixels.
            /// </summary>
            int _splitterDistance;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="FormStateManager"/> class.
            /// </summary>
            /// <param name="paginationForm">The <see cref="PaginationUtilityForm"/> whose state is
            /// to be managed.</param>
            /// <param name="persistenceFileName">The name of the file to which form properties will
            /// be maintained.</param>
            /// <param name="mutexName">Name for the mutex used to serialize persistance of the
            /// control and form layout.</param>
            /// <param name="sandDockManager">If specified, this <see cref="SandDockManager"/>'s
            /// state info will be persisted.</param>
            /// <param name="fullScreenTabText">If not <see langword="null"/>, an
            /// <see cref="Extract.Utilities.Forms.AutoHideScreenTab"/> will be displayed with the
            /// provided text that, if clicked, will exit full screen mode.</param>
            public FormStateManager(PaginationUtilityForm paginationForm,
                string persistenceFileName, string mutexName, SandDockManager sandDockManager,
                string fullScreenTabText)
                : base(paginationForm, persistenceFileName, mutexName, sandDockManager, true,
                    fullScreenTabText)
            {
                try
                {
                    _paginationForm = paginationForm;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI35632", ex);
                }
            }

            #endregion Constructors

            #region Overrides

            /// <summary>
            /// Saves the <see cref="PaginationUtilityForm"/>'s current UI state to disk.
            /// </summary>
            public override void SaveState()
            {
                try
                {
                    _splitterDistance = _paginationForm._splitContainer.SplitterDistance;

                    base.SaveState();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI35633", ex);
                }
            }

            /// <summary>
            /// Restores the state of the user interface of the <see cref="PaginationUtilityForm"/>
            /// using this <see cref="FormStateManager"/> instance's current properties.
            /// </summary>
            public override void RestoreState()
            {
                try
                {
                    base.RestoreState();

                    if (_splitterDistance > 0)
                    {
                        _paginationForm._splitContainer.SplitterDistance = _splitterDistance;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.AsExtractException("ELI35634", ex);
                }
            }

            /// <summary>
            /// Creates an <see cref="XmlNode"/> that represents the managed UI properties of the
            /// <see cref="PaginationUtilityForm"/>.
            /// </summary>
            /// <param name="document">The <see cref="XmlDocument"/> to use when creating the
            /// <see cref="XmlNode"/>.</param>
            /// <returns>An  <see cref="XmlNode"/>that represents the managed UI properties of the
            /// <see cref="PaginationUtilityForm"/>.</returns>
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
                    splitter.SetAttribute("Distance", _splitterDistance.ToString(CultureInfo.InvariantCulture));

                    // Append the XML together
                    dataWindow.AppendChild(splitter);
                    parentNode.AppendChild(dataWindow);

                    return parentNode;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI35635", ex);
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
                                _paginationForm._splitContainer.SplitterDistance);
                        }
                    }

                    return formXml;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI35636", ex);
                }
            }

            #endregion Overrides
        }
    }
}
