using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace Media
{
    public partial class MainWindow : Window
    {
        public List<string> IMAGE_EXTENSIONS = new List<string>() { ".png", ".jpg", ".jpeg", ".svg", ".webp" };
        public List<string> VIDEO_EXTENSIONS = new List<string>() { ".mp4", ".mov" };
        public List<string> AUDIO_EXTENSIONS = new List<string>() { ".mp3" };

        DispatcherTimer timer = new DispatcherTimer();

        public static string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string myAppFolder = Path.Combine(appDataPath, "KUBIXQAZ/Media");
        public static string imageExtensionsFilePath = Path.Combine(myAppFolder, "imageExtensions.json");
        public static string videoExtensionsFilePath = Path.Combine(myAppFolder, "videoExtensions.json");
        public static string audioExtensionsFilePath = Path.Combine(myAppFolder, "audioExtensions.json");

        public static string fileUrl;
        public bool loop = false;
        public bool startPaused = false;

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

                Console.WriteLine($"startpaused {startPaused} paused {isPaused}");
            };

            Load_Extensions();
        }

        private void Load_Extensions()
        {
            if(File.Exists(imageExtensionsFilePath))
            {
                string json = File.ReadAllText(imageExtensionsFilePath);

                IMAGE_EXTENSIONS = JsonConvert.DeserializeObject<List<string>>(json);
            }

            if (File.Exists(videoExtensionsFilePath))
            {
                string json = File.ReadAllText(videoExtensionsFilePath);

                VIDEO_EXTENSIONS = JsonConvert.DeserializeObject<List<string>>(json);
            }

            if (File.Exists(audioExtensionsFilePath))
            {
                string json = File.ReadAllText(audioExtensionsFilePath);

                AUDIO_EXTENSIONS = JsonConvert.DeserializeObject<List<string>>(json);
            }
        }

        private void Save_Extensions()
        {
            string jsonImageExtensions = JsonConvert.SerializeObject(IMAGE_EXTENSIONS);
            string jsonVideoExtensions = JsonConvert.SerializeObject(VIDEO_EXTENSIONS);
            string jsonAudioExtensions = JsonConvert.SerializeObject(AUDIO_EXTENSIONS);

            if(!Directory.Exists(myAppFolder)) Directory.CreateDirectory(myAppFolder);

            File.WriteAllText(imageExtensionsFilePath, jsonImageExtensions);
            File.WriteAllText(videoExtensionsFilePath, jsonVideoExtensions);
            File.WriteAllText(audioExtensionsFilePath, jsonAudioExtensions);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (fileUrl != null && IsAcceptable(fileUrl))
            {
                mediaDisplayer.Source = new Uri(fileUrl);
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
            if (IMAGE_EXTENSIONS.Contains(Path.GetExtension(path).ToLower()) ||
                VIDEO_EXTENSIONS.Contains(Path.GetExtension(path).ToLower()) ||
                AUDIO_EXTENSIONS.Contains(Path.GetExtension(path).ToLower())) return true;
            else return false;
        }

        private bool IsVideoAudio(string path)
        {
            if (VIDEO_EXTENSIONS.Contains(Path.GetExtension(path).ToLower()) ||
                AUDIO_EXTENSIONS.Contains(Path.GetExtension(path).ToLower())) return true;
            else return false;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        { 
            if (fileUrl != null)
            {
                string folder = Path.GetDirectoryName(fileUrl);
                string[] files = Directory.GetFiles(folder);
                int index = Array.IndexOf(files, Path.GetFullPath(fileUrl));
                
                if (files.Length > index + 1 && IsAcceptable(files[index + 1]))
                {
                    startPaused = false;

                    mediaDisplayer.Source = new Uri(files[index + 1]);
                    mediaDisplayer.Play();
                    fileUrl = files[index + 1];
                }
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (fileUrl != null)
            {
                string folder = Path.GetDirectoryName(fileUrl);
                string[] files = Directory.GetFiles(folder);
                int index = Array.IndexOf(files, Path.GetFullPath(fileUrl));

                if (index - 1 >= 0 && IsAcceptable(files[index - 1]))
                {
                    startPaused = false;

                    mediaDisplayer.Source = new Uri(files[index - 1]);
                    mediaDisplayer.Play();
                    fileUrl = files[index - 1];
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
            OpenFileButtonBar.Visibility = Visibility.Visible;
            OpenFileButton.Visibility = Visibility.Collapsed;

            if (!startPaused)
            {
                isPaused = false;
                PlayPauseButton.Source = new BitmapImage(new Uri("Resources/Images/pause.png", UriKind.Relative));
            } else
            {
                isPaused = true;
                PlayPauseButton.Source = new BitmapImage(new Uri("Resources/Images/play.png", UriKind.Relative));
            }
            
            if(AUDIO_EXTENSIONS.Contains(Path.GetExtension(fileUrl))) MusicImage.Visibility = Visibility.Visible;
            else MusicImage.Visibility = Visibility.Collapsed;

            FileNameLabel.Content = fileUrl;

            NextButton.Visibility = Visibility.Visible;
            PreviousButton.Visibility = Visibility.Visible;
            FileNameLabel.Visibility = Visibility.Visible;

            if (IsVideoAudio(fileUrl))
            {
                mediaDisplayer.Volume = (double)volumeSlider.Value;
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
            mediaDisplayer.MediaPosition = (long)timelineSlider.Value;
            timer.Start();
        }

        private void mediaDisplayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaDisplayer.Source = new Uri(fileUrl);

            if (loop == false)
            {
                startPaused = true;

                mediaDisplayer.MediaPosition = 0;

                PlayPauseButton.Source = new BitmapImage(new Uri("Resources/Images/play.png", UriKind.Relative));
                mediaDisplayer.Pause();
                isPaused = true;
            } else
            {
                startPaused = false;
                isPaused = false;
                PlayPauseButton.Source = new BitmapImage(new Uri("Resources/Images/play.png", UriKind.Relative));
                mediaDisplayer.Play();
            }
        }

        private void RepeatButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            loop = !loop;

            if(loop)
            {
                RepeatButton.Source = new BitmapImage(new Uri("Resources/Images/arrow_repeat_on.png", UriKind.Relative));
            } else
            {
                RepeatButton.Source = new BitmapImage(new Uri("Resources/Images/arrow_repeat_off.png", UriKind.Relative));
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                fileUrl = dialog.FileName;
                mediaDisplayer.Source = new Uri(fileUrl);
                mediaDisplayer.Play();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}