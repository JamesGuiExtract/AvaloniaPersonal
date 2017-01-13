using System.Collections.Generic;
using System.Collections.Concurrent;

namespace FileAPI_VS2017
{
    /// <summary>
    /// FileItemRepository - concrete instance of interface
    /// </summary>
    public class FileItemRepository : IFileItemRepository
    {
        private static ConcurrentDictionary<int, FileItem> _fileItems =
            new ConcurrentDictionary<int, FileItem>();

        /// <summary>
        /// CTOR
        /// </summary>
        public FileItemRepository()
        {
            Add(new FileItem { Name = "None", Id = 0, Size = 0 });
        }

        /// <summary>
        /// Add - adds a file item to the internal list
        /// </summary>
        /// <param name="item"></param>
        public void Add(FileItem item)
        {
            _fileItems[item.Id] = item;
        }

        /// <summary>
        /// GetAll - get all file items in list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileItem> GetAll()
        {
            return _fileItems.Values;
        }

        /// <summary>
        /// Find - find a file item by id
        /// </summary>
        /// <param name="Id">id of file item to find</param>
        /// <returns></returns>
        public FileItem Find(int Id)
        {
            FileItem item;
            _fileItems.TryGetValue(Id, out item);
            return item;
        }

        /// <summary>
        /// Remove - removes the specified file item from the list
        /// </summary>
        /// <param name="Id">id of file item to remove</param>
        public void Remove(int Id)
        {
            FileItem item;
            _fileItems.TryRemove(Id, out item);
        }

        /// <summary>
        /// Update - updates a specified file item
        /// </summary>
        /// <param name="item">The updated item to replace into the list</param>
        public void Update(FileItem item)
        {
            _fileItems[item.Id] = item;
        }

        /// <summary>
        /// Count - the number of items in teh file item list
        /// </summary>
        /// <returns>number of items</returns>
        public int Count()
        {
            return _fileItems.Count;
        }

    }
}
