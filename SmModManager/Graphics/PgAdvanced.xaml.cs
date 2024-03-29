﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CefSharp;
using Ookii.Dialogs.Wpf;
using SmModManager.Core;
using SmModManager.Core.Enums;
using SmModManager.Core.Handlers;

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
            AppLanguageBox.SelectedIndex = App.Settings.AppLanguage switch
            {
                _ => 0
            };
            Tutorial.MenuHandler = new MenuHandler();
            switch (App.Settings.StartupMode)
            {
                case WindowState.Minimized:
                    StartUpMode.SelectedIndex = 0;
                    break;
                case WindowState.Normal:
                    StartUpMode.SelectedIndex = 1;
                    break;
                case WindowState.Maximized:
                    StartUpMode.SelectedIndex = 2;
                    break;
            }
            XWidth.Text = App.Settings.StartUpX.ToString();
            YHeight.Text = App.Settings.StartUpY.ToString();
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
            App.Settings.AppLanguage = AppLanguageBox.SelectedIndex switch
            {
                _ => LanguageOptions.English
            };
            App.Settings.Save();
        }

        public void UpdateTextBoxSize()
        {
            ReportBox.Height = Math.Clamp(App.WindowManager.ActualHeight - 220, 100, 9999);
        }

        private void SubmitReport(object sender, RoutedEventArgs args)
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
                        WnManager.GetWnManager.SendNotification((string)System.Windows.Application.Current.FindResource("sendingrep"));
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
                        WnManager.GetWnManager.SendNotification((string)System.Windows.Application.Current.FindResource("pleasewait") + " " + (60 - (int)time) + " " + (string)System.Windows.Application.Current.FindResource("waitxtime"));
                    }
                }
                else
                {
                    if (ReportBox.Text.Length <= 20)
                        WnManager.GetWnManager.SendNotification((string)System.Windows.Application.Current.FindResource("20charreq"));
                    else if (ReportBox.Text.Length > 2500)
                        WnManager.GetWnManager.SendNotification((string)System.Windows.Application.Current.FindResource("2500charlimit"));
                }
            }
            else
            {
                WnManager.GetWnManager.SendNotification((string)System.Windows.Application.Current.FindResource("procfirstrep"));
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
            WnManager.GetWnManager.SendNotification((string)System.Windows.Application.Current.FindResource("repsent"));
            IsReporting = false;
        }

        private void BrowseGameDataPath(object sender, RoutedEventArgs args)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                if (!File.Exists(Path.Combine(dialog.SelectedPath, "Release", "ScrapMechanic.exe")))
                {
                    MessageBox.Show((string)System.Windows.Application.Current.FindResource("selectedpathmissinggamefiles"), "SmModManager");
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
                    MessageBox.Show((string)System.Windows.Application.Current.FindResource("selectedpathmissingworkshopfiles"), "SmModManager");
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
        public void MoveForward(object sender, RoutedEventArgs args)
        {
            if (Tutorial.CanGoForward)
                Tutorial.Forward();
        }

        public void MoveBackward(object sender, RoutedEventArgs args)
        {
            if (Tutorial.CanGoBack)
                Tutorial.Back();
        }

        public void GoHome(object sender, RoutedEventArgs args)
        {
            Tutorial.Address = "https://smmodmanager.com/tutorial/";
        }

        public void UpdateUrl(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (CurrentUrl.Text != Tutorial.Address)
                CurrentUrl.Text = Tutorial.Address;
        }

        private void StartUp_TextChanged(object sender, TextChangedEventArgs e)
        {
            var number = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            if (XWidth.Text.Length > 0)
                if (!number.Contains(XWidth.Text[^1]) || XWidth.Text.Length > 4)
                {
                    XWidth.Text = XWidth.Text[0..(XWidth.Text.Length - 1)];
                    XWidth.CaretIndex = Math.Clamp(XWidth.Text.Length, 0, 4);
                }
                else
                    App.Settings.StartUpX = int.Parse(XWidth.Text);
            if (YHeight.Text.Length > 0)
                if (!number.Contains(YHeight.Text[^1]) || YHeight.Text.Length > 4)
                {
                    YHeight.Text = YHeight.Text[0..(YHeight.Text.Length - 1)];
                    YHeight.CaretIndex = Math.Clamp(YHeight.Text.Length, 0, 4);
                }
                else
                    App.Settings.StartUpY = int.Parse(YHeight.Text);
        }

        private void StartUpMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (StartUpMode.SelectedIndex)
            {
                case 0:
                    App.Settings.StartupMode = WindowState.Minimized;
                    break;
                case 1:
                    App.Settings.StartupMode = WindowState.Normal;
                    break;
                case 2:
                    App.Settings.StartupMode = WindowState.Maximized;
                    break;
            }
            App.Settings.Save();
        }
    }

}