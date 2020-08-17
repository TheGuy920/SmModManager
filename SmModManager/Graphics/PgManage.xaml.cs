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
            RefreshMods(null, null);
        }

        private void InjectMod(object sender, RoutedEventArgs args)
        {
            // TODO: Inject mod
        }

        private void ArchiveMod(object sender, RoutedEventArgs args)
        {
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            Directory.Move(binding.Path, Path.Combine(Constants.ArchivesPath, new DirectoryInfo(binding.Path).Name));
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

        private void DeleteMod(object sender, RoutedEventArgs args)
        {
            var binding = (ModItemBinding)AllModsList.SelectedItem;
            Directory.Delete(binding.Path, true);
            RefreshMods(null, null);
        }

        public void RefreshMods(object sender, RoutedEventArgs args)
        {
            AllModsList.Items.Clear();
            foreach (var path in Directory.GetDirectories(App.Settings.WorkshopPath))
                if (Utilities.IsMod(path))
                    AllModsList.Items.Add(ModItemBinding.Create(path));
        }

        private void UpdateModSelection(object sender, SelectionChangedEventArgs args)
        {
            DeleteModButton.IsEnabled = AllModsList.SelectedItem != null;
        }

    }

}