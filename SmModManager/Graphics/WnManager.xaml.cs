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
            PageView.Navigate(App.PageManage);
        }

        private void ShowMultiplayerPage(object sender, RoutedEventArgs args)
        {
            PageView.Navigate(App.PageMultiplayer);
        }

        private void ShowBackupPage(object sender, RoutedEventArgs args)
        {
            PageView.Navigate(App.PageBackups);
        }

        private void ShowArchivePage(object sender, RoutedEventArgs args)
        {
            PageView.Navigate(App.PageArchives);
        }

        private void ShowAdvancedPage(object sender, RoutedEventArgs args)
        {
            PageView.Navigate(App.PageAdvanced);
        }

        private void Exit(object sender, RoutedEventArgs args)
        {
            Application.Current.Shutdown();
        }

    }

}