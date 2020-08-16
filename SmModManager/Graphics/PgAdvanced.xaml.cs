using System.Windows;
using Ookii.Dialogs.Wpf;
using SmModManager.Core;

namespace SmModManager.Graphics
{

    public partial class PgAdvanced
    {

        public PgAdvanced()
        {
            InitializeComponent();
            GameDataPathBox.Text = App.Settings.GameDataPath;
            UserDataPathBox.Text = App.Settings.UserDataPath;
        }

        private void ResetSettings(object sender, RoutedEventArgs args)
        {
            App.Settings.Reset();
            Utilities.RestartApp();
        }
        
        private void SaveSettings(object sender, RoutedEventArgs args)
        {
            App.Settings.GameDataPath = GameDataPathBox.Text;
            App.Settings.UserDataPath = UserDataPathBox.Text;
            App.Settings.Save();
        }

        private void BrowseGameDataPath(object sender, RoutedEventArgs args)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
                GameDataPathBox.Text = dialog.SelectedPath;
        }

    }

}