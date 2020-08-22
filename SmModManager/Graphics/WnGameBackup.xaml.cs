using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using SmModManager.Core;
using SmModManager.Core.Models;

namespace SmModManager.Graphics
{

    public partial class WnGameBackup
    {

        public WnGameBackup()
        {
            InitializeComponent();
        }

        private void Cancel(object sender, RoutedEventArgs args)
        {
            Close();
        }

        private void Create(object sender, RoutedEventArgs args)
        {
            var temporaryPath = Path.Combine(Constants.GameBackupsPath, Utilities.GenerateAlphanumeric(16));
            Directory.CreateDirectory(temporaryPath);
            if (CraftingRecipesBox.IsChecked == true)
            {
                const string targetPath = "Survival\\CraftingRecipes";
                Utilities.CopyDirectory(Path.Combine(App.Settings.GameDataPath, targetPath), Path.Combine(temporaryPath, targetPath));
            }
            if (GraphicalUserInterfaceBox.IsChecked == true)
            {
                const string targetPath = "Survival\\Gui";
                Utilities.CopyDirectory(Path.Combine(App.Settings.GameDataPath, targetPath), Path.Combine(temporaryPath, targetPath));
            }
            if (GameScriptsBox.IsChecked == true)
            {
                const string targetPath = "Survival\\Scripts";
                Utilities.CopyDirectory(Path.Combine(App.Settings.GameDataPath, targetPath), Path.Combine(temporaryPath, targetPath));
            }
            ZipFile.CreateFromDirectory(temporaryPath, temporaryPath + ".tmp");
            new BackupDescriptionModel
            {
                Name = BackupNameBox.Text,
                Data = File.ReadAllBytes(temporaryPath + ".tmp"),
                Time = DateTime.Now
            }.Save(temporaryPath + ".smmm");
            Directory.Delete(temporaryPath, true);
            File.Delete(temporaryPath + ".tmp");
            DialogResult = true;
            Close();
        }

    }

}