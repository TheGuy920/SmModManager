using System.Windows;

namespace SmModManager.Graphics
{

    public partial class WnManager
    {

        public WnManager()
        {
            InitializeComponent();
        }

        private void ShowManagePage(object sender, RoutedEventArgs args)
        {
            View.Navigate(App.PageManage);
        }

        private void ShowMultiplayerPage(object sender, RoutedEventArgs args)
        {
            View.Navigate(App.PageMultiplayer);
        }

        private void ShowBackupPage(object sender, RoutedEventArgs args)
        {
            View.Navigate(App.PageBackup);
        }

        private void ShowArchivePage(object sender, RoutedEventArgs args)
        {
            View.Navigate(App.PageArchive);
        }

        private void ShowAdvancedPage(object sender, RoutedEventArgs args)
        {
            View.Navigate(App.PageAdvanced);
        }

        private void Exit(object sender, RoutedEventArgs args)
        {
            Application.Current.Shutdown();
        }

        private void CheckPrerequisites(object sender, RoutedEventArgs args)
        {
            // TODO
        }

    }

}