using System.Windows;

namespace Media
{
    public partial class App : Application
    {
        public static string filePath;
        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args != null && e.Args.Length > 0) filePath = e.Args[0];
            //filePath = "D://Videos/9.mp4";
            //filePath = "D:/Windows/Pictures/Screenshot (69).png";
        }
    }
}