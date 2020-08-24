using System.Diagnostics;
using System.IO;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using SmModManager.Core;
using SmModManager.Core.Bindings;
using System.Linq;
using System.Windows.Media;
using System.Drawing;

namespace SmModManager.Graphics
{

    public partial class PgManage
    {
        public static PgManage GetPgManage;
        public PgManage()
        {
            GetPgManage = this;
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
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            CompatibleModsListPreview.Source = binding.Preview;
            CompatibleModsListName.Text = binding.Name;
            CompatibleModsListDescription.Text = binding.Description;
        }

        public void UpdatePreviewImages()
        {
            var binding = (ModItemBinding)AvailableModsList.SelectedItem;
            double ratio;
            if (App.WindowManager.ActualWidth < App.WindowManager.ActualHeight + 550)
            {
                ratio = Math.Clamp(Math.Clamp(App.WindowManager.ActualWidth + 100, 500, 1650) - 1000, 100, 1650);
            }
            else
            {
                ratio = Math.Clamp(Math.Clamp(App.WindowManager.ActualHeight + 500, 500, 1200) - 700, 100, 1200);
            }
            if (binding != null)
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(binding.ImagePath);
                var width = (double)img.Width - img.Width / 7;
                var height = (double)img.Height;
                AvailableModsListScroll.Height = App.WindowManager.ActualHeight;
                AvailableModsListDescription.Height = Count(AvailableModsListDescription.Text, '\n') * 25 + 50;
                AvailableModsListDescription.Width = Math.Clamp(App.WindowManager.ActualWidth - 700, 10, 700);
                AvailableModsListPreview.Height = Math.Clamp((height / width) * ratio, 10, 400);
                AvailableModsListPreview.Width = Math.Clamp((width / height) * ratio, 10, 600);
            }
            binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            if (binding != null)
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(binding.ImagePath);
                var width = (double)img.Width - img.Width / 7;
                var height = (double)img.Height;
                CompatibleModsListScroll.Height = App.WindowManager.ActualHeight;
                CompatibleModsListDescription.Height = Count(CompatibleModsListDescription.Text, '\n') * 25 + 50;
                CompatibleModsListDescription.Width = Math.Clamp(App.WindowManager.ActualWidth - 700, 10, 700);
                CompatibleModsListPreview.Height = Math.Clamp((height / width) * ratio, 10, 400);
                CompatibleModsListPreview.Width = Math.Clamp((width / height) * ratio, 10, 600);
            }
            if (CompatibleModsTab.IsSelected)
            {
                CompatibleModsTab.Foreground = new SolidColorBrush(Colors.Black);
            }
            else 
            {
                CompatibleModsTab.Foreground = new SolidColorBrush(Colors.White);
            }
            if (AvailableModsTab.IsSelected)
            {
                AvailableModsTab.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                AvailableModsTab.Foreground = new SolidColorBrush(Colors.White);
            }
            if (CompatibleModsList.SelectedItem == null)
            {
                CompatibleModsList.SelectedItem = CompatibleModsList.Items[0];
            }
            if (AvailableModsList.SelectedItem == null)
            {
                AvailableModsList.SelectedItem = AvailableModsList.Items[0];
            }
        }

        public int Count(string str, char Char)
        {
            int count = 0;
            foreach (char NewChar in str)
            {
                if (NewChar == Char)
                {
                    count++;
                }
            }

            return count;
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
            var binding = (ModItemBinding)AvailableModsList.SelectedItem;
            AvailableModsListPreview.Source = binding.Preview;
            AvailableModsListName.Text = binding.Name;
            AvailableModsListDescription.Text = binding.Description;
        }

        private void AvailableModsList_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }
    }

}