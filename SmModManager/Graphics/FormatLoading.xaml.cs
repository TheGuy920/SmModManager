﻿using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using SmModManager.Core;

namespace SmModManager.Graphics
{

    /// <summary>
    ///     Interaction logic for FormatLoading.xaml
    /// </summary>
    public partial class FormatLoading : Page
    {

        public int CurrentFileIndex;
        public string message = "Oh No! Previous versions of the mod loader, configured your mod files to better increase the chances of locating compatible mods\n\nUnfortunatally the older version of this mod loader configured the mod files poorly, and in order to receive the latest formatting update, you are going to have to delete all of your mods, and reinstall them...\n\nim so sorry :(";

        public FormatLoading()
        {
            InitializeComponent();
        }

        public void UpdateStatus()
        {
            if (App.RevalidateMods)
            {
                FormattingStatus.Visibility = Visibility.Hidden;
                DeleteAndValidate.Visibility = Visibility.Visible;
                Status.Visibility = Visibility.Hidden;
                RichTextBox.Document = Utilities.ParseToFlowDocument(message);
            }
            else
            {
                FormattingStatus.Visibility = Visibility.Visible;
                DeleteAndValidate.Visibility = Visibility.Hidden;
                Status.Visibility = Visibility.Hidden;
            }
        }

        public void DeleteAllAndValidate(object sender, RoutedEventArgs args)
        {
            Status.Visibility = Visibility.Visible;
            DeleteAndValidate.Visibility = Visibility.Hidden;
            new Thread(ThreadDeleteThenValidate).Start();
        }

        public void ThreadDeleteThenValidate()
        {
            var totalNumOfFiles = Directory.GetDirectories(App.Settings.WorkshopPath).Length;
            foreach (var SubDirectory in Directory.GetDirectories(App.Settings.WorkshopPath))
            {
                CurrentFileIndex++;
                Dispatcher.Invoke(() =>
                {
                    Progress.Value = CurrentFileIndex / totalNumOfFiles;
                    TextBox.Text = SubDirectory;
                });
                Directory.Delete(SubDirectory, true);
            }
            Dispatcher.Invoke(() =>
            {
                Progress.Value = 100;
                TextBox.Text = "Completed, Validating files now...";
            });
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Utilities.GetSteamLocation(), "steam.exe"),
                Arguments = @"steam://validate/387990/"
            };
            Process.Start(startInfo);
        }

        public void Exit(object sender, RoutedEventArgs args)
        {
            App.Settings.DontAskMeToReValidate = (bool)RemindMe.IsChecked;
            App.Settings.Save();
            WnManager.GetWnManager.CanNavigate = true;
            WnManager.GetWnManager.ShowHomePage(null, null);
        }

        public void Restart(object sender, RoutedEventArgs args)
        {
            Utilities.RestartApp();
        }

    }

}