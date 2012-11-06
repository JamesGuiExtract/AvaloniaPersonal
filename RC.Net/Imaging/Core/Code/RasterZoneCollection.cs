using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UCLID_COMUTILSLib;

using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents a collection <see cref="RasterZone"/> objects.
    /// </summary>
    public class RasterZoneCollection : IList<RasterZone>
    {
        #region Fields

        readonly List<RasterZone> _zones;
        
        /// <summary>
        /// Keeps track of a corresponding <see cref="ComRasterZone"/>s for each
        /// <see cref="RasterZone"/> in <see cref="_zones"/>.
        /// </summary>
        readonly Dictionary<RasterZone, ComRasterZone> _comRasterZones =
            new Dictionary<RasterZone, ComRasterZone>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterZoneCollection"/> class.
        /// </summary>
        public RasterZoneCollection()
        {
            try
            {
                _zones = new List<RasterZone>();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35115");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterZoneCollection"/> class.
        /// </summary>
        public RasterZoneCollection(int capacity)
        {
            try
            {
                _zones = new List<RasterZone>(capacity);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35116");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterZoneCollection"/> class.
        /// </summary>
        public RasterZoneCollection(IEnumerable<RasterZone> zones)
        {
            try
            {
                _zones = new List<RasterZone>(zones);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35117");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterZoneCollection"/> class.
        /// </summary>
        [CLSCompliant(false)]
        public RasterZoneCollection(IUnknownVector vector)
        {
            try
            {
                int count = vector.Size();
                _zones = new List<RasterZone>(count);
                for (int i = 0; i < count; i++)
                {
                    ComRasterZone zone = (ComRasterZone)vector.At(i);
                    _zones.Add(new RasterZone(zone));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35118");
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Creates an vector of COM raster zones from the <see cref="RasterZoneCollection"/>.
        /// </summary>
        /// <returns>A vector of COM raster zone from the <see cref="RasterZoneCollection"/>.
        /// </returns>
        [CLSCompliant(false)]
        public IUnknownVector ToIUnknownVector()
        {
            try
            {
                IUnknownVector vector = new IUnknownVector();
                foreach (RasterZone zone in _zones)
                {
                    vector.PushBack(zone.ToComRasterZone());
                }

                return vector;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29566", ex);
            }
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the 
        /// <see cref="RasterZoneCollection"/>.
        /// </summary>
        /// <param name="zones">The zones to add.</param>
        public void AddRange(IEnumerable<RasterZone> zones)
        {
            _zones.AddRange(zones);
        }

        /// <summary>
        /// Calculates the area of the raster zones in the <see cref="RasterZoneCollection"/>.
        /// </summary>
        /// <returns>The area of the raster zones in the <see cref="RasterZoneCollection"/>.
        /// </returns>
        // This is performing a complex operation and so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public double GetArea()
        {
            try
            {
                double area = 0.0;
                foreach (RasterZone zone in _zones)
                {
                    area += zone.Area();
                }

                return area;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29243", ex);
            }
        }

        /// <summary>
        /// Computes the area of overlap between this <see cref="RasterZoneCollection"/>
        /// and the specified collection of zones.
        /// </summary>
        /// <param name="zones">The zones with which to compute overlap.</param>
        /// <returns>The area of overlap between this <see cref="RasterZoneCollection"/>
        /// and the specified collection of <paramref name="zones"/>.</returns>
        public double GetAreaOverlappingWith(IEnumerable<RasterZone> zones)
        {
            try
            {
                double area = 0.0;
                foreach (ComRasterZone myZone in _zones.Select(zone => GetComRasterZone(zone)))
                {
                    foreach (ComRasterZone otherZone in zones.Select(zone => GetComRasterZone(zone)))
                    {
                        area += myZone.GetAreaOverlappingWith(otherZone);
                    }
                }

                return area;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29242", ex);
            }
        }

        /// <summary>
        /// Gets a <see cref="ComRasterZone"/> that corresponds to the specified
        /// <see paramref="zone"/>.
        /// </summary>
        /// <param name="zone">The <see cref="RasterZone"/> for which a <see cref="ComRasterZone"/>
        /// is needed.</param>
        /// <returns>The <see cref="ComRasterZone"/>.</returns>
        ComRasterZone GetComRasterZone(RasterZone zone)
        {
            ComRasterZone comRasterZone = null;
            if (!_comRasterZones.TryGetValue(zone, out comRasterZone))
            {
                comRasterZone = zone.ToComRasterZone();
                _comRasterZones[zone] = comRasterZone;
            }

            return comRasterZone;
        }

        #endregion Methods

        #region IList<RasterZone> Members

        /// <summary>
        /// Determines the index of a specific item in the <see cref="IList{T}"/>.
        /// </summary>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="IList{T}"/>.</param>
        public int IndexOf(RasterZone item)
        {
            try
            {
                return _zones.IndexOf(item);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35119");
            }
        }

        /// <summary>
        /// Inserts an item to the <see cref="IList{T}"/> at the specified index.
        /// </summary>
        /// <param name="item">The object to insert into the <see cref="IList{T}"/>.</param>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        public void Insert(int index, RasterZone item)
        {
            try
            {
                _zones.Insert(index, item);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35120");
            }
        }

        /// <summary>
        /// Removes the <see cref="IList{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            try
            {
                _comRasterZones.Remove(_zones[index]);

                _zones.RemoveAt(index);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35121");
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        public RasterZone this[int index]
        {
            get
            {
                return _zones[index];
            }
            set
            {
                _zones[index] = value;
            }
        }

        #endregion IList<RasterZone> Members

        #region ICollection<RasterZone> Members

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
        public void Add(RasterZone item)
        {
            try
            {
                _zones.Add(item);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35122");
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}"/>.
        /// </summary>
        public void Clear()
        {
            try
            {
                _zones.Clear();
                _comRasterZones.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35123");
            }
        }

        /// <summary>
        /// Determines whether the <see cref="ICollection{T}"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if item is found in the <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
        public bool Contains(RasterZone item)
        {
            try
            {
                return _zones.Contains(item);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35124");
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}"/> to an <see cref="Array"/>, 
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of 
        /// the elements copied from <see cref="ICollection{T}"/>.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(RasterZone[] array, int arrayIndex)
        {
            try
            {
                _zones.CopyTo(array, arrayIndex);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35125");
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="ICollection{T}"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return _zones.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="ICollection{T}"/> is read-only; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed from the 
        /// <see cref="ICollection{T}"/>; <see langword="false"/> if item is not found in the 
        /// original <see cref="ICollection{T}"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
        public bool Remove(RasterZone item)
        {
            try
            {
                _comRasterZones.Remove(item);

                return _zones.Remove(item);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35114");
            }
        }

        #endregion ICollection<RasterZone> Members

        #region IEnumerable<RasterZone> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<RasterZone> GetEnumerator()
        {
            return _zones.GetEnumerator();
        }

        #endregion IEnumerable<RasterZone> Members

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _zones.GetEnumerator();
        }

        #endregion IEnumerable Members
    }
}