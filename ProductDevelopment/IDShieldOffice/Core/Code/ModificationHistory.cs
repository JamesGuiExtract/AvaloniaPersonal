using Extract;
using Extract.Encryption;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Rules;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IDShieldOffice
{
    /// <summary>
    /// Provides data for the <see cref="ModificationHistory.ModificationHistoryLoaded"/> event.
    /// </summary>
    internal class ModificationHistoryLoadedEventArgs : EventArgs
    {
    }

    /// <summary>
    /// Represents the changes made to a document across all ID Shield Office sessions.
    /// </summary>
    internal class ModificationHistory
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ModificationHistory).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The version number for the <see cref="ModificationHistory"/>.
        /// </summary>
        readonly int _version = 1;

        /// <summary>
        /// The image viewer associated with the <see cref="ModificationHistory"/>.
        /// </summary>
        readonly ImageViewer _imageViewer;

        /// <summary>
        /// The name of the ID Shield Office data file to load.
        /// </summary>
        string _idsoFile;

        /// <summary>
        /// An xml document that contains the nodes relevant to the 
        /// <see cref="ModificationHistory"/>.
        /// </summary>
        readonly XmlDocument _document = new XmlDocument();

        /// <summary>
        /// The original HistoricalObjects node.
        /// </summary>
        readonly XmlNode _originalHistoricalObjects;

        /// <summary>
        /// The original Sessions node.
        /// </summary>
        readonly XmlNode _originalSessions;

        /// <summary>
        /// The original CurrentObjects node.
        /// </summary>
        readonly XmlNode _originalCurrentObjects;

        /// <summary>
        /// The nodes from the original CurrentObjects that have been deleted.
        /// </summary>
        readonly XmlNode _originalDeleted;

        /// <summary>
        /// The nodes from the original CurrentObjects that have been modified
        /// </summary>
        readonly XmlNode _originalModified;

        /// <summary>
        /// The idAttribute of the current session.
        /// </summary>
        int _sessionId = 1;

        /// <summary>
        /// The start of the current session.
        /// </summary>
        DateTime _startTime = DateTime.Now;

        /// <summary>
        /// A collection of xml serializers for each layer object, keyed by type.
        /// </summary>
        readonly Dictionary<Type, XmlSerializer> _serializersByType = 
            new Dictionary<Type,XmlSerializer>();

        /// <summary>
        /// If <see langword="true"/> then currently loading an IDSO file, if
        /// <see langword="false"/> then not loading an IDSO file.
        /// </summary>
        bool _loadingIDSO;

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when the modification history has been loaded (when an IDSO file is loaded).
        /// </summary>
        internal event EventHandler<ModificationHistoryLoadedEventArgs> ModificationHistoryLoaded;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModificationHistory"/> class.
        /// </summary>
        public ModificationHistory(ImageViewer imageViewer)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23200",
                    _OBJECT_NAME);

                // Store the image viewer
                _imageViewer = imageViewer;

                // Handle image file changed events
                _imageViewer.ImageFileChanged += HandleImageFileChanged;

                // Create the root node
                XmlNode root = _document.CreateNode(XmlNodeType.Element, "IDShieldOfficeData", null);
                _document.AppendChild(root);

                // Create the relevant child nodes
                _originalHistoricalObjects = CreateNode("HistoricalObjects");
                _originalCurrentObjects = CreateNode("CurrentObjects");
                _originalSessions = CreateNode("Sessions");
                _originalDeleted = CreateNode("ObjectsDeleted");
                _originalModified = CreateNode("ObjectsModified");
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23201", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Creates a child node of root node of the <see cref="_document"/>.
        /// </summary>
        /// <param name="name">The name of the node to create.</param>
        /// <returns>The child node that was created.</returns>
        XmlNode CreateNode(string name)
        {
            // Create the node
            XmlNode node = _document.CreateNode(XmlNodeType.Element, name, null);

            // Append the node to the root node
            _document.FirstChild.AppendChild(node);

            // Return the result
            return node;
        }

        /// <summary>
        /// Resets the modification history.
        /// </summary>
        void Clear()
        {
            _originalHistoricalObjects.RemoveAll();
            _originalCurrentObjects.RemoveAll();
            _originalSessions.RemoveAll();
            _originalDeleted.RemoveAll();
            _originalModified.RemoveAll();
            _sessionId = 1;
            _startTime = DateTime.Now;
        }

        /// <summary>
        /// Saves the modification history to an ID Shield Office data file.
        /// </summary>
        public void Save()
        {
            try
            {
                // Write the encrypted string to the IDSO file
                ExtractEncryption.EncryptTextFile(ToXmlString(), _idsoFile, true, new MapLabel());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23032", ex);
            }
        }

        /// <summary>
        /// Converts the modification history to an xml string.
        /// </summary>
        public string ToXmlString()
        {
            try
            {
                // Get the following groups of layer objects:
                // (1) The current layer objects that have been modified since last session
                // (2) The current layer objects that have not been modified since the last session
                // (3) The current layer objects that have been created during this session
                LayerObjectsCollection currentModified;
                LayerObjectsCollection currentUnmodified;
                LayerObjectsCollection currentAdded;

                // Note this will also update the following groups of layer objects:
                // (1) The original CurrentObjects that have been deleted since last session
                // (2) The original CurrentObjects that have been modified since last session
                GetLayerObjectGroups(out currentModified, out currentUnmodified, out currentAdded);

                // Create a string writer for the unencrypted xml
                using (StringWriter stringWriter = new StringWriter(CultureInfo.CurrentCulture))
                {
                    // Prepare to write to the string
                    XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                    xmlWriter.Formatting = Formatting.Indented;
                    xmlWriter.Indentation = 4;
                    xmlWriter.WriteRaw("");

                    // Write the root node
                    xmlWriter.WriteStartElement("IDShieldOfficeData");
                    xmlWriter.WriteAttributeString("Version",
                        _version.ToString(CultureInfo.CurrentCulture));

                    // Write the historical objects
                    xmlWriter.WriteStartElement("HistoricalObjects");
                    _originalHistoricalObjects.WriteContentTo(xmlWriter);
                    _originalDeleted.WriteContentTo(xmlWriter);
                    _originalModified.WriteContentTo(xmlWriter);
                    xmlWriter.WriteEndElement();

                    // Write the current objects
                    xmlWriter.WriteStartElement("CurrentObjects");
                    WriteLayerObjects(xmlWriter, currentUnmodified);
                    WriteLayerObjects(xmlWriter, currentModified);
                    WriteLayerObjects(xmlWriter, currentAdded);
                    xmlWriter.WriteEndElement();

                    // Write the session information
                    xmlWriter.WriteStartElement("Sessions");
                    _originalSessions.WriteContentTo(xmlWriter);

                    // Write the current session
                    xmlWriter.WriteStartElement("Session");
                    xmlWriter.WriteAttributeString("Id",
                        _sessionId.ToString(CultureInfo.CurrentCulture));

                    // Write the session info
                    xmlWriter.WriteStartElement("SessionInfo");
                    xmlWriter.WriteElementString("User", Environment.UserName);
                    xmlWriter.WriteElementString("Computer", Environment.MachineName);
                    xmlWriter.WriteElementString("StartTime",
                        _startTime.ToString(CultureInfo.InvariantCulture));
                    xmlWriter.WriteElementString("EndTime",
                        DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    xmlWriter.WriteEndElement();

                    // Write the objects added
                    xmlWriter.WriteStartElement("ObjectsAdded");
                    WriteObjectList(xmlWriter, currentAdded);
                    xmlWriter.WriteEndElement();

                    // Write the objects modified
                    xmlWriter.WriteStartElement("ObjectsModified");
                    WriteObjectList(xmlWriter, currentModified);
                    xmlWriter.WriteEndElement();

                    // Write the objects deleted
                    xmlWriter.WriteStartElement("ObjectsDeleted");
                    WriteObjectList(xmlWriter, _originalDeleted);
                    xmlWriter.WriteEndElement();

                    // Close the xml writer
                    xmlWriter.Close();

                    // Return the xml string
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23033", ex);
            }
        }

        /// <summary>
        /// Loads the modification history from an ID Shield Office data file.
        /// </summary>
        void Load()
        {
            try
            {
                // Set the loading flag
                _loadingIDSO = true;

                // Decrypt the IDSO
                string xml = ExtractEncryption.DecryptTextFile(_idsoFile, Encoding.ASCII,
                    new MapLabel());

                // Open a string reader for the xml
                using (StringReader stringReader = new StringReader(xml))
                {
                    // Prepare to read the stream
                    XmlTextReader xmlReader = new XmlTextReader(stringReader, _document.NameTable);
                    xmlReader.WhitespaceHandling = WhitespaceHandling.Significant;
                    xmlReader.Normalization = true;
                    xmlReader.Read();

                    // Read the version number
                    if (xmlReader.Name != "IDShieldOfficeData")
                    {
                        throw new ExtractException("ELI22951", "Invalid ID Shield Office data file.");
                    }
                    int version =
                        Convert.ToInt32(xmlReader.GetAttribute("Version"), CultureInfo.CurrentCulture);
                    if (version > _version)
                    {
                        ExtractException ee =
                            new ExtractException("ELI22932", "Invalid ID Shield Office data file.");
                        ee.AddDebugData("Maximum version", _version, false);
                        ee.AddDebugData("IDSO version", version, false);
                        throw ee;
                    }
                    xmlReader.Read();

                    // Read the historical objects
                    ReadChildNodes(xmlReader, "HistoricalObjects", _originalHistoricalObjects);

                    // Read the current objects
                    ReadChildNodes(xmlReader, "CurrentObjects", _originalCurrentObjects);

                    // Read the session info
                    ReadChildNodes(xmlReader, "Sessions", _originalSessions);

                    // Check if a previous session was in the IDSO
                    XmlNode lastSession = _originalSessions.LastChild;
                    if (lastSession != null)
                    {
                        // Get the next session id
                        _sessionId = (int)GetNodeId(lastSession) + 1;
                    }

                    // Close the xml reader
                    xmlReader.Close();
                }

                // Add the CurrentObjects as layer objects to the image viewer's collection
                AddCurrentLayerObjects();

            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23382", ex);
            }
            finally
            {
                // Ensure the loading flag is set back to false
                _loadingIDSO = false;
            }

            // Raise the ModificationHistoryLoaded event
            OnModificationHistoryLoaded(new ModificationHistoryLoadedEventArgs());
        }

        /// <summary>
        /// Adds the <see cref="_originalCurrentObjects"/> as layer objects to the image viewer.
        /// </summary>
        void AddCurrentLayerObjects()
        {
            // Get the current layer objects
            LayerObjectsCollection layerObjects = _imageViewer.LayerObjects;

            // Add all the layer objects
            foreach (XmlNode node in _originalCurrentObjects.ChildNodes)
            {
                // Get the serializer for this layer object
                XmlSerializer serializer = GetSerializer(node.Name);

                // Get the xml node xmlReader for this layer object
                XmlNodeReader reader = new XmlNodeReader(node);

                // Create this layer object
                LayerObject layerObject = (LayerObject)serializer.Deserialize(reader);

                // Add the layer object to the image viewer
                layerObject.ImageViewer = _imageViewer;
                layerObjects.Add(layerObject);

                // Close the xml node xmlReader
                reader.Close();
            }

            // Make a second pass to link the layer objects together
            foreach (XmlNode layerObject in _originalCurrentObjects.ChildNodes)
            {
                // Check if this layer object is linked to a subsequent one
                XmlNode nextIdNode = layerObject.SelectSingleNode("NextObjectId");
                if (nextIdNode != null)
                {
                    // Get the ids of this layer object and the next layer object
                    long currentId = GetNodeId(layerObject);
                    long nextId = Convert.ToInt64(nextIdNode.FirstChild.Value, CultureInfo.CurrentCulture);

                    // Link the layer objects together
                    layerObjects[currentId].AddLink(layerObjects[nextId]);
                }
            }

            // Make a third pass to set all the dirty flags to false
            // Note: The second pass set linked layer objects dirty flags to true
            foreach (LayerObject layerObject in layerObjects)
            {
                layerObject.Dirty = false;
            }

            // Invalidate the image viewer to redraw the new layer objects. [IDSO #338]
            _imageViewer.Invalidate();
        }

        /// <summary>
        /// Gets the value of the Id attribute of the specifed node.
        /// </summary>
        /// <param name="node">The node to retrieve the Id attribute from</param>
        /// <returns>The value of the Id attribute of the specifed node.</returns>
        static long GetNodeId(XmlNode node)
        {
            XmlAttribute idAttribute = node.Attributes["Id"];
            return Convert.ToInt64(idAttribute.Value, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Reads the child nodes of the parent with the specified name into an XML document.
        /// </summary>
        /// <param name="reader">The stream from which to read the child nodes.</param>
        /// <param name="parent">The name of the parent node whose children should be read.</param>
        /// <param name="node">The node onto which the child nodes should be appended.</param>
        void ReadChildNodes(XmlReader reader, string parent, XmlNode node)
        {
            if (reader.Name != parent)
            {
                throw new ExtractException("ELI22933", "Invalid ID Shield Office data file.");
            }
            if (!reader.IsEmptyElement)
            {
                reader.Read();

                // Read each historical object
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    XmlNode child = _document.ReadNode(reader);
                    ExtractException.Assert("ELI29221", "Unrecognized node.", child != null);
                    node.AppendChild(child);
                }
            }
            reader.Read();
        }

        /// <summary>
        /// Gets three groups of layer objects and updates <see cref="_originalDeleted"/> and 
        /// <see cref="_originalModified"/>.
        /// </summary>
        /// <param name="currentModified">The current layer objects that have been modified 
        /// since last session.</param>
        /// <param name="currentUnmodified">The current layer objects that existed in and have not 
        /// been modified since the last session.</param>
        /// <param name="currentAdded">The current layer objects that have been created 
        /// during this session.</param>
        void GetLayerObjectGroups(out LayerObjectsCollection currentModified, 
            out LayerObjectsCollection currentUnmodified, out LayerObjectsCollection currentAdded)
        {
            // Split the original CurrentObjects into two categories:
            // 1) Original CurrentObjects that have been deleted
            // 2) Original CurrentObjects that have been modified
            _originalDeleted.RemoveAll();
            _originalModified.RemoveAll();

            // Split the current objects into three categories:
            // 1) Current objects that have been modified
            // 2) Current objects that have not been modified
            // 3) Current objects that have been created 
            // (optimize by assuming all layer objects are new)
            currentModified = new LayerObjectsCollection();
            currentUnmodified = new LayerObjectsCollection();
            currentAdded = GetSaveableLayerObjects();

            // Iterate through the original CurrentObjects 
            foreach(XmlNode original in _originalCurrentObjects.ChildNodes)
            {
                // Check if original layer object exists in the current collection
                LayerObject layerObject = TryGetLayerObjectFromNode(original);
                if (layerObject == null)
                {
                    // This layer object was deleted
                    _originalDeleted.AppendChild(original.Clone());
                }
                else 
                {
                    // This layer object existed in the previous session
                    currentAdded.Remove(layerObject);

                    // Check if this layer object was modified during this session
                    if (layerObject.Dirty)
                    {
                        _originalModified.AppendChild(original.Clone());
                        currentModified.Add(layerObject);
                    }
                    else
	                {
                        currentUnmodified.Add(layerObject);
	                }
                }
            }
        }

        /// <summary>
        /// Get all the layer objects that can be saved.
        /// </summary>
        /// <returns>All the layer objects that can be saved.</returns>
        LayerObjectsCollection GetSaveableLayerObjects()
        {
            // Iterate through all the layer objects
            LayerObjectsCollection saveable = new LayerObjectsCollection();
            foreach (LayerObject layerObject in _imageViewer.LayerObjects)
            {
                // Add this layer object if it is not search results
                if (!layerObject.Tags.Contains(RuleForm.SearchResultTag))
                {
                    saveable.Add(layerObject);
                }
            }

            // Return the result
            return saveable;
        }

        /// <summary>
        /// Gets the layer object that corresponds to the specified xml node if it exists.
        /// </summary>
        /// <param name="node">The xml representation of the desired layer object.</param>
        /// <returns>The layer object that corresponds to the specified xml node if it exists; 
        /// <see langword="null"/> if it doesn't exist.</returns>
        LayerObject TryGetLayerObjectFromNode(XmlNode node)
        {
            // Get the id of the layer object represented by the node
            long id = GetNodeId(node);

            // Retrieve the layer object from the current collection, if it exists
            return _imageViewer.LayerObjects.TryGetLayerObject(id);
        }

        /// <summary>
        /// Serializes the specified layer objects into the specified stream.
        /// </summary>
        /// <param name="writer">The stream in which to serialize <paramref name="layerObjects"/>.
        /// </param>
        /// <param name="layerObjects">The layer objects to serialize into 
        /// <paramref name="writer"/>.</param>
        void WriteLayerObjects(XmlWriter writer, IEnumerable<LayerObject> layerObjects)
        {
            foreach (LayerObject layerObject in layerObjects)
            {
                // Get the serializer for this layer object
                XmlSerializer serializer = GetSerializer(layerObject.GetType());

                // Write the layer object
                serializer.Serialize(writer, layerObject);
            }
        }

        /// <overloads>Gets the <see cref="XmlSerializer"/> that corresponds to the specified 
        /// layer object.</overloads>
        /// <summary>
        /// Gets the <see cref="XmlSerializer"/> that corresponds to the specified layer object.
        /// </summary>
        /// <param name="type">The type of the layer object corresponding to the desired 
        /// <see cref="XmlSerializer"/>.</param>
        /// <returns>The <see cref="XmlSerializer"/> that corresponds to the specified layer 
        /// object.</returns>
        XmlSerializer GetSerializer(Type type)
        {
            // Check if the type is in the collection
            XmlSerializer serializer;
            if (!_serializersByType.TryGetValue(type, out serializer))
            {
                // The serializer for this type hasn't been made yet. Make it now.
                serializer = new XmlSerializer(type);
                _serializersByType.Add(type, serializer);
            }

            // Return the found serializer
            return serializer;
        }

        /// <summary>
        /// Gets the <see cref="XmlSerializer"/> that corresponds to the layer object with the 
        /// specified type name.
        /// </summary>
        /// <param name="typeName">The name of the type of the layer object corresponding to the 
        /// desired <see cref="XmlSerializer"/>.</param>
        /// <returns>The <see cref="XmlSerializer"/> that corresponds to the layer object with the 
        /// specified type name.</returns>
        XmlSerializer GetSerializer(string typeName)
        {
            switch (typeName)
            {
                case "Redaction":
                    return GetSerializer(typeof(Redaction));
                case "TextLayerObject":
                    return GetSerializer(typeof(TextLayerObject));
                case "Clue":
                    return GetSerializer(typeof(Clue));
                default:
                {
                    ExtractException ee = new ExtractException("ELI22936", "Unexpected object.");
                    ee.AddDebugData("Type", typeName, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Writes an &lt;Object&gt; node for each layer object.
        /// </summary>
        /// <param name="writer">The stream on which to write &lt;Object&gt; nodes.</param>
        /// <param name="layerObjects">The layer objects from which &lt;Object&gt; nodes should be 
        /// created.</param>
        static void WriteObjectList(XmlWriter writer, IEnumerable<LayerObject> layerObjects)
        {
            foreach (LayerObject layerObject in layerObjects)
            {
                writer.WriteStartElement("Object");
                writer.WriteAttributeString("Type", layerObject.GetType().Name);
                writer.WriteAttributeString("Id", 
                    layerObject.Id.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("Revision", 
                    layerObject.Revision.ToString(CultureInfo.CurrentCulture));
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes an &lt;Object&gt; node for each layer object.
        /// </summary>
        /// <param name="writer">The stream on which to write &lt;Object&gt; nodes.</param>
        /// <param name="layerObjects">The parent node of layer object nodes from which 
        /// &lt;Object&gt; nodes should be created.</param>
        static void WriteObjectList(XmlWriter writer, XmlNode layerObjects)
        {
            foreach (XmlNode layerObject in layerObjects.ChildNodes)
            {
                writer.WriteStartElement("Object");
                writer.WriteAttributeString("Type", layerObject.Name);
                writer.WriteAttributeString("Id", layerObject.Attributes["Id"].Value);
                writer.WriteAttributeString("Revision", layerObject.Attributes["Revision"].Value);
                writer.WriteEndElement();
            }
        }

        #endregion Methods

        #region Event handlers

        /// <summary>
        /// Handles the <see cref="ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ImageViewer.ImageFileChanged"/> event.</param>
        void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                // Clear existing modification history
                Clear();

                if (!string.IsNullOrEmpty(e.FileName))
                {
                    // Reset the next layer object id. 
                    // Note: This is safe because the image has changed.
                    LayerObject.ResetNextId();

                    // Check if an IDSO file exists
                    _idsoFile = e.FileName + ".idso";
                    if (File.Exists(_idsoFile))
                    {
                        Load();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22875", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event handlers

        #region OnEvents

        /// <summary>
        /// Raises the <see cref="ModificationHistory.ModificationHistoryLoaded"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ModificationHistoryLoadedEventArgs"/>
        /// that contains the data associated with the event.</param>
        void OnModificationHistoryLoaded(ModificationHistoryLoadedEventArgs e)
        {
            if (ModificationHistoryLoaded != null)
            {
                ModificationHistoryLoaded(this, e);
            }
        }

        #endregion OnEvents

        #region Properties

        /// <summary>
        /// Gets whether an IDSO file is currently being loaded.
        /// </summary>
        /// <returns><see langword="true"/> if an IDSO file is being loaded;
        /// <see langword="false"/> if an IDSO file is not being loaded.</returns>
        internal bool LoadingIdso
        {
            get
            {
                return _loadingIDSO;
            }
        }

        #endregion Properties
    }
}
