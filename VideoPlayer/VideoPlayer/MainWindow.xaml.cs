using System;
using System.Windows;
using MediaControl.ViewModels;
using Microsoft.Win32;

namespace VideoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItemOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();

            if (!openDialog.ShowDialog().Value) return;

            var videoPath = openDialog.FileName;
            var title = openDialog.SafeFileName;

            if (videoPath.Length <= 0) return;

            var mediaViewModel = (MediaViewModel) MediaPlayer.DataContext;
            mediaViewModel.MediaUrl = videoPath;
            mediaViewModel.MediaTitle = title;
            mediaViewModel.Play();
        }

        private void MenuItemClose_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
