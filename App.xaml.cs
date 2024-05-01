using System.Windows;
using static Media.MainWindow;

namespace Media
{
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args != null && e.Args.Length > 0) fileUrl = e.Args[0];
            //fileUrl = "D://Videos/9.mp4";
            //fileUrl = "D:/Windows/Pictures/Screenshot (69).png";
        }
    }
}