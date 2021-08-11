using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileWatcher.Model;
using Newtonsoft.Json;

namespace FileWatcher.Internal
{
    /// <inheritdoc />
    public class CompareFileLists : ICompareFileLists
    {
        private readonly App _app;
        private readonly IListFromFileSystem _listFromFileSystem;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="listFromFileSystem"></param>
        /// <param name="app"></param>
        public CompareFileLists(IListFromFileSystem listFromFileSystem, App app)
        {
            _listFromFileSystem = listFromFileSystem ?? throw new ArgumentNullException(nameof(listFromFileSystem));
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        /// <inheritdoc />
        public List<FileInfo> Value
        {
            get
            {
                var json = File.ReadAllText(_app.JsonPath);
                var listFromSetting = JsonConvert.DeserializeObject<ConcurrentBag<FileWatcherHelper>>(json);

                var newer = new ConcurrentBag<FileInfo>();
                var listFromFileSystem = _listFromFileSystem.Value;

                if (listFromSetting == null || listFromFileSystem == null || !listFromSetting.Any() || !listFromFileSystem.Any())
                {
                    return new();
                }

                Parallel.ForEach(listFromFileSystem,
                    path =>
                    {
                        var file = new FileInfo(path);
                        if (listFromSetting.Any(item => item.Path == path))
                        {
                            var fromSetting = listFromSetting.First(list => list.Path == path);
                            if (File.Exists(fromSetting.Path) && fromSetting.LastWriteTime < file.LastWriteTime)
                            {
                                newer.Add(file);
                            }
                        }
                        else
                        {
                            newer.Add(file);
                        }
                    });

                return newer.ToList();
            }
        }
    }
}