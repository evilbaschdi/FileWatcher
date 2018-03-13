using System;
using Newtonsoft.Json;

namespace FileWatcher.Model
{
    /// <summary>
    /// </summary>
    public class FileWatcherHelper
    {
        /// <summary>
        /// </summary>
        [JsonProperty("Path")]
        public string Path { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty("LastWriteTime")]
        public DateTime LastWriteTime { get; set; }
    }
}