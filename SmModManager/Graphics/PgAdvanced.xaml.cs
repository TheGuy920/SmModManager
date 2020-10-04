using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ookii.Dialogs.Wpf;
using SmModManager.Core;
using SmModManager.Core.Enums;

namespace SmModManager.Graphics
{

    public partial class PgAdvanced
    {

        public static string BackBlazeFolder = Path.Combine(Constants.Resources, "Api", "BackBlaze", "report");
        public static string RunBackBlaze = Path.Combine(Constants.Resources, "Api", "BackBlaze", "report", "b2.bat");
        public static string ReportBucket = "SmUserReports";
        public static string BackBlazeApiKey = "b2f43e974cff 0027e35a1f2600a7c06de4936ea8d91c7784266ab7";
        public static bool IsReporting;
        public static PgAdvanced GetPgAdvanced;

        public PgAdvanced()
        {
            GetPgAdvanced = this;
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version == null)
            {
                VersionText.Text = "Unknown";
            }
            else
            {
                VersionText.Text += $"{version.Major}.{version.Minor}";
                #if DEBUG
                VersionText.Text += "-DEBUG";
                #endif
            }
            ChangelogText.Text = Utilities.RetrieveResourceData("SmModManager.Resources.Documents.Changelog.txt");
            CreditsText.Text = Utilities.RetrieveResourceData("SmModManager.Resources.Documents.Credits.txt");
            foreach (var userDataPath in Directory.GetDirectories(Constants.UsersDataPath))
                UserDataPathBox.Items.Add(userDataPath);
            GameDataPathBox.Text = App.Settings.GameDataPath;
            WorkshopPathBox.Text = App.Settings.WorkshopPath;
            UserDataPathBox.Text = App.Settings.UserDataPath;
            UpdateBehaviorBox.SelectedIndex = App.Settings.UpdatePreference switch
            {
                UpdateBehaviorOptions.RemindForUpdates => 1,
                UpdateBehaviorOptions.DontCheckForUpdates => 2,
                _ => 0
            };
        }

        private void ResetSettings(object sender, RoutedEventArgs args)
        {
            App.Settings.Reset();
            Utilities.RestartApp();
        }

        private void SaveSettings(object sender, RoutedEventArgs args)
        {
            App.Settings.GameDataPath = GameDataPathBox.Text;
            App.Settings.WorkshopPath = WorkshopPathBox.Text;
            App.Settings.UserDataPath = UserDataPathBox.Text;
            App.Settings.UpdatePreference = UpdateBehaviorBox.SelectedIndex switch
            {
                1 => UpdateBehaviorOptions.RemindForUpdates,
                2 => UpdateBehaviorOptions.DontCheckForUpdates,
                _ => UpdateBehaviorOptions.AlwaysAutoUpdate
            };
            App.Settings.Save();
        }

        public void UpdateTextBoxSize()
        {
            ReportBox.Height = Math.Clamp(App.WindowManager.ActualHeight - 220, 100, 9999);
        }

        public void SubmitReport(object sender, RoutedEventArgs args)
        {
            if (!IsReporting)
            {
                if (ReportBox.Text.Length > 20 && ReportBox.Text.Length <= 2500)
                {
                    double time = 61;
                    if (File.Exists(Path.Combine(BackBlazeFolder, "FileDetailsReport.json")))
                    {
                        var oFileInfo = new FileInfo(Path.Combine(BackBlazeFolder, "FileDetailsReport.json"));
                        time = (DateTime.Now - oFileInfo.LastAccessTime).TotalMinutes;
                    }
                    if (time > 60)
                    {
                        WnManager.GetWnManager.SendNotification("Sending Report...");
                        IsReporting = true;
                        if (File.Exists(Path.Combine(BackBlazeFolder, "FileDetailsReport.json")))
                            File.Delete(Path.Combine(BackBlazeFolder, "FileDetailsReport.json"));
                        var UniqueFileName = App.UserSteamId;
                        if (UniqueFileName == null)
                            UniqueFileName = DateTime.UtcNow.Ticks + "report";
                        else
                            UniqueFileName += "report";
                        if (File.Exists(Path.Combine(BackBlazeFolder, UniqueFileName + ".txt")))
                            File.Delete(Path.Combine(BackBlazeFolder, UniqueFileName + ".txt"));
                        File.WriteAllText(Path.Combine(BackBlazeFolder, UniqueFileName + ".txt"), ReportBox.Text);
                        ReportBox.Text = "";
                        var p2Info = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/c \"" + RunBackBlaze + "\" " + UniqueFileName + ".txt " + ReportBucket + " " + BackBlazeApiKey + @" .\report\FileDetailsReport.json",
                            CreateNoWindow = true
                        };
                        Process.Start(p2Info);
                        var thread = new Thread(WaitForReportUpload)
                        {
                            IsBackground = true
                        };
                        thread.Start();
                    }
                    else
                    {
                        WnManager.GetWnManager.SendNotification("Please wait " + (60 - (int)time) + " minutes before sending another report");
                    }
                }
                else
                {
                    if (ReportBox.Text.Length <= 20)
                        WnManager.GetWnManager.SendNotification("Warnging: At least 20 characters are required!");
                    else if (ReportBox.Text.Length > 2500)
                        WnManager.GetWnManager.SendNotification("Warnging: Maximum of 2500 characters is allowed!");
                }
            }
            else
            {
                WnManager.GetWnManager.SendNotification("Warnging: Still processing first report!");
            }
        }

        public void WaitForReportUpload()
        {
            var UploadCompleted = false;
            while (!UploadCompleted)
                try
                {
                    File.ReadAllText(Path.Combine(BackBlazeFolder, "FileDetailsReport.json"));
                    UploadCompleted = true;
                }
                catch
                {
                    UploadCompleted = false;
                    Thread.Sleep(250);
                }
            WnManager.GetWnManager.SendNotification("Your report has been successfully sent!");
            IsReporting = false;
        }

        private void BrowseGameDataPath(object sender, RoutedEventArgs args)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                if (!File.Exists(Path.Combine(dialog.SelectedPath, "Release", "ScrapMechanic.exe")))
                {
                    MessageBox.Show("The selected path doesn't contain the game's executable!", "SmModManager");
                    return;
                }
                GameDataPathBox.Text = dialog.SelectedPath;
            }
        }

        private void BrowseWorkshopPath(object sender, RoutedEventArgs args)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                if (!Directory.Exists(Path.Combine(dialog.SelectedPath, "2122179067")))
                {
                    MessageBox.Show("The selected path doesn't contain the workshop files!", "SmModManager");
                    return;
                }
                WorkshopPathBox.Text = dialog.SelectedPath;
            }
        }

        public void UpdateTabSelection(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0].GetType() == typeof(TabItem))
            {
                var item = (TabItem)args.AddedItems[0];
                item.Foreground = Brushes.Black;
            }
            if (args.RemovedItems.Count > 0 && args.RemovedItems[0].GetType() == typeof(TabItem))
            {
                var item = (TabItem)args.RemovedItems[0];
                item.Foreground = Brushes.White;
            }
        }

    }

}