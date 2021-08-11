using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using ControlzEx.Theming;

namespace FileWatcher
{
    /// <inheritdoc />
    /// <inheritdoc cref="Application" />
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        /// <summary>
        ///     Path to store json db file.
        /// </summary>
        public string JsonPath { get; private set; }

        /// <summary>
        ///     List of paths the app should scan.
        /// </summary>
        public IList<string> PathsToWatch { get; private set; }

        /// <inheritdoc />
        /// <summary>
        ///     Löst das <see cref="E:System.Windows.Application.Startup" />-Ereignis aus.
        /// </summary>
        /// <param name="e">Ein <see cref="T:System.Windows.StartupEventArgs" />, das die Ereignisdaten enthält.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="e" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">
        ///     Could not retrieve a
        ///     <see cref="T:System.Collections.Specialized.NameValueCollection" /> object with the application settings data.
        /// </exception>
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }


            ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncAll);

            var config = ConfigurationManager.AppSettings;
            JsonPath = $@"{config["XmlDbPath"]?.TrimEnd('\\')}\fileWatcher.json";
            var pathsToWatch = config["PathsToWatch"];

            base.OnStartup(e);

            PathsToWatch = new List<string>();

            if (!string.IsNullOrWhiteSpace(pathsToWatch))
            {
                var paths = pathsToWatch.Split(';');
                foreach (var path in paths)
                {
                    AggregatePathsToWatch(path);
                }
            }

            if (!e.Args.Any())
            {
                return;
            }

            {
                foreach (var arg in e.Args)
                {
                    var path = arg.TrimStart('"').TrimEnd('"');
                    AggregatePathsToWatch(path);
                }
            }
        }

        private void AggregatePathsToWatch(string path)
        {
            if (Directory.Exists(path))
            {
                PathsToWatch.Add(path);
            }
        }
    }
}