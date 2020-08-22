using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmModManager.Core;
using SmModManager.Core.Bindings;

namespace SmModManager.Graphics
{

    public partial class PgManage
    {

        public PgManage()
        {
            InitializeComponent();
            RefreshCompatibleMods(null, null);
            RefreshAvailableMods(null, null);
        }

        private void InjectMod(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show("Injecting this mod might conflict with the already injected mods, it is recommended to backup before injecting. Do you want to continue?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            Utilities.CopyDirectory(Path.Combine(binding.Path, "Survival"), Path.Combine(App.Settings.GameDataPath, "Survival"));
        }

        private void ArchiveCompatibleMod(object sender, RoutedEventArgs args)
        {
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            Utilities.CopyDirectory(binding.Path, Path.Combine(Constants.ArchivesPath, Utilities.GetDirectoryName(binding.Path)));
            App.PageArchives.RefreshMods(null, null);
        }

        private void DeleteCompatibleMod(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show("Are you sure that you want to delete this mod?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            Directory.Delete(binding.Path, true);
            RefreshCompatibleMods(null, null);
        }

        public void RefreshCompatibleMods(object sender, RoutedEventArgs args)
        {
            CompatibleModsList.Items.Clear();
            foreach (var path in Directory.GetDirectories(App.Settings.WorkshopPath))
                if (Utilities.IsCompatibleMod(path))
                    CompatibleModsList.Items.Add(ModItemBinding.Create(path));
            foreach (var path in Directory.GetDirectories(Path.Combine(App.Settings.UserDataPath, "Mods")))
                if (Utilities.IsCompatibleMod(path))
                    CompatibleModsList.Items.Add(ModItemBinding.Create(path));
        }

        private void OpenCompatibleMod(object sender, MouseButtonEventArgs args)
        {
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            if (!string.IsNullOrEmpty(binding.Url))
                Utilities.OpenBrowserUrl(binding.Url);
            else
                Process.Start(binding.Path);
        }

        private void UpdateCompatibleSelection(object sender, SelectionChangedEventArgs args)
        {
            if (CompatibleModsList.SelectedItem == null)
            {
                InjectButton.IsEnabled = false;
                ArchiveCompatibleButton.IsEnabled = false;
                DeleteCompatibleButton.IsEnabled = false;
            }
            else
            {
                InjectButton.IsEnabled = true;
                ArchiveCompatibleButton.IsEnabled = true;
                DeleteCompatibleButton.IsEnabled = true;
            }
        }

        private void ArchiveAvailableMod(object sender, RoutedEventArgs args)
        {
            var binding = (ModItemBinding)AvailableModsList.SelectedItem;
            Utilities.CopyDirectory(binding.Path, Path.Combine(Constants.ArchivesPath, Utilities.GetDirectoryName(binding.Path)));
            App.PageArchives.RefreshMods(null, null);
        }

        private void DeleteAvailableMod(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show("Are you sure that you want to delete this mod?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (ModItemBinding)AvailableModsList.SelectedItem;
            Directory.Delete(binding.Path, true);
            RefreshAvailableMods(null, null);
        }

        public void RefreshAvailableMods(object sender, RoutedEventArgs args)
        {
            AvailableModsList.Items.Clear();
            foreach (var path in Directory.GetDirectories(App.Settings.WorkshopPath))
                if (Utilities.IsMod(path))
                    AvailableModsList.Items.Add(ModItemBinding.Create(path));
            foreach (var path in Directory.GetDirectories(Path.Combine(App.Settings.UserDataPath, "Mods")))
                if (Utilities.IsMod(path))
                    AvailableModsList.Items.Add(ModItemBinding.Create(path));
        }

        private void OpenAvailableMod(object sender, MouseButtonEventArgs args)
        {
            var binding = (ModItemBinding)AvailableModsList.SelectedItem;
            if (!string.IsNullOrEmpty(binding.Url))
                Utilities.OpenBrowserUrl(binding.Url);
            else
                Utilities.OpenExplorerUrl(binding.Path);
        }

        private void UpdateAvailableModSelection(object sender, SelectionChangedEventArgs args)
        {
            if (AvailableModsList.SelectedItem == null)
            {
                ArchiveAvailableButton.IsEnabled = false;
                DeleteAvailableButton.IsEnabled = false;
            }
            else
            {
                ArchiveAvailableButton.IsEnabled = true;
                DeleteAvailableButton.IsEnabled = true;
            }
        }

    }

}