using System.IO;
using System.Windows.Input;
using MediaControl.Events;
using MediaControl.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaControl.Views
{
    /// <summary>
    /// Interaction logic for MediaView.xaml
    /// </summary>
    public partial class MediaView : UserControl
    {
        public enum MediaPlay
        {
            Forward,
            Rewind
        }

        private Window _parentWindow;
                
        private DispatcherTimer _timer;
        private bool _isDragging;

        private double[] speedRatio = new double[] { 1.0, 2.0, 4.0, 8.0, 16.0 };
        private int speedIndex = 0;

        private MediaPlay _mediaPlay;

        public MediaView()
        {
            InitializeComponent();

            var viewModel = new MediaViewModel();

            InitMediaPlayer(viewModel);
        }

        public MediaView(MediaViewModel viewModel)
        {
            InitializeComponent();

            InitMediaPlayer(viewModel);
        }

        private void InitMediaPlayer(MediaViewModel viewModel)
        {
            _mediaPlay = MediaPlay.Forward;
            _isDragging = false;

            this.DataContext = viewModel;

            viewModel.PlayRequested += (sender, e) => media.Play();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(200);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Stop();

            media.MediaOpened += (o, e) =>
            {
                _timer.Start();

                media.ScrubbingEnabled = false;

                sliderVolume.IsEnabled = media.IsLoaded;

                if (media.NaturalDuration.HasTimeSpan)
                {
                    var ts = media.NaturalDuration.TimeSpan;
                    sliderTime.Minimum = 0;
                    sliderTime.Maximum = ts.TotalSeconds;
                    sliderTime.SmallChange = 1;
                    sliderTime.LargeChange = Math.Min(10, ts.Seconds / 10);

                    timeEnd.Content = new DateTime(ts.Ticks).ToString("mm:ss");
                }

            };

            media.MediaFailed += (o, e) =>
            {
                //to do: think if here or in viewmodel to report of failed media?!
            };

            media.MediaEnded += (o, e) => { media.Stop(); btnPlay.IsChecked = false; };

            sliderVolume.IsEnabled = false;

            this.Loaded += (sender, e) => { _parentWindow = Window.GetWindow(this); _parentWindow.Background = Brushes.Black; };

            viewModel.ScreenSizeChangeRequested += (sender, e) => { setFullScreen(((ScreenSizeEventArgs)e).Fullscreen); };
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (!_isDragging && _mediaPlay == MediaPlay.Forward)
            {
                sliderTime.Value = media.Position.TotalSeconds;
            }
            else
            {
                media.Position = media.Position.Subtract(new TimeSpan(0, 0, 1));
                sliderTime.Value = media.Position.TotalSeconds;

                if (media.Position.Milliseconds <= 0)
                {
                    btnFF.ToolTip = null;
                    _mediaPlay = MediaPlay.Forward;
                    btnPlay.IsChecked = true;
                    media.Stop();
                }
            }

            timeStart.Content = new DateTime(media.Position.Ticks).ToString("mm:ss");
        }
                  
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)btnPlay.IsChecked)
            {
                media.Pause();
                if (_mediaPlay == MediaPlay.Rewind)
                    _timer.Stop();
                else
                {
                    _timer.Start();
                }
            }
            else
            {                
                media.ScrubbingEnabled = false;
                _mediaPlay = MediaPlay.Forward;
                speedIndex = 0;
                media.SpeedRatio = speedRatio[speedIndex];
                btnFF.ToolTip = ((int)media.SpeedRatio).ToString() + "X";
                media.Volume = (double)sliderVolume.Value;
                media.Play();
                System.Threading.Thread.Sleep(100);
                _timer.Start();              
            }            
        }
              
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnFF.ToolTip = null;
            _mediaPlay = MediaPlay.Forward;
            btnPlay.IsChecked = true;
            media.Stop();
            sliderTime.Value = 0;
        }

        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Volume = (double)sliderVolume.Value;
        }

        private void setFullScreen(bool FullScreenMode)
        {
            if (FullScreenMode)
            {
                _parentWindow.WindowStyle = WindowStyle.None;
                _parentWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                _parentWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                _parentWindow.WindowState = WindowState.Normal;
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            btnFF.ToolTip = null;
            btnPlay.IsChecked = false;
            media.Stop();
            speedIndex = 0;
            media.SpeedRatio = speedRatio[speedIndex];
            btnFF.ToolTip = ((int)media.SpeedRatio).ToString() + "X";
            _mediaPlay = MediaPlay.Forward;
            media.ScrubbingEnabled = false;
            media.Play();
        }
                
        private void btnRW_Click(object sender, RoutedEventArgs e)
        {
            btnFF.ToolTip = null;
            _mediaPlay = MediaPlay.Rewind;
            media.Pause();
            media.ScrubbingEnabled = true;
        }

        private void btnFF_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlay == MediaPlay.Rewind)
            {
                speedIndex = 0;
                _mediaPlay = MediaPlay.Forward;
                media.Play();
            }
            else
            {
                _mediaPlay = MediaPlay.Forward;
                if (speedIndex < speedRatio.Length - 1)
                {
                    speedIndex++;
                }
                else
                {
                    speedIndex = 0;
                }
            }

            media.SpeedRatio = speedRatio[speedIndex];
            btnFF.ToolTip = ((int)media.SpeedRatio).ToString() + "X";
        }

        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)btnVolume.IsChecked)
            {
                media.IsMuted = true;
            }
            else
            {
                media.IsMuted = false;
            }
        }    

        private void sliderTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Position = TimeSpan.FromSeconds(sliderTime.Value);
        }

        private void MediaView_OnDrop(object sender, DragEventArgs e)
        {
            var fileName = (String[])e.Data.GetData(DataFormats.FileDrop, true);
            if (fileName.Length > 0)
            {
                var videoPath = fileName[0];
                var title = Path.GetFileName(videoPath);
             
                var mediaViewModel = (MediaViewModel)this.DataContext;
                mediaViewModel.MediaUrl = videoPath;
                mediaViewModel.MediaTitle = title;
                mediaViewModel.Play();
            }
            e.Handled = true; 
        }
    }
}
