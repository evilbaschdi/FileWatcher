using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;
using EvilBaschdi.About.Core;
using EvilBaschdi.About.Core.Models;
using EvilBaschdi.About.Wpf;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.CoreExtended;
using FileWatcher.Internal;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace FileWatcher;

/// <inheritdoc cref="MetroWindow" />
/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
// ReSharper disable once RedundantExtendsListEntry
public partial class MainWindow : MetroWindow
{
    private readonly App _app;
    private readonly ICompareFileLists _compareFileLists;
    private readonly IWriteFileListToDb _writeFileListToDb;
    private List<FileInfo> _list;
    private string _message;

    /// <inheritdoc />
    public MainWindow()
    {
        InitializeComponent();

        IApplicationStyle applicationStyle = new ApplicationStyle(true);
        applicationStyle.Run();

        WindowState = WindowState.Minimized;
        _app = (App)Application.Current;

        IFileListFromPath fileListFromPath = new FileListFromPath();
        IListFromFileSystem listFromFileSystem = new ListFromFileSystem(_app, fileListFromPath);
        _compareFileLists = new CompareFileLists(listFromFileSystem, _app);
        _writeFileListToDb = new WriteFileListToDb(listFromFileSystem, _app);

        LoadAsync();
    }

    private void SetFileGrid()
    {
        var listCollectionView = new ListCollectionView(_list);
        listCollectionView.GroupDescriptions?.Add(new PropertyGroupDescription("DirectoryName"));

        FileGrid.SetCurrentValue(System.Windows.Controls.ItemsControl.ItemsSourceProperty, listCollectionView);
    }

    private async void LoadAsync()
    {
        var task = Task.Factory.StartNew(Compare);
        await task;

        if (!string.IsNullOrWhiteSpace(_message))
        {
            SetCurrentValue(ShowInTaskbarProperty, true);
            SetCurrentValue(WindowStateProperty, WindowState.Normal);
            await this.ShowMessageAsync("directories with changed files:", _message);
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
        ICurrentAssembly currentAssembly = new CurrentAssembly();
        IAboutContent aboutContent = new AboutContent(currentAssembly);
        IAboutModel aboutModel = new AboutViewModel(aboutContent);
        var aboutWindow = new AboutWindow(aboutModel);

        aboutWindow.ShowDialog();
    }
}