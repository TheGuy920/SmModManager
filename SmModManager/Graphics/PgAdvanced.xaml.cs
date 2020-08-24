using System.IO;
using System.Reflection;
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