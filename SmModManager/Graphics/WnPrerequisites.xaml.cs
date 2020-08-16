using System.IO;
using System.Windows;
using Ookii.Dialogs.Wpf;
using SmModManager.Core;

namespace SmModManager.Graphics
{

    public partial class WnPrerequisites
    {

        public WnPrerequisites()
        {
            InitializeComponent();
            foreach (var userDataPath in Directory.GetDirectories(Constants.UsersDataPath))
                UserDataPathBox.Items.Add(userDataPath);
            UserDataPathBox.SelectedIndex = 0;
        }

        private void Cancel(object sender, RoutedEventArgs args)
        {
            Application.Current.Shutdown();
        }

        private void Continue(object sender, RoutedEventArgs args)
        {
            if (string.IsNullOrEmpty(GameDataPathBox.Text) || string.IsNullOrEmpty(UserDataPathBox.Text))
            {
                MessageBox.Show("Make sure all the inputs are filled!", "SmModManager");
                return;
            }
            App.Settings.GameDataPath = GameDataPathBox.Text;
            App.Settings.WorkshopPath = WorkshopPathBox.Text;
            App.Settings.UserDataPath = UserDataPathBox.Text;
            App.Settings.Save();
            DialogResult = true;
            Close();
        }

        private void BrowseGameDataPath(object sender, RoutedEventArgs args)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                if (!File.Exists(Path.Combine(dialog.SelectedPath, "Release", "ScrapMechanic.exe")))
                {
                    MessageBox.Show("The selected path doesn't contain the game's executable!", "SmModManager");
                    return;
                }
                GameDataPathBox.Text = dialog.SelectedPath;
            }
        }

        private void BrowseWorkshopPath(object sender, RoutedEventArgs args)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                if (!Directory.Exists(Path.Combine(dialog.SelectedPath, "2122179067")))
                {
                    MessageBox.Show("The selected path doesn't contain the workshop files!", "SmModManager");
                    return;
                }
                WorkshopPathBox.Text = dialog.SelectedPath;
            }
        }

    }

}