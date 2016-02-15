using System;

namespace FileWatcher.Internal
{
    /// <summary>
    /// </summary>
    [Serializable]
    public class FileWatcherHelper
    {
        /// <summary>
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// </summary>
        public DateTime LastWriteTime { get; set; }
    }
}