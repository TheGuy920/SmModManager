﻿using System.IO;
using System.Windows;
using Ookii.Dialogs.Wpf;
using SmModManager.Core;
using SmModManager.Core.Enums;

namespace SmModManager.Graphics
{

    public partial class PgAdvanced
    {

        public PgAdvanced()
        {
            InitializeComponent();
            ChangelogText.Text = Utilities.RetrieveResourceData("SmModManager.Resources.Documents.Changelog.txt");
            CreditsText.Text = Utilities.RetrieveResourceData("SmModManager.Resources.Documents.Credits.txt");
            foreach (var userDataPath in Directory.GetDirectories(Constants.UsersDataPath))
                UserDataPathBox.Items.Add(userDataPath);
            GameDataPathBox.Text = App.Settings.GameDataPath;
            WorkshopPathBox.Text = App.Settings.WorkshopPath;
            UserDataPathBox.Text = App.Settings.UserDataPath;
            UpdatePreferenceBox.SelectedIndex = App.Settings.UpdatePreference switch
            {
                UpdatePreferenceOptions.RemindForUpdates => 1,
                UpdatePreferenceOptions.DontCheckForUpdates => 2,
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
            App.Settings.UpdatePreference = UpdatePreferenceBox.SelectedIndex switch
            {
                1 => UpdatePreferenceOptions.RemindForUpdates,
                2 => UpdatePreferenceOptions.DontCheckForUpdates,
                _ => UpdatePreferenceOptions.AlwaysAutoUpdate
            };
            App.Settings.Save();
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

    }

}