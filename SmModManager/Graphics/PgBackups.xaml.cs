using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmModManager.Core;
using SmModManager.Core.Bindings;

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
            // TODO: Create world backup
        }

        private void RestoreWorld(object sender, RoutedEventArgs args)
        {
            // TODO: Restore world backup
        }

        private void DeleteWorld(object sender, RoutedEventArgs args)
        {
            var binding = (BackupItemBinding)GamesList.SelectedItem;
            Directory.Delete(binding.Path, true);
            RefreshWorlds(null, null);
        }

        private void RefreshWorlds(object sender, RoutedEventArgs args)
        {
            WorldsList.Items.Clear();
            foreach (var path in Directory.GetDirectories(Constants.WorldBackupsPath))
                if (Utilities.IsBackup(path))
                    WorldsList.Items.Add(BackupItemBinding.Create(path));
        }

        private void OpenWorldFolder(object sender, MouseButtonEventArgs args)
        {
            var binding = (BackupItemBinding)WorldsList.SelectedItem;
            Utilities.OpenExplorerUrl(binding.Path);
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
            // TODO: Create game backup
        }

        private void RestoreGame(object sender, RoutedEventArgs args)
        {
            // TODO: Restore game backup
        }

        private void DeleteGame(object sender, RoutedEventArgs args)
        {
            var binding = (BackupItemBinding)GamesList.SelectedItem;
            Directory.Delete(binding.Path, true);
            RefreshGames(null, null);
        }

        private void RefreshGames(object sender, RoutedEventArgs args)
        {
            GamesList.Items.Clear();
            foreach (var path in Directory.GetDirectories(Constants.GameBackupsPath))
                if (Utilities.IsBackup(path))
                    GamesList.Items.Add(BackupItemBinding.Create(path));
        }

        private void OpenGameFolder(object sender, MouseButtonEventArgs args)
        {
            var binding = (BackupItemBinding)GamesList.SelectedItem;
            Utilities.OpenExplorerUrl(binding.Path);
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