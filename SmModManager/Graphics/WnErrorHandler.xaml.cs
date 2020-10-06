using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using SmModManager.Core;

namespace SmModManager.Graphics
{

    public partial class WnErrorHandler
    {

        public static string BackBlazeFolder = Path.Combine(Constants.Resources, "Api", "BackBlaze", "report");
        public static string RunBackBlaze = Path.Combine(Constants.Resources, "Api", "BackBlaze", "report", "b2.bat");
        public static string ReportBucket = "SmUserReports";
        public static string BackBlazeApiKey = "b2f43e974cff 0027e35a1f2600a7c06de4936ea8d91c7784266ab7";
        public Exception Error;
        public string LastText;
        public int limit = 450;

        public WnErrorHandler(Exception error)
        {
            Error = error;
            InitializeComponent();
            MessageText.Text = error.Message;
        }

        private void SendButton(object sender, RoutedEventArgs args)
        {
            var UniqueFileName = App.UserSteamId;
            if (UniqueFileName == null)
                UniqueFileName = Environment.UserName + DateTime.UtcNow.Ticks + "ErrorReport.txt";
            else
                UniqueFileName += "ErrorReport.txt";
            if (File.Exists(Path.Combine(BackBlazeFolder, "FileDetailsErrorReport.json")))
                File.Delete(Path.Combine(BackBlazeFolder, "FileDetailsErrorReport.json"));
            if (File.Exists(Path.Combine(BackBlazeFolder, UniqueFileName)))
                File.Delete(Path.Combine(BackBlazeFolder, UniqueFileName));
            var message = "User Message:\n" + NotesText.Text + "\nError Report:\n" + Error.Message + "\n===\n" + Error.StackTrace + "\n===\n" + Error.Source + "\n===\n" + Error.InnerException + "\n===\n" + Error.TargetSite + "\n===\n" + Error.Data + "\n===\n" + Error.HelpLink + "\n ===\n" + Error.HResult;
            File.WriteAllText(Path.Combine(BackBlazeFolder, UniqueFileName), message);
            var p2Info = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c \"" + RunBackBlaze + "\" " + UniqueFileName + " " + ReportBucket + " " + BackBlazeApiKey + @" .\report\FileDetailsErrorReport.json",
                CreateNoWindow = true
            };
            Process.Start(p2Info);
            Close();
        }

        private void TextLimitCheck(object sender, RoutedEventArgs args)
        {
            if (NotesText.Text.Length > limit)
            {
                var ind = NotesText.CaretIndex;
                if (ind - 1 >= 0)
                {
                    NotesText.Text = NotesText.Text.Remove(ind - 1, 1);
                    NotesText.CaretIndex = ind - 1;
                }
                else
                {
                    NotesText.Text = LastText;
                }
                WnManager.GetWnManager.Notification("Warning: Max Length Reached!");
            }
            LastText = NotesText.Text;
        }

        private void CloseButton(object sender, RoutedEventArgs args)
        {
            Close();
        }

    }

}