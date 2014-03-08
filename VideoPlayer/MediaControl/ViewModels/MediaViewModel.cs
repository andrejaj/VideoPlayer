using System;
using System.ComponentModel;
using Microsoft.Practices.Prism.Commands;
using MediaControl.Events;

namespace MediaControl.ViewModels
{
    public class MediaViewModel : INotifyPropertyChanged
    {
        private string _mediaUrl;
        private string _mediaTitle;
        private bool _isFullscreen;
      
        public string MediaUrl 
        {
            get { return _mediaUrl; }
            set { _mediaUrl = value; OnPropertyChanged("MediaUrl"); }
        }

        public string MediaTitle
        {
            get { return _mediaTitle; }
            set { _mediaTitle = value; OnPropertyChanged("MediaTitle"); }
        }

        public event EventHandler PlayRequested;
        public event EventHandler ScreenSizeChangeRequested;
    
        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand ResizeCommand { get; private set; }
        public DelegateCommand EscCommand { get; private set; }

        public MediaViewModel()
        {
            _isFullscreen = false;
            
            ResizeCommand = new DelegateCommand(OnResizeCommandExecuted);
            EscCommand = new DelegateCommand(OnEscCommandExecuted);
        }

        private void OnResizeCommandExecuted()
        {
            _isFullscreen = !_isFullscreen;

            if (this.ScreenSizeChangeRequested != null)
            {
                this.ScreenSizeChangeRequested(this, new ScreenSizeEventArgs(_isFullscreen));
            }
        }

        private void OnEscCommandExecuted()
        {
            if (_isFullscreen)
            {
                _isFullscreen = !_isFullscreen;

                if (this.ScreenSizeChangeRequested != null)
                {
                    this.ScreenSizeChangeRequested(this, new ScreenSizeEventArgs(_isFullscreen));
                }
            }          
        }
        
        public void Play()
        {
            if (!string.IsNullOrEmpty(MediaUrl))
            {
                if (this.PlayRequested != null)
                {
                    this.PlayRequested(this, EventArgs.Empty);
                }
            }
        }

        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
