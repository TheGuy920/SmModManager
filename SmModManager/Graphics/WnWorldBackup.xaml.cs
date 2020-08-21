using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using SmModManager.Core;
using SmModManager.Core.Models;

namespace SmModManager.Graphics
{

    public partial class WnWorldBackup
    {

        public WnWorldBackup()
        {
            InitializeComponent();
            foreach (var worldPath in Directory.GetFiles(Path.Combine(App.Settings.UserDataPath, "Save", "Survival")))
            {
                var item = new ListBoxItem();
                item.Content = Path.GetFileNameWithoutExtension(worldPath);
                item.Tag = worldPath;
                WorldList.Items.Add(item);
            }
        }

        private void Cancel(object sender, RoutedEventArgs args)
        {
            Close();
        }

        private void Create(object sender, RoutedEventArgs args)
        {
            if (WorldList.SelectedItem == null || string.IsNullOrEmpty(BackupNameBox.Text))
            {
                MessageBox.Show("Make sure you select a world and entered a name before continuing!", "SmModManager");
                return;
            }
            var item = (ListBoxItem)WorldList.SelectedItem;
            new BackupDescriptionModel
            {
                Name = BackupNameBox.Text,
                WorldName = (string)item.Content,
                Data = File.ReadAllBytes((string)item.Tag),
                Time = DateTime.Now
            }.Save(Path.Combine(Constants.WorldBackupsPath, Utilities.GenerateAlphanumeric(16) + ".smmm"));
            DialogResult = true;
            Close();
        }

        private void UpdateWorldSelection(object sender, SelectionChangedEventArgs args)
        {
            BackupNameBox.Text = "Backup of " + ((ListBoxItem)WorldList.SelectedItem).Content;
        }

    }

}