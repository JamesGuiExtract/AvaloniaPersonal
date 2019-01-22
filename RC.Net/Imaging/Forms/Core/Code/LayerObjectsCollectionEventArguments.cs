using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Provides data for the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
    /// </summary>
    [CLSCompliant(false)]
    public class LayerObjectAddedEventArgs : EventArgs
    {
        /// <summary>
        /// <see cref="LayerObject"/> that was added.
        /// </summary>
        private readonly LayerObject _object;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerObjectAddedEventArgs"/> class.
        /// </summary>
        /// <param name="layerObject">The layerObject that was added.</param>
        public LayerObjectAddedEventArgs(LayerObject layerObject)
        {
            _object = layerObject;
        }

        /// <summary>
        /// Gets the layerObject that was added.
        /// </summary>
        /// <returns>The layerObject that was added.</returns>
        public LayerObject LayerObject
        {
            get
            {
                return _object;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
    /// </summary>
    [CLSCompliant(false)]
    public class LayerObjectChangedEventArgs : EventArgs
    {
        /// <summary>
        /// <see cref="LayerObject"/> that was changed.
        /// </summary>
        private readonly LayerObject _object;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerObjectChangedEventArgs"/> class.
        /// </summary>
        /// <param name="layerObject">The layerObject that was changed.</param>
        public LayerObjectChangedEventArgs(LayerObject layerObject)
        {
            _object = layerObject;
        }

        /// <summary>
        /// Gets the layerObject that was changed.
        /// </summary>
        /// <returns>The layerObject that was changed.</returns>
        public LayerObject LayerObject
        {
            get
            {
                return _object;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="LayerObjectsCollection.DeletingLayerObjects"/> event.
    /// </summary>
    [CLSCompliant(false)]
    public class DeletingLayerObjectsEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The collection of layer objects that are being deleted.
        /// </summary>
        readonly LayerObjectsCollection _layerObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeletingLayerObjectsEventArgs"/> class.
        /// </summary>
        /// <param name="layerObjects">The collection of layer objects that are being deleted.
        /// </param>
        public DeletingLayerObjectsEventArgs(LayerObjectsCollection layerObjects)
        {
            _layerObjects = layerObjects;
        }

        /// <summary>
        /// Gets the collection of layer objects that are being deleted.
        /// </summary>
        /// <returns>The collection of layer objects that are being deleted.</returns>
        public LayerObjectsCollection LayerObjects
        {
            get
            {
                return _layerObjects;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
    /// </summary>
    [CLSCompliant(false)]
    public class LayerObjectDeletedEventArgs : EventArgs
    {
        /// <summary>
        /// <see cref="LayerObject"/> that was removed.
        /// </summary>
        private readonly LayerObject _object;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerObjectDeletedEventArgs"/> class.
        /// </summary>
        /// <param name="layerObject">The layerObject that was removed.</param>
        public LayerObjectDeletedEventArgs(LayerObject layerObject)
        {
            _object = layerObject;
        }

        /// <summary>
        /// Gets the layerObject that was removed.
        /// </summary>
        /// <returns>The layerObject that was removed.</returns>
        public LayerObject LayerObject
        {
            get
            {
                return _object;
            }
        }
    } 

    /// <summary>
    /// Provides data for the LayerObjectVisibilityChanged event.
    /// </summary>
    [CLSCompliant(false)]
    public class LayerObjectVisibilityChangedEventArgs : EventArgs
    {
        /// <summary>
        /// <see cref="LayerObject"/> whose visibility changed.
        /// </summary>
        private readonly LayerObject _object;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerObjectAddedEventArgs"/> class.
        /// </summary>
        /// <param name="layerObject">The layerObject that changed visibility.</param>
        public LayerObjectVisibilityChangedEventArgs(LayerObject layerObject)
        {
            _object = layerObject;
        }

        /// <summary>
        /// Gets the layerObject whose visibility changed.
        /// </summary>
        /// <returns>The layerObject whose visibility changed.</returns>
        public LayerObject LayerObject
        {
            get
            {
                return _object;
            }
        }
    }

}