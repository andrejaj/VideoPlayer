using System;

namespace MediaControl.Events
{
    public class ScreenSizeEventArgs : EventArgs
    {
        public bool Fullscreen { get; private set; }

        public ScreenSizeEventArgs(bool fullscreen)
        {
            Fullscreen = fullscreen;
        }
    }
}
