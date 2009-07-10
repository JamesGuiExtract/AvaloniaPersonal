using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace SourceControl
{
    /// <summary>
    /// Represents a mutable collection of display directories.
    /// </summary>
    public class DisplayDirectoryBuilder
    {
        #region DisplayDirectoryBuilder Fields

        /// <summary>
        /// The physical (local) directories from which the abbreviated (display) directories will 
        /// be built.
        /// </summary>
        readonly List<string> _directories = new List<string>();

        /// <summary>
        /// Each <see cref="Queue{T}"/> is a collection of child directories in the same parent 
        /// directory. The first <see cref="LinkedListNode{T}"/> represents the child directories 
        /// of <see cref="_engineeringRoot"/>. Each subsequent <see cref="LinkedListNode{T}"/> 
        /// contains the child directories of <see cref="Queue{T}.Peek"/> of the preceding 
        /// <see cref="LinkedListNode{T}"/>.
        /// </summary>
        readonly LinkedList<Queue<DisplayDirectory>> _displayDirectories = 
            new LinkedList<Queue<DisplayDirectory>>();

        /// <summary>
        /// The lowest level physical directory. 
        /// </summary>
        readonly string _rootDirectory;

        /// <summary>
        /// The physical directory of the engineering tree. All abbreviated (display) directories 
        /// are implicitly understood to be under this directory.
        /// </summary>
        readonly string _engineeringRoot;

        #endregion DisplayDirectoryBuilder Fields

        #region DisplayDirectoryBuilder Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayDirectoryBuilder"/> class.
        /// </summary>
        /// <param name="rootDirectory">The physical (local) directory to which the repository 
        /// root is bound.</param>
        public DisplayDirectoryBuilder(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
            _engineeringRoot = Path.Combine(rootDirectory, "Engineering");
        }

        #endregion DisplayDirectoryBuilder Constructors

        #region DisplayDirectoryBuilder Properties

        /// <summary>
        /// Gets the number of display directories.
        /// </summary>
        /// <returns>The number of display directories.</returns>
        public int Count
        {
            get
            {
                return _directories.Count;
            }
        }

        #endregion DisplayDirectoryBuilder Properties

        #region DisplayDirectoryBuilder Methods

        /// <summary>
        /// Adds the repository path to add to the <see cref="DisplayDirectoryBuilder"/>.
        /// </summary>
        /// <param name="path">The repository path to add to the 
        /// <see cref="DisplayDirectoryBuilder"/>.</param>
        public void AddRepositoryPath(string path)
        {
            // If path is empty don't add it.
            if (path.Length <= 0)
            {
                return;
            }

            // If the directory already exists, don't add it.
            string directory = GetPhysicalDirectory(path);
            foreach (string dir in _directories)
            {
                if (dir.Equals(directory, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            _directories.Add(directory);
        }

        /// <summary>
        /// Gets the physical (local) directory for the specified repository path.
        /// </summary>
        /// <param name="repositoryPath">The repository path.</param>
        /// <returns>The physical (local) directory of <paramref name="repositoryPath"/>.</returns>
        string GetPhysicalDirectory(string repositoryPath)
        {
            string physicalPath = Path.Combine(_rootDirectory, repositoryPath.Replace("/", @"\"));

            if (Directory.Exists(physicalPath))
	        {
                return physicalPath;
	        }

            return Path.GetDirectoryName(physicalPath);
        }

        /// <summary>
        /// Iterates over the display directories.
        /// </summary>
        /// <returns>Each display directory.</returns>
        // This is a complex operation that is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<string> GetDisplayDirectories()
        {
            // Sort the directories case-insensitively
            _directories.Sort(StringComparer.OrdinalIgnoreCase);

            // Iterate through each directory
            foreach (string directory in _directories)
            {
                // Construct and store the display directory name for this directory
                yield return GetDisplayDirectory(directory);
            }
        }

        /// <summary>
        /// Gets the abbreviated (display) directory form of the specified physical directory.
        /// </summary>
        /// <param name="physicalDirectory">The physical (local) directory to abbreviate.</param>
        /// <returns>The abbreviated (display) directory form of 
        /// <paramref name="physicalDirectory"/>.</returns>
        string GetDisplayDirectory(string physicalDirectory)
        {
            // TODO: This is a long method. Can it be broken up for clarity?

            // If this directory is not under the VSS root, just return the physical directory
            if (!physicalDirectory.StartsWith(_engineeringRoot, StringComparison.OrdinalIgnoreCase))
            {
                return physicalDirectory;
            }

            // Used to build the display directory
            StringBuilder displayDirectory = new StringBuilder();

            // Keeps track of the first slash of the current directory being abbreviated
            int startIndex = _engineeringRoot.Length + 1;

            // A collection of the sibling display directories.
            LinkedListNode<Queue<DisplayDirectory>> directoryQueueNode = 
                _displayDirectories.First;

            // Iterate through each subdirectory of the specified physical directory
            while (startIndex < physicalDirectory.Length)
            {
                // Find the slash of the next directory
                int nextIndex = 
                    physicalDirectory.IndexOf("\\", startIndex, StringComparison.Ordinal);

                // Find the name of the current subdirectory
                string subdirectory = 
                    GetSubstringByIndexes(physicalDirectory, startIndex, nextIndex);

                // Check if the display directory collection has gone this deep yet
                if (directoryQueueNode == null)
                {
                    // Create the new queue
                    string directory = physicalDirectory.Substring(0, startIndex);
                    directoryQueueNode = CreateDirectoryQueueNode(directory);
                }

                // Queue up the current subdirectory in the directory queue
                Queue<DisplayDirectory> directoryQueue = directoryQueueNode.Value;
                if (directoryQueue.Count > 0 && Compare(subdirectory, directoryQueue) != 0)
                {
                    // Remove child directories from the linked list
                    while (directoryQueueNode != _displayDirectories.Last)
                    {
                        _displayDirectories.RemoveLast();
                    }

                    // Iterate through the subdirectory queue
                    while (directoryQueue.Count > 0 && Compare(subdirectory, directoryQueue) > 0)
                    {
                        // Remove directories until the directory is found
                        directoryQueue.Dequeue();
                    }

                    // If the directory wasn't found return the physical directory
                    // NOTE: If the directory wasn't found, it means it's not currently in the directory tree on disk
                    // or that the sorted list of physical directories was not alphabetized in the same way as the 
                    // linked list directory tree from disk. The latter would be an internal logic error.
                    if (directoryQueue.Count == 0 || Compare(subdirectory, directoryQueue) != 0)
                    {
                        // If this directory queue is empty, remove it from the display directory collection
                        if (directoryQueue.Count == 0)
                        {
                            _displayDirectories.RemoveLast();
                            directoryQueueNode = _displayDirectories.Last;
                        }

                        // Return the original directory
                        return physicalDirectory;
                    }
                }

                // Add the display subdirectory
                displayDirectory.Append(directoryQueue.Peek().Display);

                // If this is the last directory, we are done.
                if (nextIndex < 0)
                {
                    break;
                }

                // Iterate stuff
                startIndex = nextIndex + 1;
                directoryQueueNode = directoryQueueNode.Next;
            }

            return displayDirectory.ToString();
        }

        /// <summary>
        /// Compares the top 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        static int Compare(string directory, Queue<DisplayDirectory> queue)
        {
            return string.Compare(directory, queue.Peek().Physical, 
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the substring from <paramref name="startIndex"/> to the character before 
        /// <paramref name="endIndex"/>.
        /// </summary>
        /// <param name="fullString">The string from which to get the substring.</param>
        /// <param name="startIndex">The index of the initial character of the substring.</param>
        /// <param name="endIndex">The index of character after the last character of the 
        /// substring or a negative number to indicate extending to the end of 
        /// <paramref name="fullString"/>.</param>
        /// <returns>The substring from <paramref name="startIndex"/> to the character before 
        /// <paramref name="endIndex"/> if <paramref name="endIndex"/> is positive; or the 
        /// substring from <paramref name="startIndex"/> to the end of the string if 
        /// <paramref name="endIndex"/> is negative.</returns>
        static string GetSubstringByIndexes(string fullString, int startIndex, int endIndex)
        {
            if (endIndex < 0)
            {
                // Get the substring from the start index to the end of the string
                return fullString.Substring(startIndex);
            }
            else
            {
                // Get the substring from the start index (inclusive) to the end index (exclusive)
                return fullString.Substring(startIndex, endIndex - startIndex);
            }
        }

        /// <summary>
        /// Creates a node of subdirectories for the specified parent directory.
        /// </summary>
        /// <param name="parentDirectory">The parent directory of the directories to be returned
        /// in the node.</param>
        /// <returns>A node of subdirectories for <paramref name="parentDirectory"/>.</returns>
        LinkedListNode<Queue<DisplayDirectory>> CreateDirectoryQueueNode(string parentDirectory)
        {
            // Get the abbreviated form of all the child directories of the specified directory
            List<DisplayDirectory> displayDirectories = 
                GetAbbreviatedChildDirectories(parentDirectory);

            // Ensure the uniqueness of each display directory name
            RevertDuplicates(displayDirectories);

            // Add and return the resultant queue node
            Queue<DisplayDirectory> node = new Queue<DisplayDirectory>(displayDirectories);
            return _displayDirectories.AddLast(node);
        }

        /// <summary>
        /// Gets the abbreviated form of all the child directories of the specified parent 
        /// directory. The abbreviated form is NOT guaranteed to be unique.
        /// </summary>
        /// <param name="parentDirectory">The parent directory of the child directories to 
        /// abbreviate.</param>
        /// <returns>The non-unique abbreviated form of all the child directories of the specified 
        /// parent directory.</returns>
        static List<DisplayDirectory> GetAbbreviatedChildDirectories(string parentDirectory)
        {
            // Get and sort an array of the subdirectories of the specified directory
            string[] subdirectories = Directory.GetDirectories(parentDirectory);
            Array.Sort(subdirectories, StringComparer.OrdinalIgnoreCase);

            List<DisplayDirectory> displayDirectories =
                new List<DisplayDirectory>(subdirectories.Length);
            foreach (string directory in subdirectories)
            {
                // Add this directory to the array
                int lastSlashIndex = directory.LastIndexOf("\\", StringComparison.Ordinal);
                string subdirectory = directory.Substring(lastSlashIndex + 1);
                DisplayDirectory displayDirectory = new DisplayDirectory(subdirectory);
                displayDirectories.Add(displayDirectory);
            }

            return displayDirectories;
        }

        /// <summary>
        /// Replaces duplicated display directory names with their physical (local) directory 
        /// representation.
        /// </summary>
        /// <param name="displayDirectories">The display directories to check for duplicates.
        /// </param>
        static void RevertDuplicates(List<DisplayDirectory> displayDirectories)
        {
            for (int i = 0; i <= displayDirectories.Count - 2; i++)
            {
                bool isUnique = true;
                for (int j = i + 1; j <= displayDirectories.Count - 1; j++)
                {
                    // Check if a preceding display name matches this one
                    if (displayDirectories[i].Display == displayDirectories[j].Display)
                    {
                        // Change both display names to their physical directory name
                        displayDirectories[j].Display = "/" + displayDirectories[j].Physical;
                        isUnique = false;
                    }
                }

                if (!isUnique)
                {
                    displayDirectories[i].Display = "/" + displayDirectories[i].Physical;
                }
            }
        }

        #endregion DisplayDirectoryBuilder Methods
    }
}
