using System;
using System.Collections.Generic;
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
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            var config = System.Configuration.ConfigurationManager.AppSettings;
            var styleAccent = ThemeManager.GetAccent(config["Accent"]);
            var styleTheme = ThemeManager.GetAppTheme(config["Theme"]);
            ThemeManager.ChangeAppStyle(Current, styleAccent, styleTheme);

            base.OnStartup(e);

            if (e.Args.Any())
            {
                PathsToWatch = new List<string>();

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