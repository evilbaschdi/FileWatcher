using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using EvilBaschdi.Core.Extensions;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.CoreExtended;
using EvilBaschdi.CoreExtended.AppHelpers;
using EvilBaschdi.CoreExtended.Metro;
using FileWatcher.Internal;
using FileWatcher.Properties;
using MahApps.Metro.Controls;

namespace FileWatcher
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : MetroWindow
    {
        private readonly IWorker _worker;
        private string _message;
        private List<FileInfo> _list;
        private readonly App _app;

        private readonly IDialogService _dialogService;
        private readonly int _overrideProtection;
        private readonly IApplicationStyle _applicationStyle;


        /// <inheritdoc />
        public MainWindow()
        {
            InitializeComponent();
            IAppSettingsBase appSettingsBase = new AppSettingsBase(Settings.Default);
            IApplicationStyleSettings applicationStyleSettings = new ApplicationStyleSettings(appSettingsBase);
            IThemeManagerHelper themeManagerHelper = new ThemeManagerHelper();
            _applicationStyle = new ApplicationStyle(this, Accent, ThemeSwitch, applicationStyleSettings, themeManagerHelper);
            _applicationStyle.Load(true);
            _dialogService = new DialogService(this);
            var linkerTime = Assembly.GetExecutingAssembly().GetLinkerTime();
            LinkerTime.Content = linkerTime.ToString(CultureInfo.InvariantCulture);
            WindowState = WindowState.Minimized;
            _app = (App) Application.Current;
            var multiThreadingHelper = new MultiThreading();
            var filePath = new FileListFromPath(multiThreadingHelper);
            _worker = new Worker(filePath, _app);


            LoadAsync();
            _overrideProtection = 1;
        }

        private void SetFileGrid()
        {
            var listCollectionView = new ListCollectionView(_list);
            listCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("DirectoryName"));
            FileGrid.ItemsSource = listCollectionView;
        }

        private async void LoadAsync()
        {
            var task = Task.Factory.StartNew(Compare);
            await task;

            if (!string.IsNullOrWhiteSpace(_message))
            {
                ShowInTaskbar = true;
                WindowState = WindowState.Normal;
                await _dialogService.ShowMessage("directories with changed files:", _message);
                SetFileGrid();
            }
            else
            {
                Close();
            }
        }

        private void Compare()
        {
            if (!File.Exists(_app.XmlPath))
            {
                _worker.Write();
            }

            _list = _worker.Compare();

            _message = string.Join(Environment.NewLine, _list.Select(item => item.DirectoryName).Distinct());

            _worker.Write();
        }


        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #region Flyout

        private void ToggleSettingsFlyoutClick(object sender, RoutedEventArgs e)
        {
            ToggleFlyout(0);
        }

        private void ToggleFlyout(int index, bool stayOpen = false)
        {
            var activeFlyout = (Flyout) Flyouts.Items[index];
            if (activeFlyout == null)
            {
                return;
            }

            foreach (
                var nonactiveFlyout in
                Flyouts.Items.Cast<Flyout>()
                       .Where(nonactiveFlyout => nonactiveFlyout.IsOpen && nonactiveFlyout.Name != activeFlyout.Name))
            {
                nonactiveFlyout.IsOpen = false;
            }

            if (activeFlyout.IsOpen && stayOpen)
            {
                activeFlyout.IsOpen = true;
            }
            else
            {
                activeFlyout.IsOpen = !activeFlyout.IsOpen;
            }
        }

        #endregion Flyout

        #region MetroStyle

        private void SaveStyleClick(object sender, RoutedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }

            _applicationStyle.SaveStyle();
        }

        private void Theme(object sender, EventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }

            _applicationStyle.SetTheme(sender);
        }

        private void AccentOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }

            _applicationStyle.SetAccent(sender, e);
        }

        #endregion MetroStyle
    }
}