using Extract.Licensing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a collection of <see cref="LayerObject"/> classes.
    /// </summary>
    /// <seealso cref="LayerObject"/>
    [CLSCompliant(false)]
    public class LayerObjectsCollection : IEnumerable<LayerObject>, IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(LayerObjectsCollection).ToString();

        #endregion Constants

        #region LayerObjects Collection Fields

        /// <summary>
        /// <see cref="Dictionary{T,T}"/> mapping layerObject ids to objects.
        /// </summary>
        /// <seealso cref="LayerObject.Id"/>
        /// <seealso cref="LayerObject"/>
        private Dictionary<long, LayerObject> _objects = new Dictionary<long, LayerObject>();

        /// <summary>
        /// <see cref="List{T}"/> containing all layer objects in a sorted order.
        /// </summary>
        private List<LayerObject> _sortedCollection = new List<LayerObject>();

        /// <summary>
        /// <see cref="List{T}"/> containing all layer objects in the order they are painted to the
        /// screen. By default, this is the order in which they were added to the collection.
        /// </summary>
        private List<LayerObject> _objectsInZOrder = new List<LayerObject>();

        /// <summary>
        /// A collection of the currently selected objects. <see langword="null"/> if this 
        /// collection is the <see cref="Selection"/> of another 
        /// <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <seealso cref="Selection"/>
        private LayerObjectsCollection _selection;

        /// <summary>
        /// <see langword="true"/> if the <see cref="LayerObjectsCollection"/> is the 
        /// <see cref="Selection"/> property of another <see cref="LayerObjectsCollection"/>; 
        /// <see langword="false"/> otherwise.
        /// </summary>
        bool _isSelectionCollection;

        #endregion

        #region LayerObjects Collection Events

        /// <summary>
        /// Occurs when a <see cref="LayerObject"/> is added to the collection.
        /// </summary>
        /// <seealso cref="Add(LayerObject)"/>
        /// <seealso cref="Add(LayerObject, bool)"/>
        public event EventHandler<LayerObjectAddedEventArgs> LayerObjectAdded;

        /// <summary>
        /// Occurs when a <see cref="LayerObject"/> contained in the collection is changed.
        /// </summary>
        /// <seealso cref="this"/>
        public event EventHandler<LayerObjectChangedEventArgs> LayerObjectChanged;

        /// <summary>
        /// Occurs when layer objects are about to be removed from the 
        /// <see cref="DocumentViewer.LayerObjects"/> collection.
        /// </summary>
        /// <remarks>This event is not raised by a <see cref="LayerObjectsCollection"/> that is 
        /// not connected to an <see cref="DocumentViewer"/> or by the 
        /// <see cref="LayerObjectsCollection.Selection"/> property.</remarks>
        public event EventHandler<DeletingLayerObjectsEventArgs> DeletingLayerObjects;

        /// <summary>
        /// Occurs when a <see cref="LayerObject"/> is removed from the collection.
        /// </summary>
        /// <seealso cref="Remove(long)"/>
        /// <seealso cref="Remove(long, bool, bool)"/>
        /// <seealso cref="Clear"/>
        public event EventHandler<LayerObjectDeletedEventArgs> LayerObjectDeleted;

        /// <summary>
        /// Occurs when a <see cref="LayerObject"/> changes visibility.
        /// </summary>
        public event EventHandler<LayerObjectVisibilityChangedEventArgs>
            LayerObjectVisibilityChanged;

        #endregion

        #region LayerObjects Collection Constructor

        /// <overloads>Initializes a new <see cref="LayerObjectsCollection"/> class.</overloads>
        /// <summary>
        /// Initializes an empty collection of objects.
        /// </summary>
        public LayerObjectsCollection()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23113",
					_OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23114", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="LayerObjectsCollection"/> class that contains references to
        /// objects from the specified <paramref name="objects"/> collection. 
        /// </summary>
        /// <param name="objects">Collection whose <see cref="LayerObject"/> elements are stored 
        /// in the new <see cref="LayerObjectsCollection"/></param>
        /// <exception cref="ExtractException"><paramref name="objects"/> contains a 
        /// <see langword="null"/> <see cref="LayerObject"/>.</exception>
        /// <exception cref="ExtractException"><paramref name="objects"/> contains one or more 
        /// <see cref="LayerObject"/> classes with the same <see cref="LayerObject.Id"/> property 
        /// value.</exception>
        /// <remarks>This constructor stores the actual <see cref="LayerObject"/> elements in 
        /// <paramref name="objects"/>; it does not store a copy of each layerObject.
        /// </remarks>
        public LayerObjectsCollection(IEnumerable<LayerObject> objects)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23115",
					_OBJECT_NAME);

                // Add the specified objects
                foreach (LayerObject layerObject in objects)
                {
                    // Ensure layerObject is non-null.
                    if (layerObject == null)
                    {
                        throw new ExtractException("ELI21198", "LayerObject cannot be null.");
                    }

                    // Add the layerObject to the collection
                    _objects.Add(layerObject.Id, layerObject);

                    // Add the layerObject to the sorted collection
                    _sortedCollection.Add(layerObject);

                    // Add the layerObject to the z-ordered list.
                    _objectsInZOrder.Add(layerObject);
                }

                _sortedCollection.Sort();
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21128", 
                    "Unable to create objects collection.", e);
            }
        }

        #endregion

        #region LayerObjects Collection Properties

        /// <summary>
        /// Gets or sets the <see cref="LayerObject"/> with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the layerObject to get or set from the collection.</param>
        /// <value>The <see cref="LayerObject"/> associated with the specified 
        /// <paramref name="id"/>. Cannot be <see langword="null"/>.</value>
        /// <returns>The <see cref="LayerObject"/> associated with the specified 
        /// <paramref name="id"/>.</returns>
        /// <event cref="LayerObjectChanged">Occurs for each successful set operation.</event>
        /// <exception cref="ExtractException"><paramref name="value"/> is <see langword="null"/>
        /// </exception>
        public LayerObject this[long id]
        {
            get
            {
                return _objects[id];
            }
            set
            {
                try
                {
                    // Ensure the layerObject is not null
                    if (value == null)
                    {
                        throw new ExtractException("ELI21199", "LayerObject cannot be null.");
                    }

                    // Change the layerObject with the specified id.
                    _objects[id] = value;

                    // Raise the layerObject changed event
                    OnLayerObjectChanged(new LayerObjectChangedEventArgs(value));
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21137",
                        "Unable to set layerObject.", e);
                    ee.AddDebugData("Id", id, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets the number of objects in the collection.
        /// </summary>
        /// <value>The number of objects in the collection.</value>
        public int Count
        {
            get
            {
                return _objects.Count;
            }
        }

        /// <summary>
        /// Gets the currently selected objects.
        /// </summary>
        /// <returns>The currently selected objects.</returns>
        public LayerObjectsCollection Selection
        {
            get
            {
                return _selection;
            }
        }

        /// <summary>
        /// Gets an enumeration of the <see cref="LayerObject"/>s that enumerates in the order the
        /// <see cref="LayerObject"/>s are to be displayed (the top-most obejct is last).
        /// </summary>
        /// <returns>An enumeration of the <see cref="LayerObject"/>s that enumerates in the order
        /// the <see cref="LayerObject"/>s are to be displayed (the top-most obejct is last).
        /// </returns>
        public IEnumerable<LayerObject> InZOrder
        {
            get
            {
                return _objectsInZOrder;
            }
        }

        #endregion

        #region LayerObjects Collection Methods

        /// <summary>
        /// Adds the specified layerObject to the objects collection.
        /// </summary>
        /// <param name="layerObject">The layerObject to add. Cannot be <see langword="null"/>.
        /// </param>
        /// <event cref="LayerObjectAdded">Occurs for each successful add.</event>
        public void Add(LayerObject layerObject)
        {
            Add(layerObject, true);
        }

        /// <summary>
        /// Adds the specified layerObject to the objects collection.
        /// </summary>
        /// <param name="layerObject">The layerObject to add. Cannot be <see langword="null"/>.
        /// </param>
        /// <param name="raiseLayerObjectAdded"><see langword="true"/> if the
        /// <see cref="LayerObjectAdded"/> event should be raised, otherwise <see langword="false"/>.
        /// </param>
        /// <event cref="LayerObjectAdded">Occurs for each successful add if
        /// <see paramref="raiseLayerObjectAdded"/> is <see langword="true"/>.</event>
        public void Add(LayerObject layerObject, bool raiseLayerObjectAdded)
        {
            try
            {
                // Ensure the layerObject is not null
                if (layerObject == null)
                {
                    throw new ExtractException("ELI21200", "LayerObject cannot be null.");
                }

                // If this is the selection collection, just set the selected property.
                if (_isSelectionCollection)
                {
                    if (!layerObject.Selected)
                    {
                        // Note: the object will add itself to this collection
                        layerObject.Selected = true;
                        return;
                    }
                }

                // Add the layerObject to the collection.
                _objects.Add(layerObject.Id, layerObject);

                // Find the location where this layer object should be inserted into the sorted list.
                // If positive, the index will indicate an equivalent object already in the collection.
                // If negative, the bitwise completment will indicate the position of the first layer
                // object that is greater than the object to add using LayerObject.CompareTo.
                int index = _sortedCollection.BinarySearch(layerObject);

                // Insert the new item at the appropriate location.
                _sortedCollection.Insert((index < 0 ? ~index : index), layerObject);

                // Add to the list of layer objects z-ordered list.
                _objectsInZOrder.Add(layerObject);

                if (raiseLayerObjectAdded)
                {
                    // Raise the layerObject added event
                    OnLayerObjectAdded(new LayerObjectAddedEventArgs(layerObject));
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26559", ex);
            }
        }

        /// <summary>
        /// Removes the layerObject with the specified id.
        /// </summary>
        /// <param name="id">The id of the layerObject to remove.</param>
        /// <event cref="LayerObjectDeleted">Occurs on successful removal.
        /// </event>
        /// <exception cref="ExtractException">A <see cref="LayerObject"/> with the specified 
        /// <paramref name="id"/> is not contained in the collection.</exception>
        public void Remove(long id)
        {
            Remove(id, true, true);
        }

        /// <summary>
        /// Removes the layerObject with the specified id.
        /// </summary>
        /// <param name="id">The id of the layerObject to remove.</param>
        /// <param name="dispose"><see langword="true"/> if the layer object should be disposed
        /// after it is removed, otherwise <see langword="false"/></param>
        /// <param name="raiseEvents"><see langword="true"/> if the <see cref="DeletingLayerObjects"/>
        /// and <see cref="LayerObjectDeleted"/> events should be raised, <see landword="false"/>
        /// otherwise.</param>
        /// <event cref="LayerObjectDeleted">Occurs on successful removal if
        /// <see paramref="raiseEvents"/> is <see langword="true"/>.</event>
        /// <exception cref="ExtractException">A <see cref="LayerObject"/> with the specified 
        /// <paramref name="id"/> is not contained in the collection.</exception>
        public void Remove(long id, bool dispose, bool raiseEvents)
        {
            try
            {
                // Retrieve the specified layerObject
                LayerObject layerObject;
                if (!_objects.TryGetValue(id, out layerObject))
                {
                    throw new ExtractException("ELI21103",
                        "Cannot remove layerObject. LayerObject not found.");
                }

                // Remove the specified layer object
                Remove(new LayerObject[] { layerObject }, dispose, raiseEvents);
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21129",
                    "Unable to remove layerObject.", e);
                ee.AddDebugData("Id", id, false);
                throw ee;
            }
        }

        /// <overloads>Removes a layerObject.</overloads>
        /// <summary>
        /// Removes the specified layerObject.
        /// </summary>
        /// <param name="layerObject">The layerObject to remove from the collection.</param>
        /// <returns>The layerObject that was removed.</returns>
        /// <event cref="LayerObjectDeleted">Occurs on a successful removal.
        /// </event>
        /// <exception cref="ExtractException">The <paramref name="layerObject"/> is not contained 
        /// in the collection.</exception>
        public void Remove(LayerObject layerObject)
        {
            Remove(layerObject, true, true);
        }

        /// <overloads>Removes a layerObject.</overloads>
        /// <summary>
        /// Removes the specified layerObject.
        /// </summary>
        /// <param name="layerObject">The layerObject to remove from the collection.</param>
        /// <param name="dispose"><see langword="true"/> if the layer object should be disposed
        /// after it is removed, otherwise <see langword="false"/></param>
        /// <param name="raiseEvents"><see langword="true"/> if the <see cref="DeletingLayerObjects"/>
        /// and <see cref="LayerObjectDeleted"/> events should be raised, <see landword="false"/>
        /// otherwise.</param>
        /// <event cref="LayerObjectDeleted">Occurs on successful removal if
        /// <see paramref="raiseEvents"/> is <see langword="true"/>.</event>
        /// <exception cref="ExtractException">The <paramref name="layerObject"/> is not contained 
        /// in the collection.</exception>
        public void Remove(LayerObject layerObject, bool dispose, bool raiseEvents)
        {
            try
            {
                // Ensure the layer object is in the collection
                if (!_objects.ContainsKey(layerObject.Id))
                {
                    throw new ExtractException("ELI21104",
                        "Cannot remove layerObject. LayerObject not found.");
                }

                Remove(new LayerObject[] { layerObject }, dispose, raiseEvents);
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21130", 
                    "Unable to remove layerObject.", e);
                if (layerObject != null)
                {
                    ee.AddDebugData("LayerObject Id", layerObject.Id, false);
                }
                else
                {
                    ee.AddDebugData("LayerObject", "null", false);
                }
                throw ee;
            }
        }

        /// <summary>
        /// Removes the specified <see cref="IEnumerable{T}"/> collection
        /// of <see cref="LayerObject"/> objects.
        /// </summary>
        /// <param name="layerObjects">An <see cref="IEnumerable{T}"/> collection
        /// of <see cref="LayerObject"/> objects to be removed from the collection.</param>
        /// <param name="dispose"><see langword="true"/> if the layer object should be disposed
        /// after it is removed, otherwise <see langword="false"/></param>
        /// <param name="raiseEvents"><see langword="true"/> if the <see cref="DeletingLayerObjects"/>
        /// and <see cref="LayerObjectDeleted"/> events should be raised, <see landword="false"/>
        /// otherwise.</param>
        /// <exception cref="ExtractException">If <paramref name="layerObjects"/>
        /// is <see langword="null"/></exception>
        /// <exception cref="ExtractException">If any of the <see cref="LayerObject"/>
        /// objects are not part of the current <see cref="LayerObject"/> collection.</exception>
        /// after it is removed, otherwise <see langword="false"/>
        public void Remove(IEnumerable<LayerObject> layerObjects, bool dispose, bool raiseEvents)
        {
            try
            {
                ExtractException.Assert("ELI22133", "Objects collection may not be null!",
                    layerObjects != null);

                // First check that each object exists in the collection
                foreach (LayerObject layerObject in layerObjects)
                {
                    if (!_objects.ContainsKey(layerObject.Id))
                    {
                        ExtractException ee = new ExtractException("ELI22134",
                            "Specified object does not exist in collection!");
                        ee.AddDebugData("LayerObject Id", layerObject.Id, false);
                        throw ee;
                    }
                }

                LayerObjectsCollection objectsToRemove =  new LayerObjectsCollection(layerObjects);

                if (raiseEvents)
                {
                    DeletingLayerObjectsEventArgs eventArgs =
                        new DeletingLayerObjectsEventArgs(objectsToRemove);
                    OnDeletingLayerObjects(eventArgs);

                    // Check if the event was cancelled
                    if (eventArgs.Cancel)
                    {
                        return;
                    }
                }

                // Now remove each object
                // Note: Use the event args collection, because event handlers
                // may have added or removed layer objects from the collection.
                foreach (LayerObject layerObject in objectsToRemove)
                {
                    RemoveOne(layerObject, dispose, raiseEvents);
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI22135",
                    "Unable to remove LayerObject collection!", ex);
            }
        }

        /// <summary>
        /// Removes all selected objects from the <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <event cref="LayerObjectDeleted">Occurs for each layerObject that is removed.</event>
        public void RemoveSelected()
        {
            try
            {
                if (_selection != null)
                {
                    // Create a copy of the objects to remove
                    List<LayerObject> objects = new List<LayerObject>(_selection.Count);
                    objects.AddRange(_selection);

                    // Remove each layerObject individually
                    foreach (LayerObject layerObject in objects)
                    {
                        this.Remove(layerObject, true, true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26560", ex);
            }
        }

        /// <summary>
        /// Removes all selected objects from the <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <param name="pageNumber">The page to remove selected objects from.</param>
        /// <event cref="LayerObjectDeleted">Occurs for each layerObject that is removed.</event>
        public void RemoveSelected(int pageNumber)
        {
            try
            {
                if (_selection != null)
                {
                    // Create a copy of the objects to remove
                    List<LayerObject> objects = new List<LayerObject>(_selection.Count);
                    objects.AddRange(_selection);

                    // Remove each layerObject individually
                    foreach (LayerObject layerObject in objects)
                    {
                        if (layerObject.PageNumber == pageNumber)
                        {
                            this.Remove(layerObject, true, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26561", ex);
            }
        }

        /// <summary>
        /// Adds all the objects in the <see cref="LayerObjectsCollection"/> to the 
        /// <see cref="Selection"/>.
        /// </summary>
        public void SelectAll()
        {
            try
            {
                if (_selection != null)
                {
                    foreach (LayerObject layerObject in _objects.Values)
                    {
                        // Ensure the layer object is selectable before selecting it
                        if (layerObject.Selectable && layerObject.Visible)
                        {
                            layerObject.Selected = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26562", ex);
            }
        }

        /// <summary>
        /// Removes all objects from the collection.
        /// </summary>
        /// <event cref="LayerObjectDeleted">Occurs for each layerObject that is removed.</event>
        public void Clear()
        {
            try
            {
                // Remove all the layer objects
                Remove(_objects.Values, true, true);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21599",
                    "Unable to clear objects.", ex);
            }
        }

        /// <overloads>Determines whether the <see cref="LayerObjectsCollection"/> contains a 
        /// layerObject.</overloads>
        /// <summary>
        /// Determines whether a layerObject with the specified id is contained in the 
        /// <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <param name="id">The id of the layerObject to check for containment.</param>
        /// <returns><see langword="true"/> if the layerObject with the specified id is contained in 
        /// the collection; <see langword="false"/> if no layerObject with the specified id is 
        /// contained in the collection.</returns>
        public bool Contains(long id)
        {
            return _objects.ContainsKey(id);
        }

        /// <summary>
        /// Determines whether the specified layerObject is contained in the 
        /// <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <param name="layerObject">The layerObject to check for containment.</param>
        /// <returns><see langword="true"/> if <paramref name="layerObject"/> is contained in the 
        /// collection; <see langword="false"/> if <paramref name="layerObject"/> is not contained 
        /// in the collection.</returns>
        public bool Contains(LayerObject layerObject)
        {
            return layerObject != null && _objects.ContainsKey(layerObject.Id);
        }

        /// <summary>
        /// Gets the layer object associated with the specified id or <see langword="null"/> if it 
        /// doesn't exist.
        /// </summary>
        /// <param name="id">The id of the layer object to retrieve.</param>
        /// <returns>The layer object associated with the specified id or <see langword="null"/> 
        /// if it doesn't exist.</returns>
        public LayerObject TryGetLayerObject(long id)
        {
            try
            {
                // Attempt to retrieve 
                LayerObject layerObject;
                if (_objects.TryGetValue(id, out layerObject))
                {
                    return layerObject;
                }

                // If we reached this point, the layer object was not found
                return null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26563", ex);
            }
        }

        /// <summary>
        /// Constructs a new <see cref="LayerObjectsCollection"/> containing references to the 
        /// objects on specified page.
        /// </summary>
        /// <param name="page">The one-based page number from which layerObject references should be 
        /// copied.</param>
        /// <returns>A <see cref="LayerObjectsCollection"/> containing references to the 
        /// <see cref="LayerObject"/> classes on the specified page. If no objects are on the 
        /// specified <paramref name="page"/>, the <see cref="LayerObjectsCollection"/> will be 
        /// empty.</returns>
        public LayerObjectsCollection GetLayerObjectsOnPage(int page)
        {
            try
            {
                // Construct a linked list of objects on the specified page
                LinkedList<LayerObject> objectsOnPage = new LinkedList<LayerObject>();
                foreach (KeyValuePair<long, LayerObject> pair in _objects)
                {
                    if (pair.Value.PageNumber == page)
                    {
                        objectsOnPage.AddLast(pair.Value);
                    }
                }

                // Return the objects as a new LayerObjectsCollection
                return new LayerObjectsCollection(objectsOnPage);
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21131",
                    "Unable to get objects on page.", e);
                ee.AddDebugData("Page number", page, false);
                throw ee;
            }   
        }

        /// <summary>
        /// Returns a <see cref="Collection{T}"/> of the indexes into the
        /// visible sorted collection (see <seealso cref="GetSortedVisibleCollection(bool)"/>)
        /// of <see cref="LayerObject"/> that intersect
        /// with the specified <see cref="Rectangle"/> on the specified page.
        /// If <paramref name="allObjects"/> is <see langword="true"/> then the
        /// returned list contains the index of all items contained in the
        /// <see cref="Rectangle"/>; if <paramref name="allObjects"/> is
        /// <see langword="false"/> then the list will only contain the index
        /// of the first item contained in the <see cref="Rectangle"/>.
        /// <para><b>Note:</b></para>
        /// The returned collection of indexes is only correct as long as no layer objects
        /// change, add, or delete from the <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <param name="pageNumber">The page to search on.</param>
        /// <param name="viewRectangle">The <see cref="Rectangle"/> to search in.</param>
        /// <param name="allObjects">Whether to return the index for all objects or
        /// just the first object.</param>
        /// <param name="mustBeContained">If <see langword="true"/> then checks if
        /// the <see cref="LayerObject"/> is completely contained by the specified
        /// rectangle before adding its index to the collection; if <see langword="false"/>
        /// will just check if there is an intersection.</param>
        /// <returns>A <see cref="Collection{T}"/> of indexes into the sorted collection
        /// of <see cref="LayerObject"/> that are contained within the specified
        /// <see cref="Rectangle"/> on the specified page.</returns>
        public Collection<int> GetIndexOfSortedLayerObjectsInRectangle(int pageNumber,
            Rectangle viewRectangle, bool allObjects, bool mustBeContained)
        {
            try
            {
                // Get the collection of sorted visible objects
                ReadOnlyCollection<LayerObject> sortedCollection =
                    this.GetSortedVisibleCollection(true);

                // Create the collection to hold the indexes
                Collection<int> indexes = new Collection<int>();

                // Loop through the sorted collection and get the index of objects that
                // intersect the specified rectangle
                for (int i = 0; i < sortedCollection.Count; i++)
                {
                    LayerObject layerObject = sortedCollection[i];

                    // If we have passed the page that we are searching on we are done
                    if (layerObject.PageNumber > pageNumber)
                    {
                        break;
                    }

                    // If mustBeContained is true then check if the object is contained by
                    // the rectangle, otherwise just check for intersection on the specified page
                    bool addIndex = mustBeContained ?
                        layerObject.IsContained(viewRectangle, pageNumber)
                        : (layerObject.PageNumber == pageNumber
                        && layerObject.IsVisible(viewRectangle));

                    if (addIndex)
                    {
                        indexes.Add(i);

                        // If not looking for all objects then we are done
                        if (!allObjects)
                        {
                            break;
                        }
                    }
                }

                // Return the collection of indexes
                return indexes;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22442", ex);
            }
        }

        /// <summary>
        /// Gets the sorted collection of <see cref="LayerObject"/>.
        /// <para><b>Note:</b></para>
        /// The returned collection is only correct as long as no layer objects
        /// change, add, or delete from the <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <returns>The current collection of sorted objects.</returns>
        // FXCop thinks this should be a property but this data is not stored in a field,
        // it is recomputed each time the method is called
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ReadOnlyCollection<LayerObject> GetSortedCollection()
        {
            try
            {
                return _sortedCollection.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22443", ex);
            }
        }

        /// <summary>
        /// Gets the sorted collection of <see cref="LayerObject"/> that
        /// are currently visible (<see cref="LayerObject.Visible"/>
        /// is <see langword="true"/>).
        /// <para><b>Note:</b></para>
        /// The returned collection is only correct as long as no layer objects
        /// change, add, delete, or change visibility in the <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <param name="onlySelectable">If <see langword="true"/> will only return objects
        /// that are both visible and selectable; if <see langword="false"/> returns all
        /// visible objects.</param>
        /// <returns>The current collection of sorted visible <see cref="LayerObject"/>.</returns>
        // FXCop thinks this should be a property but this data is not stored in a field,
        // it is recomputed each time the method is called
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ReadOnlyCollection<LayerObject> GetSortedVisibleCollection(bool onlySelectable)
        {
            try
            {
                List<LayerObject> visibleList = new List<LayerObject>();
                foreach (LayerObject layerObject in _sortedCollection)
                {
                    if (layerObject.Visible && (!onlySelectable || layerObject.Selectable))
                    {
                        visibleList.Add(layerObject);
                    }
                }

                return visibleList.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22444", ex);
            }
        }

        /// <summary>
        /// Moves the specified <see cref="LayerObject"/> to the top of the z-order
        /// </summary>
        /// <param name="layerObject">The <see cref="LayerObject"/> to be moved to the top of the
        /// z-order.</param>
        public void MoveToTop(LayerObject layerObject)
        {
            try
            {
                for (int i = 0; i < _objectsInZOrder.Count; i++)
                {
                    if (_objectsInZOrder[i].Id == layerObject.Id)
                    {
                        _objectsInZOrder.RemoveAt(i);
                        _objectsInZOrder.Add(layerObject);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25703", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="LayerObjectChanged"/> event for the specified layerObject.
        /// </summary>
        /// <param name="layerObject">The <see cref="LayerObject"/> that changed.</param>
        internal void RaiseLayerObjectChangedEvent(LayerObject layerObject)
        {
            // Raise the LayerObjectChanged event
            OnLayerObjectChanged(new LayerObjectChangedEventArgs(layerObject));
        }

        /// <summary>
        /// Raises the <see cref="LayerObjectVisibilityChanged"/> event for the specified
        /// <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="layerObject">The <see cref="LayerObject"/> that changed
        /// visibility.</param>
        internal void RaiseLayerObjectVisibilityChangedEvent(LayerObject layerObject)
        {
            // Raise the LayerObjectVisibilityChanged event
            OnLayerObjectVisibilityChanged(new LayerObjectVisibilityChangedEventArgs(layerObject));
        }

        /// <summary>
        /// Creates a <see cref="LayerObjectsCollection"/> class with an empty 
        /// <see cref="Selection"/> collection.
        /// </summary>
        /// <returns>A <see cref="LayerObjectsCollection"/> class with an empty 
        /// <see cref="Selection"/> collection.</returns>
        internal static LayerObjectsCollection CreateLayerObjectsWithSelection()
        {
            LayerObjectsCollection objects = new LayerObjectsCollection();
            objects._selection = new LayerObjectsCollection();
            objects._selection._isSelectionCollection = true;
            return objects;
        }

        /// <summary>
        /// Gets whether any of the <see cref="LayerObject"/> in the
        /// <see cref="LayerObjectsCollection"/> are completely contained in
        /// the specified <see cref="Rectangle"/> on the specified page.
        /// </summary>
        /// <param name="rectangle">The <see cref="Rectangle"/> to search in.</param>
        /// <param name="pageNumber">The page to search for.</param>
        /// <returns><see langword="true"/> if at least one of the <see cref="LayerObject"/>
        /// is completely contained in the specified rectangle on the specified page and
        /// <see langword="false"/> otherwise.</returns>
        public bool IsAnyObjectContained(Rectangle rectangle, int pageNumber)
        {
            try
            {
                // Loop through each of the objects checking if one is contained
                // in the specified rectangle
                bool returnVal = false;
                foreach (LayerObject layerObject in _objects.Values)
                {
                    // Check if object is contained in the specified rectangle on the specified page
                    if (layerObject.IsContained(rectangle, pageNumber))
                    {
                        // Found one that is contained, no need to keep searching
                        returnVal = true;
                        break;
                    }
                }

                return returnVal;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22482", ex);
            }
        }

        /// <summary>
        /// Creates an array with the elements of the <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <returns>An array with the elements of the <see cref="LayerObjectsCollection"/>.
        /// </returns>
        public LayerObject[] ToArray()
        {
            return new List<LayerObject>(this).ToArray();
        }

        #endregion

        #region LayerObjects Collection Events

        /// <summary>
        /// Raises the <see cref="LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="e">A <see cref="LayerObjectAddedEventArgs"/> that contains the event 
        /// data.</param>
        /// <seealso cref="Add(LayerObject)"/>
        /// <seealso cref="Add(LayerObject, bool)"/>
        protected virtual void OnLayerObjectAdded(LayerObjectAddedEventArgs e)
        {
            LayerObjectAdded?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="LayerObjectChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="LayerObjectChangedEventArgs"/> that contains the event 
        /// data.</param>
        /// <seealso cref="this"/>
        protected virtual void OnLayerObjectChanged(LayerObjectChangedEventArgs e)
        {
            // Resort the sorted collection based on the change
            _sortedCollection.Sort();

            LayerObjectChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="DeletingLayerObjects"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="DeletingLayerObjects"/> 
        /// event.</param>
        protected virtual void OnDeletingLayerObjects(DeletingLayerObjectsEventArgs e)
        {
            DeletingLayerObjects?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="e">A <see cref="LayerObjectDeletedEventArgs"/> that contains the event 
        /// data.</param>
        /// <seealso cref="Remove(long)"/>
        /// <seealso cref="Remove(LayerObject, bool, bool)"/>
        protected virtual void OnLayerObjectDeleted(LayerObjectDeletedEventArgs e)
        {
            LayerObjectDeleted?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="LayerObjectVisibilityChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="LayerObjectVisibilityChangedEventArgs"/> that
        /// contains the event data.</param>
        protected virtual void OnLayerObjectVisibilityChanged(
            LayerObjectVisibilityChangedEventArgs e)
        {
            // If this object is in a current selection, remove it
            if (_selection != null && _selection.Contains(e.LayerObject))
            {
                _selection.Remove(e.LayerObject, true, true);
            }

            // Raise the event if there is a listener
            LayerObjectVisibilityChanged?.Invoke(this, e);
        }

        #endregion

        #region IEnumerable<LayerObject> Members

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="LayerObjectsCollection"/>.</returns>
        /// <seealso cref="IEnumerator{T}"/>
        public IEnumerator<LayerObject> GetEnumerator()
        {
            return _objects.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="LayerObjectsCollection"/>.</returns>
        /// <seealso cref="IEnumerator{T}"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _objects.Values.GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="LayerObjectsCollection"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="LayerObjectsCollection"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="LayerObjectsCollection"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed objects
                if (_objects != null)
                {
                    foreach (LayerObject layerObject in _objects.Values)
                    {
                        // No layerObject should be null, but ensure this since 
                        // Dispose methods should never throw exceptions
                        if (layerObject != null)
                        {
                            layerObject.Dispose();
                        }
                    }
                }

                // This is redundant disposal, but it makes FxCop happy.
                if (_selection != null)
                {
                    _selection.Dispose();
                }
            }

            // No unmanaged resources to free
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Removes the specified layer object.
        /// </summary>
        /// <param name="layerObject">The layer object to remove.</param>
        /// <param name="dispose"><see langword="true"/> if the layer object should be disposed
        /// after it is removed, otherwise <see langword="false"/></param>
        /// <param name="raiseLayerObjectDeleted"><see langword="true"/> if
        /// <see cref="LayerObjectDeleted"/> should be raised, <see landword="false"/> otherwise.
        /// </param>
        private void RemoveOne(LayerObject layerObject, bool dispose, bool raiseLayerObjectDeleted)
        {
            // If this is the selection collection, just reset the selected property.
            if (_isSelectionCollection)
            {
                if (layerObject.Selected)
                {
                    // Note: the object will remove itself from this collection
                    layerObject.Selected = false;
                    return;
                }
            }

            // Remove the layer object from the collection
            _objects.Remove(layerObject.Id);

            // Get the index of the object from the sorted collection
            int index = 0;
            for (; index < _sortedCollection.Count; index++)
            {
                if (_sortedCollection[index].Id == layerObject.Id)
                {
                    break;
                }
            }

            // Remove it from the sorted collection
            _sortedCollection.RemoveAt(index);

            // Get the index of the object from the z-order collection
            index = 0;
            for (; index < _objectsInZOrder.Count; index++)
            {
                if (_objectsInZOrder[index].Id == layerObject.Id)
                {
                    break;
                }
            }

            // Remove it from the creation order list.
            _objectsInZOrder.RemoveAt(index);

            // Check if this objects collection has a selection collection
            if (_selection != null)
            {
                // Remove the links associated with this layer object if necessary
                if (layerObject.IsLinked)
                {
                    layerObject.RemoveLinks();
                }

                // Check if the selection contains the layerObject being removed
                if (_selection.Contains(layerObject.Id))
                {
                    _selection.RemoveOne(layerObject, dispose, raiseLayerObjectDeleted);
                }
            }

            if (raiseLayerObjectDeleted)
            {
                // Raise the layerObject deleted event
                OnLayerObjectDeleted(new LayerObjectDeletedEventArgs(layerObject));
            }

            // Dispose of the layer object if the collection has a selection collection
            if (_selection != null && dispose)
            {
                // Detach this layerObject from the image viewer
                layerObject.ImageViewer = null;

                layerObject.Dispose();
            }
        }

        #endregion Private Members
    }
}
