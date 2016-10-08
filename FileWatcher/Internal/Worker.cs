﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Xml.Serialization;
using EvilBaschdi.Core.DirectoryExtensions;

namespace FileWatcher.Internal
{
    /// <summary>
    /// </summary>
    public class Worker
    {
        private readonly IFilePath _filePath;
        private readonly App _app;
        private readonly string _xmlPath;

        /// <summary>
        ///     Initialisiert eine neue Instanz der <see cref="T:System.Object" />-Klasse.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="filePath" /> is <see langword="null" />.
        ///     <paramref name="app" /> is <see langword="null" />.
        /// </exception>
        public Worker(IFilePath filePath, App app)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            _filePath = filePath;
            _app = app;
            _xmlPath = _app.XmlPath;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AggregateException">The exception that contains all the individual exceptions thrown on all threads.</exception>
        public List<FileInfo> Compare()
        {
            var listFromSetting = ConvertPathsFromSettingToDictionary();
            //var newer = new List<FileInfo>();
            var newer = new ConcurrentBag<FileInfo>();
            var listFromFileSystem = ListFromFileSystem();

            if (listFromSetting.Any() && listFromFileSystem.Any())
            {
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
            }
            return newer.ToList();
        }

        /// <summary>
        /// </summary>
        /// <exception cref="AggregateException">The exception that contains all the individual exceptions thrown on all threads.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied. </exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
        /// <exception cref="IOException">
        ///     path includes an incorrect or invalid syntax for file name, directory name, or volume label syntax.
        /// </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        public void Write()
        {
            var pathsForSetting = new ConcurrentBag<FileWatcherHelper>();

            Parallel.ForEach(ListFromFileSystem(),
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
            StreamWriter streamWriter = null;
            try
            {
                var serialiser = new XmlSerializer(typeof(List<FileWatcherHelper>));
                streamWriter = new StreamWriter(_xmlPath);
                serialiser.Serialize(streamWriter, pathsForSetting.ToList());
            }
            finally
            {
                streamWriter?.Close();
            }
        }

        private List<FileWatcherHelper> ConvertPathsFromSettingToDictionary()
        {
            StreamReader streamReader = null;
            try
            {
                var serialiser = new XmlSerializer(typeof(List<FileWatcherHelper>));
                var paths = new ConcurrentBag<FileWatcherHelper>();
                streamReader = new StreamReader(_xmlPath);

                var pathsFromSetting = (List<FileWatcherHelper>) serialiser.Deserialize(streamReader);


                Parallel.ForEach(pathsFromSetting,
                    pathFromSetting =>
                    {
                        var fileWatcherHelper = new FileWatcherHelper
                                                {
                                                    Path = pathFromSetting.Path,
                                                    LastWriteTime = pathFromSetting.LastWriteTime
                                                };
                        paths.Add(fileWatcherHelper);
                    });

                return paths.ToList();
            }
            finally
            {
                streamReader?.Close();
            }
        }

        private List<string> ListFromFileSystem()
        {
            var pathsToWatch = _app.PathsToWatch;
            var paths = new ConcurrentBag<string>();
            var excludeExtensionList = new List<string>
                                       {
                                           "ruleset",
                                           "dll",
                                           "config",
                                           "pdb",
                                           "xml"
                                       };
            Parallel.ForEach(pathsToWatch, pathToWath => { Parallel.ForEach(_filePath.GetFileList(pathToWath, null, excludeExtensionList), path => { paths.Add(path); }); });
            return paths.ToList();
        }
    }
}