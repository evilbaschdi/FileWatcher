using System;
using System.Runtime.Serialization;

namespace FileWatcher.Internal
{
    /// <summary>
    /// </summary>
    [Serializable]
    [DataContract]
    public class FileWatcherHelper
    {
        /// <summary>
        /// </summary>
        [DataMember]
        public string Path { get; set; }

        /// <summary>
        /// </summary>
        [DataMember]
        public DateTime LastWriteTime { get; set; }
    }
}