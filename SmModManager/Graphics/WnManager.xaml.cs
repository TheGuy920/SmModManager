using System.Windows;

namespace SmModManager.Graphics
{

    public partial class WnManager
    {
        public static WnManager GetWnManager;
        public WnManager()
        {
            InitializeComponent();
            GetWnManager = this;
        }

        public void UpdateTopMenu()
        {
            TopMenuPanel.Width = App.WindowManager.ActualWidth;
            PageView.Width = App.WindowManager.ActualWidth - 15;
        }
        private void ClearButtonFontWeight()
        {
            ManageButton.FontWeight = FontWeights.Normal;
            MultiplayerButton.FontWeight = FontWeights.Normal;
            BackupsButton.FontWeight = FontWeights.Normal;
            ArchivesButton.FontWeight = FontWeights.Normal;
            AdvancedButton.FontWeight = FontWeights.Normal;
        }

        private void ShowManagePage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageManage);
            ManageButton.FontWeight = FontWeights.Bold;
        }

        private void ShowMultiplayerPage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageMultiplayer);
            MultiplayerButton.FontWeight = FontWeights.Bold;
        }

        private void ShowBackupsPage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageBackups);
            BackupsButton.FontWeight = FontWeights.Bold;
        }

        private void ShowArchivesPage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageArchives);
            ArchivesButton.FontWeight = FontWeights.Bold;
        }

        private void ShowAdvancedPage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageAdvanced);
            AdvancedButton.FontWeight = FontWeights.Bold;
        }

        private void Exit(object sender, RoutedEventArgs args)
        {
            Application.Current.Shutdown();
        }

    }

}