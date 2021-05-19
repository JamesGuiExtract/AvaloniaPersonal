using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Extract.FileConverter
{
    public abstract class NotifyPropertyChangedObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Event required for INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Updates specified value, and raises the <see cref="PropertyChanged"/> event when the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="storage">The current stored value</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The optional property name, automatically set to caller member name when not set.</param>
        /// <returns>Indicates whether the value has changed.</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        protected bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (!object.Equals(storage, value))
            {
                storage = value;
                NotifyPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called by each of the property Set accessors when property changes
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
