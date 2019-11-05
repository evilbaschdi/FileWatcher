using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.CoreExtended;
using EvilBaschdi.CoreExtended.Metro;
using EvilBaschdi.CoreExtended.Mvvm;
using EvilBaschdi.CoreExtended.Mvvm.View;
using EvilBaschdi.CoreExtended.Mvvm.ViewModel;
using FileWatcher.Internal;
using MahApps.Metro.Controls;

namespace FileWatcher
{
    /// <inheritdoc cref="MetroWindow" />
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : MetroWindow
    {
        private readonly App _app;
        private readonly ICompareFileLists _compareFileLists;
        private readonly IDialogService _dialogService;
        private readonly IThemeManagerHelper _themeManagerHelper;
        private readonly IWriteFileListToDb _writeFileListToDb;
        private List<FileInfo> _list;
        private string _message;


        /// <inheritdoc />
        public MainWindow()
        {
            InitializeComponent();
            _themeManagerHelper = new ThemeManagerHelper();
            IApplicationStyle applicationStyle = new ApplicationStyle(_themeManagerHelper);
            applicationStyle.Load(true);
            _dialogService = new DialogService(this);
            WindowState = WindowState.Minimized;
            _app = (App) Application.Current;
            IMultiThreading multiThreadingHelper = new MultiThreading();
            IFileListFromPath fileListFromPath = new FileListFromPath(multiThreadingHelper);
            IListFromFileSystem listFromFileSystem = new ListFromFileSystem(_app, fileListFromPath);
            _compareFileLists = new CompareFileLists(listFromFileSystem, _app);
            _writeFileListToDb = new WriteFileListToDb(listFromFileSystem, _app);

            LoadAsync();
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
            if (!File.Exists(_app.JsonPath))
            {
                _writeFileListToDb.Run();
            }

            _list = _compareFileLists.Value;

            _message = string.Join(Environment.NewLine, _list.Select(item => item.DirectoryName).Distinct());

            _writeFileListToDb.Run();
        }


        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void AboutWindowClick(object sender, RoutedEventArgs e)
        {
            var assembly = typeof(MainWindow).Assembly;
            IAboutWindowContent aboutWindowContent = new AboutWindowContent(assembly, $@"{AppDomain.CurrentDomain.BaseDirectory}\b.png");

            var aboutWindow = new AboutWindow
                              {
                                  DataContext = new AboutViewModel(aboutWindowContent, _themeManagerHelper)
                              };

            aboutWindow.ShowDialog();
        }
    }
}