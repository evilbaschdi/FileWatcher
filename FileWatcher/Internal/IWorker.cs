using System;
using System.Collections.Generic;
using System.IO;
using System.Security;

namespace FileWatcher.Internal
{
    /// <summary>
    /// </summary>
    public interface IWorker
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AggregateException">The exception that contains all the individual exceptions thrown on all threads.</exception>
        List<FileInfo> Compare();

        /// <summary>
        /// </summary>
        /// <exception cref="AggregateException">The exception that contains all the individual exceptions thrown on all threads.</exception>
        /// <exception cref="UnauthorizedAccessException">Access is denied. </exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
        /// <exception cref="IOException">
        ///     path includes an incorrect or invalid syntax for file name, directory name, or volume label syntax.
        /// </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        void Write();
    }
}