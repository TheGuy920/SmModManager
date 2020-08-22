﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using SmModManager.Core;
using SmModManager.Core.Bindings;

namespace SmModManager.Graphics
{

    public partial class PgArchives
    {

        public PgArchives()
        {
            InitializeComponent();
            RefreshMods(null, null);
        }

        private void UnarchiveMod(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show("Are you sure that you want to unarchive this mod?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (ModItemBinding)ArchivedModsList.SelectedItem;
            Directory.Move(binding.Path, Path.Combine(App.Settings.UserDataPath, "Mods", Utilities.GetDirectoryName(binding.Path)));
            RefreshMods(null, null);
            App.PageManage.RefreshCompatibleMods(null, null);
            App.PageManage.RefreshAvailableMods(null, null);
        }

        private void DeleteMod(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show("Are you sure that you want to delete this archive?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (ModItemBinding)ArchivedModsList.SelectedItem;
            Directory.Delete(binding.Path, true);
            RefreshMods(null, null);
        }

        public void RefreshMods(object sender, RoutedEventArgs args)
        {
            ArchivedModsList.Items.Clear();
            foreach (var path in Directory.GetDirectories(Constants.ArchivesPath))
                if (Utilities.IsMod(path))
                    ArchivedModsList.Items.Add(ModItemBinding.Create(path));
        }

        private void UpdateSelection(object sender, SelectionChangedEventArgs args)
        {
            if (ArchivedModsList.SelectedItem == null)
            {
                UnarchiveButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
            }
            else
            {
                UnarchiveButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
            }
        }

    }

}