using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using MahApps.Metro;

namespace FileWatcher
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        ///     List of paths the app should scan.
        /// </summary>
        public IList<string> PathsToWatch { get; set; }

        /// <summary>
        ///     Löst das <see cref="E:System.Windows.Application.Startup" />-Ereignis aus.
        /// </summary>
        /// <param name="e">Ein <see cref="T:System.Windows.StartupEventArgs" />, das die Ereignisdaten enthält.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e" /> is <see langword="null" />.</exception>
        /// <exception cref="ConfigurationErrorsException">
        ///     Could not retrieve a
        ///     <see cref="T:System.Collections.Specialized.NameValueCollection" /> object with the application settings data.
        /// </exception>
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            var config = ConfigurationManager.AppSettings;
            var styleAccent = ThemeManager.GetAccent(config["Accent"]);
            var styleTheme = ThemeManager.GetAppTheme(config["Theme"]);
            ThemeManager.ChangeAppStyle(Current, styleAccent, styleTheme);

            base.OnStartup(e);

            PathsToWatch = new List<string>();

            if (e.Args.Any())
            {
                foreach (var arg in e.Args)
                {
                    var local = arg.TrimStart('"').TrimEnd('"');
                    if (Directory.Exists(local))
                    {
                        PathsToWatch.Add(local);
                    }
                }
            }
        }
    }
}