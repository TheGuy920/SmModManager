using System.IO;
using System.Windows;
using System.Windows.Controls;
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
            // TODO: Inject mod
        }

        private void ArchiveMod(object sender, RoutedEventArgs args)
        {
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            Utilities.CopyDirectory(binding.Path, Path.Combine(Constants.ArchivesPath, new DirectoryInfo(binding.Path).Name));
            App.PageArchives.RefreshMods(null, null);
        }

        private void DeleteCompatibleMod(object sender, RoutedEventArgs args)
        {
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
        }

        private void UpdateCompatibleSelection(object sender, SelectionChangedEventArgs args)
        {
            if (CompatibleModsList.SelectedItem == null)
            {
                InjectButton.IsEnabled = false;
                ArchiveButton.IsEnabled = false;
                DeleteCompatibleButton.IsEnabled = false;
            }
            else
            {
                InjectButton.IsEnabled = true;
                ArchiveButton.IsEnabled = true;
                DeleteCompatibleButton.IsEnabled = true;
            }
        }

        private void DeleteAvailableMod(object sender, RoutedEventArgs args)
        {
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
        }

        private void UpdateAvailableModSelection(object sender, SelectionChangedEventArgs args)
        {
            DeleteModButton.IsEnabled = AvailableModsList.SelectedItem != null;
        }

    }

}