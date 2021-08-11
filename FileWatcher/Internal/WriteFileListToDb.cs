using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using FileWatcher.Model;
using Newtonsoft.Json;

namespace FileWatcher.Internal
{
    /// <inheritdoc />
    public class WriteFileListToDb : IWriteFileListToDb
    {
        private readonly App _app;
        private readonly IListFromFileSystem _listFromFileSystem;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="listFromFileSystem"></param>
        /// <param name="app"></param>
        public WriteFileListToDb(IListFromFileSystem listFromFileSystem, App app)
        {
            _listFromFileSystem = listFromFileSystem ?? throw new ArgumentNullException(nameof(listFromFileSystem));
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        /// <inheritdoc />
        public void Run()
        {
            var pathsForSetting = new ConcurrentBag<FileWatcherHelper>();

            Parallel.ForEach(_listFromFileSystem.Value,
                name =>
                {
                    var file = new FileInfo(name);
                    var fileWatcherHelper = new FileWatcherHelper
                                            {
                                                Path = name,
                                                LastWriteTime = file.LastWriteTime
                                            };
                    pathsForSetting.Add(fileWatcherHelper);
                });


            var json = JsonConvert.SerializeObject(pathsForSetting, Formatting.Indented);

            if (File.Exists(_app.JsonPath))
            {
                File.Delete(_app.JsonPath);
            }

            File.AppendAllText(_app.JsonPath, json);
        }
    }
}