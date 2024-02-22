using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using static Media.App;
using System.Windows.Threading;

namespace Media
{
    public partial class MainWindow : Window
    {
        public readonly List<string> IMAGE_EXTENSIONS = new List<string>() { ".png", ".jpg", ".jpeg", ".svg" };
        public readonly List<string> VIDEO_EXTENSIONS = new List<string>() { ".mp4" };
        public readonly List<string> AUDIO_EXTENSIONS = new List<string>() { ".mp3" };

        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += (s, e) =>
            {
                timelineSlider.Value = mediaDisplayer.MediaPosition;

                var mediaPosition = TimeSpan.FromTicks(mediaDisplayer.MediaPosition);
                var mediaDuration = TimeSpan.FromTicks(mediaDisplayer.MediaDuration);

                TimeLabel.Content = $"{mediaDuration.ToString(@"h\:mm\:ss")}/{mediaPosition.ToString(@"h\:mm\:ss")}";
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (filePath != null &&
                IsAcceptable(filePath))
            {
                mediaDisplayer.Source = new Uri(filePath);
                mediaDisplayer.Play();
            }
        }

        #region window control
        bool isDraging = false;
        private void SwitchState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    break;
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    break;
            }
        }

        private void MinimizeWindowButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ResizeWindowButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchState();
        }

        private void ExitWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DockPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraging == true)
            {
                isDraging = false;

                double percentHorizontal = e.GetPosition(this).X / ActualWidth;
                double targetHorizontal = RestoreBounds.Width * percentHorizontal;

                double percentVertical = e.GetPosition(this).Y / ActualHeight;
                double targetVertical = RestoreBounds.Height * percentVertical;

                WindowState = WindowState.Normal;

                System.Drawing.Point lMousePosition = System.Windows.Forms.Cursor.Position;

                Left = lMousePosition.X - targetHorizontal;
                Top = lMousePosition.Y - targetVertical;

                DragMove();
            }
        }

        private void DockPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDraging = false;
        }

        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) SwitchState();
            else isDraging = true;
        }
        #endregion

        private bool IsAcceptable(string path)
        {
            if (IMAGE_EXTENSIONS.Contains(Path.GetExtension(path)) ||
                VIDEO_EXTENSIONS.Contains(Path.GetExtension(path)) ||
                AUDIO_EXTENSIONS.Contains(Path.GetExtension(path))) return true;
            else return false;
        }

        private bool IsVideoAudio(string path)
        {
            if (VIDEO_EXTENSIONS.Contains(Path.GetExtension(path)) ||
                AUDIO_EXTENSIONS.Contains(Path.GetExtension(path))) return true;
            else return false;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        { 
            if (filePath != null)
            {
                string folder = Path.GetDirectoryName(filePath);
                string[] files = Directory.GetFiles(folder);
                int index = Array.IndexOf(files, Path.GetFullPath(filePath));
                
                if (files.Length > index + 1 && IsAcceptable(files[index + 1]))
                {
                    mediaDisplayer.Source = new Uri(files[index + 1]);
                    mediaDisplayer.Play();
                    filePath = files[index + 1];
                }
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (filePath != null)
            {
                string folder = Path.GetDirectoryName(filePath);
                string[] files = Directory.GetFiles(folder);
                int index = Array.IndexOf(files, Path.GetFullPath(filePath));

                if (index - 1 >= 0 && IsAcceptable(files[index - 1]))
                {
                    mediaDisplayer.Source = new Uri(files[index - 1]);
                    mediaDisplayer.Play();
                    filePath = files[index - 1];
                }
            }
        }

        bool isPaused = false;
        void OnMouseDownPausePlayMedia(object sender, MouseButtonEventArgs args)
        {
            if (!isPaused)
            {
                PlayPauseButton.Source = new BitmapImage(new Uri("Resources/Images/play.png", UriKind.Relative));
                mediaDisplayer.Pause();
            }
            else
            {
                PlayPauseButton.Source = new BitmapImage(new Uri("Resources/Images/pause.png", UriKind.Relative));
                mediaDisplayer.Play();
            }

            isPaused = !isPaused;
        }

        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            mediaDisplayer.Volume = (double)volumeSlider.Value;
        }

        private void mediaDisplayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            isPaused = false;
            PlayPauseButton.Source = new BitmapImage(new Uri("Resources/Images/pause.png", UriKind.Relative));
            
            if(AUDIO_EXTENSIONS.Contains(Path.GetExtension(filePath))) MusicImage.Visibility = Visibility.Visible;
            else MusicImage.Visibility = Visibility.Collapsed;

            FileNameLabel.Content = filePath;

            if (IsVideoAudio(filePath))
            {
                mediaDisplayer.Volume = (double)volumeSlider.Value;
                //timelineSlider.Maximum = mediaDisplayer.NaturalDuration.TimeSpan.TotalSeconds;
                timelineSlider.Maximum = mediaDisplayer.MediaDuration;
                timer.Start();
                VideoControls.Visibility = Visibility.Visible;
            }
            else
            {
                timer.Stop();
                VideoControls.Visibility = Visibility.Collapsed;
            }
        }

        private void timelineSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
        }

        private void timelineSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //mediaDisplayer.Position = new TimeSpan(0, 0, 0, (int)timelineSlider.Value);
            mediaDisplayer.MediaPosition = (long)timelineSlider.Value;

            timer.Start();
        }
    }
}