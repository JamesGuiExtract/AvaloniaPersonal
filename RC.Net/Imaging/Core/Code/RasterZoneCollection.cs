using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterZoneCollection"/> class.
        /// </summary>
        public RasterZoneCollection()
        {
            _zones = new List<RasterZone>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterZoneCollection"/> class.
        /// </summary>
        public RasterZoneCollection(int capacity)
        {
            _zones = new List<RasterZone>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterZoneCollection"/> class.
        /// </summary>
        public RasterZoneCollection(IEnumerable<RasterZone> zones)
        {
            _zones = new List<RasterZone>(zones);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterZoneCollection"/> class.
        /// </summary>
        [CLSCompliant(false)]
        public RasterZoneCollection(IUnknownVector vector)
        {
            int count = vector.Size();
            _zones = new List<RasterZone>(count);
            for (int i = 0; i < count; i++)
            {
                ComRasterZone zone = (ComRasterZone)vector.At(i);
                _zones.Add(new RasterZone(zone));
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
                ComRasterZone[] myZones = GetAsComArray(_zones);
                ComRasterZone[] otherZones = GetAsComArray(zones);

                double area = 0.0;
                foreach (ComRasterZone myZone in myZones)
                {
                    foreach (ComRasterZone otherZone in otherZones)
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
        /// Creates an array of COM raster zones from the specified collection of raster zones.
        /// </summary>
        /// <param name="zones">The zones from which to create an array of COM raster zones.</param>
        /// <returns>An array of COM raster zones from the specified collection of 
        /// <paramref name="zones"/>.</returns>
        static ComRasterZone[] GetAsComArray(IEnumerable<RasterZone> zones)
        {
            IList<RasterZone> list = zones as IList<RasterZone>;
            ComRasterZone[] result;
            if (list != null)
            {
                result = new ComRasterZone[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    result[i] = list[i].ToComRasterZone();
                }
            }
            else
            {
                List<ComRasterZone> comZones = new List<ComRasterZone>();
                foreach (RasterZone zone in zones)
                {
                    comZones.Add(zone.ToComRasterZone());
                }
                result = comZones.ToArray();
            }
            
            return result;
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
            return _zones.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="IList{T}"/> at the specified index.
        /// </summary>
        /// <param name="item">The object to insert into the <see cref="IList{T}"/>.</param>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        public void Insert(int index, RasterZone item)
        {
            _zones.Insert(index, item);
        }

        /// <summary>
        /// Removes the <see cref="IList{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            _zones.RemoveAt(index);
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
            _zones.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}"/>.
        /// </summary>
        public void Clear()
        {
            _zones.Clear();
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
            return _zones.Contains(item);
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
            _zones.CopyTo(array, arrayIndex);
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
            return _zones.Remove(item);
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