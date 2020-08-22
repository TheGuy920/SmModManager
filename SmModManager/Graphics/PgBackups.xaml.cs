using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using SmModManager.Core;
using SmModManager.Core.Bindings;
using SmModManager.Core.Models;

namespace SmModManager.Graphics
{

    public partial class PgBackups
    {

        public PgBackups()
        {
            InitializeComponent();
            RefreshWorlds(null, null);
            RefreshGames(null, null);
        }

        private void CreateWorldBackup(object sender, RoutedEventArgs args)
        {
            var dialog = new WnWorldBackup { Owner = App.WindowManager };
            if (dialog.ShowDialog() == true)
                RefreshWorlds(null, null);
        }

        private void RestoreWorld(object sender, RoutedEventArgs args)
        {
            var binding = (BackupItemBinding)WorldsList.SelectedItem;
            File.WriteAllBytes(Path.Combine(App.Settings.UserDataPath, "Save", "Survival", binding.WorldName + ".db"), BackupDescriptionModel.Load(binding.Path).Data);
        }

        private void DeleteWorld(object sender, RoutedEventArgs args)
        {
            var binding = (BackupItemBinding)WorldsList.SelectedItem;
            File.Delete(binding.Path);
            RefreshWorlds(null, null);
        }

        private void RefreshWorlds(object sender, RoutedEventArgs args)
        {
            WorldsList.Items.Clear();
            foreach (var path in Directory.GetFiles(Constants.WorldBackupsPath))
                WorldsList.Items.Add(BackupItemBinding.Create(path));
        }

        private void UpdateWorldSelection(object sender, SelectionChangedEventArgs args)
        {
            if (WorldsList.SelectedItem == null)
            {
                RestoreWorldButton.IsEnabled = false;
                DeleteWorldButton.IsEnabled = false;
            }
            else
            {
                RestoreWorldButton.IsEnabled = true;
                DeleteWorldButton.IsEnabled = true;
            }
        }

        private void CreateGameBackup(object sender, RoutedEventArgs args)
        {
            var dialog = new WnGameBackup { Owner = App.WindowManager };
            if (dialog.ShowDialog() == true)
                RefreshGames(null, null);
        }

        private void RestoreGame(object sender, RoutedEventArgs args)
        {
            var binding = (BackupItemBinding)GamesList.SelectedItem;
            var temporaryPath = Path.Combine(Constants.GameBackupsPath, Path.GetFileNameWithoutExtension(binding.Path)!);
            File.WriteAllBytes(temporaryPath + ".tmp", BackupDescriptionModel.Load(binding.Path).Data);
            Directory.CreateDirectory(temporaryPath);
            ZipFile.ExtractToDirectory(temporaryPath + ".tmp", temporaryPath);
            Utilities.CopyDirectory(temporaryPath, Path.Combine(App.Settings.GameDataPath, "Survival"));
        }

        private void DeleteGame(object sender, RoutedEventArgs args)
        {
            var binding = (BackupItemBinding)GamesList.SelectedItem;
            File.Delete(binding.Path);
            RefreshGames(null, null);
        }

        private void RefreshGames(object sender, RoutedEventArgs args)
        {
            GamesList.Items.Clear();
            foreach (var path in Directory.GetFiles(Constants.GameBackupsPath))
                GamesList.Items.Add(BackupItemBinding.Create(path));
        }

        private void UpdateGameSelection(object sender, SelectionChangedEventArgs args)
        {
            if (GamesList.SelectedItem == null)
            {
                RestoreGameButton.IsEnabled = false;
                DeleteGameButton.IsEnabled = false;
            }
            else
            {
                RestoreGameButton.IsEnabled = true;
                DeleteGameButton.IsEnabled = true;
            }
        }

    }

}