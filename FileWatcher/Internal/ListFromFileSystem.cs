using System.Collections.Concurrent;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.Core.Model;

namespace FileWatcher.Internal;

/// <inheritdoc />
public class ListFromFileSystem : IListFromFileSystem
{
    private readonly App _app;
    private readonly IFileListFromPath _fileListFromPath;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="app"></param>
    /// <param name="fileListFromPath"></param>
    public ListFromFileSystem(App app, IFileListFromPath fileListFromPath)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _fileListFromPath = fileListFromPath ?? throw new ArgumentNullException(nameof(fileListFromPath));
    }

    /// <inheritdoc />
    public List<string> Value
    {
        get
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
            var filter = new FileListFromPathFilter
                         {
                             FilterExtensionsNotToEqual = excludeExtensionList
                         };
            Parallel.ForEach(pathsToWatch, pathToWath => { Parallel.ForEach(_fileListFromPath.ValueFor(pathToWath, filter), path => { paths.Add(path); }); });
            return paths.ToList();
        }
    }
}