using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;
using EvilBaschdi.Core.DirectoryExtensions;
using EvilBaschdi.Core.MultiThreading;
using FileWatcher.Internal;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace FileWatcher
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly Worker _worker;
        private string _message;
        private List<FileInfo> _list;
        private readonly IMultiThreadingHelper _multiThreadingHelper;

        /// <summary>
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Minimized;
            var app = (App) Application.Current;
            _multiThreadingHelper = new MultiThreadingHelper();
            var filePath = new FilePath(_multiThreadingHelper);
            _worker = new Worker(filePath, app);

            using (var backgroundWorker = new BackgroundWorker())
            {
                backgroundWorker.DoWork += (sender, args) => Load();
                backgroundWorker.WorkerReportsProgress = true;
                backgroundWorker.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_message))
            {
                ShowInTaskbar = true;
                WindowState = WindowState.Normal;
                ShowMessage("directories with changed files:", _message);
                SetFileGrid();
            }
            else
            {
                Close();
            }
        }


        private void SetFileGrid()
        {
            var listCollectionView = new ListCollectionView(_list);
            listCollectionView.GroupDescriptions?.Add(new PropertyGroupDescription("DirectoryName"));
            FileGrid.ItemsSource = listCollectionView;
        }


        private void Load()
        {
            var xmlPath = @"c:\temp\fileWatcher.xml";
            if (!File.Exists(xmlPath))
            {
                _worker.Write();
            }
            _list = _worker.Compare();

            _message = string.Empty;

            foreach (var dir in _list.Select(item => item.DirectoryName).Distinct())
            {
                _message += $"{dir}{Environment.NewLine}";
            }

            _worker.Write();
        }

        /// <summary>
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public async void ShowMessage(string title, string message)
        {
            var options = new MetroDialogSettings
                          {
                              ColorScheme = MetroDialogColorScheme.Theme
                          };

            MetroDialogOptions = options;
            await this.ShowMessageAsync(title, message);
        }

        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}