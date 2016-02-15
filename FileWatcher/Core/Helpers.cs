using System;
using System.IO;
using System.Linq;

namespace FileWatcher.Core
{
    /// <summary>
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// </summary>
        public static bool IsAccessible(this string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            //get directory info
            var realpath = new DirectoryInfo(path);
            try
            {
                //if GetDirectories works then is accessible
                realpath.GetDirectories();
                return true;
            }
            catch (Exception)
            {
                //if exception is not accesible
                return false;
            }
        }

        /// <summary>
        /// </summary>
        public static double GetDirectorySize(this DirectoryInfo dir)
        {
            if (dir == null)
            {
                throw new ArgumentNullException(nameof(dir));
            }
            var sum = dir.GetFiles().Aggregate<FileInfo, double>(0, (current, file) => current + file.Length);
            return dir.GetDirectories().Aggregate(sum, (current, dir1) => current + GetDirectorySize(dir1));
        }
    }
}