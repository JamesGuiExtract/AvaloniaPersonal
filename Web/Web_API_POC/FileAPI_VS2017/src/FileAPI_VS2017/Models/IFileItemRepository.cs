using System.Collections.Generic;

namespace FileAPI_VS2017
{
    /// <summary>
    /// File item repository interface
    /// </summary>
    public interface IFileItemRepository
    {
        /// <summary>
        /// Add - interface definition
        /// </summary>
        /// <param name="item"></param>
        void Add(FileItem item);

        /// <summary>
        /// GetAll - interface definition
        /// </summary>
        /// <returns></returns>
        IEnumerable<FileItem> GetAll();

        /// <summary>
        /// Find - interface definition
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        FileItem Find(int Id);

        /// <summary>
        /// Remove - interface defintion
        /// </summary>
        /// <param name="Id"></param>
        void Remove(int Id);

        /// <summary>
        /// Update - interface definition
        /// </summary>
        /// <param name="item"></param>
        void Update(FileItem item);

        /// <summary>
        /// Count - interface defintion
        /// </summary>
        /// <returns></returns>
        int Count();
    }
}
