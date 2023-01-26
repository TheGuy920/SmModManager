using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using SmModManager.Core;
using SmModManager.Core.Bindings;
using SmModManager.Core.Enums;
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

        private void ImportWorld(object sender, RoutedEventArgs args)
        {
            var dialog = new OpenFileDialog { Filter = "Scrap Mechanic Mod Manager|*.smmm", Multiselect = true };
            if (dialog.ShowDialog() == false)
                return;
            foreach (var path in dialog.FileNames)
            {
                var description = BackupDescriptionModel.Load(path);
                if (description.Type != BackupType.World)
                    continue;
                File.Copy(path, Path.Combine(Constants.WorldBackupsPath, Utilities.GenerateAlphanumeric(16) + ".smmm"));
            }
            RefreshWorlds(null, null);
        }

        private void ExportWorld(object sender, RoutedEventArgs args)
        {
            var dialog = new SaveFileDialog { Filter = "Scrap Mechanic Mod Manager|*.smmm" };
            if (dialog.ShowDialog() == true)
                File.Copy(((BackupItemBinding)WorldsList.SelectedItem).Path, dialog.FileName);
        }

        private void RestoreWorld(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show((string)System.Windows.Application.Current.FindResource("confirmoverwriteworld"), "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (BackupItemBinding)WorldsList.SelectedItem;
            File.WriteAllBytes(Path.Combine(App.Settings.UserDataPath, "Save", "Survival", binding.WorldName + ".db"), BackupDescriptionModel.Load(binding.Path).Data);
        }

        private void DeleteWorld(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show((string)System.Windows.Application.Current.FindResource("confirmdeletebackup"), "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (BackupItemBinding)WorldsList.SelectedItem;
            File.Delete(binding.Path);
            RefreshWorlds(null, null);
        }

        private void RefreshWorlds(object sender, RoutedEventArgs args)
        {
            WorldsList.Items.Clear();
            foreach (var path in Directory.GetFiles(Constants.WorldBackupsPath))
                try
                {
                    if (string.IsNullOrEmpty(path))
                        continue;
                    WorldsList.Items.Add(BackupItemBinding.Create(path));
                }
                catch
                {
                    // nothing
                }
        }

        private void UpdateWorldSelection(object sender, SelectionChangedEventArgs args)
        {
            if (WorldsList.SelectedItem == null)
            {
                ExportWorldButton.IsEnabled = false;
                RestoreWorldButton.IsEnabled = false;
                DeleteWorldButton.IsEnabled = false;
            }
            else
            {
                ExportWorldButton.IsEnabled = true;
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

        private void ImportGame(object sender, RoutedEventArgs args)
        {
            var dialog = new OpenFileDialog { Filter = "Scrap Mechanic Mod Manager|*.smmm", Multiselect = true };
            if (dialog.ShowDialog() == false)
                return;
            foreach (var path in dialog.FileNames)
            {
                var description = BackupDescriptionModel.Load(path);
                if (description.Type != BackupType.Game)
                    continue;
                File.Copy(path, Path.Combine(Constants.GameBackupsPath, Utilities.GenerateAlphanumeric(16) + ".smmm"));
            }
            RefreshGames(null, null);
        }

        private void ExportGame(object sender, RoutedEventArgs args)
        {
            var dialog = new SaveFileDialog { Filter = "Scrap Mechanic Mod Manager|*.smmm" };
            if (dialog.ShowDialog() != true)
                return;
            if (File.Exists(dialog.FileName))
                if (MessageBox.Show((string)System.Windows.Application.Current.FindResource("confirmoverwrite"), "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
            File.Copy(((BackupItemBinding)GamesList.SelectedItem).Path, dialog.FileName, true);
        }

        private void RestoreGame(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show((string)System.Windows.Application.Current.FindResource("confirmbreakparts"), "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (BackupItemBinding)GamesList.SelectedItem;
            var temporaryPath = Path.Combine(Constants.GameBackupsPath, Path.GetFileNameWithoutExtension(binding.Path)!);
            File.WriteAllBytes(temporaryPath + ".tmp", BackupDescriptionModel.Load(binding.Path).Data);
            Directory.CreateDirectory(temporaryPath);
            ZipFile.ExtractToDirectory(temporaryPath + ".tmp", temporaryPath, true);
            Utilities.CopyDirectory(temporaryPath, Path.Combine(App.Settings.GameDataPath, "Survival"));
        }

        private void DeleteGame(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show((string)System.Windows.Application.Current.FindResource("confirmdeletebackup"), "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            try
            {
                var binding = (BackupItemBinding)GamesList.SelectedItem;
                File.Delete(binding.Path);
            }
            catch (Exception error)
            {
                MessageBox.Show((string)System.Windows.Application.Current.FindResource("unabletodelete") + $" {error.Message}", "SmModManager", MessageBoxButton.OK);
            }
            RefreshGames(null, null);
        }

        private void RefreshGames(object sender, RoutedEventArgs args)
        {
            GamesList.Items.Clear();
            foreach (var path in Directory.GetFiles(Constants.GameBackupsPath))
                try
                {
                    if (string.IsNullOrEmpty(path))
                        continue;
                    GamesList.Items.Add(BackupItemBinding.Create(path));
                }
                catch
                {
                    // nothing
                }
        }

        private void UpdateGameSelection(object sender, SelectionChangedEventArgs args)
        {
            if (GamesList.SelectedItem == null)
            {
                ExportGameButton.IsEnabled = false;
                RestoreGameButton.IsEnabled = false;
                DeleteGameButton.IsEnabled = false;
            }
            else
            {
                ExportGameButton.IsEnabled = true;
                RestoreGameButton.IsEnabled = true;
                DeleteGameButton.IsEnabled = true;
            }
        }

        private void UpdateTabSelection(object sender, SelectionChangedEventArgs args)
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] == OpenFileExporer && OpenFileExporer.IsSelected)
            {
                var item = (TabItem)args.AddedItems[0];
                item.IsSelected = true;
                item = (TabItem)args.RemovedItems[0];
                item.IsSelected = true;
                var thread = new Thread(OpenOldBackups) { IsBackground = true };
                if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "backups")))
                    thread.Start();
                else
                    thread.Start();
            }
        }

        private void OpenOldBackups()
        {
            Utilities.OpenExplorerUrl(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups"));
        }

    }

}