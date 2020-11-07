using System.Windows;
using System.Windows.Controls;
using CefSharp;

namespace SmModManager.Graphics
{

    /// <summary>
    ///     Interaction logic for PgCommunity.xaml
    /// </summary>
    public partial class PgCommunity : Page
    {

        public PgCommunity()
        {
            InitializeComponent();
        }

        public void MoveForward(object sender, RoutedEventArgs args)
        {
            if (HomePageSite.CanGoForward)
                HomePageSite.Forward();
        }

        public void MoveBackward(object sender, RoutedEventArgs args)
        {
            if (HomePageSite.CanGoBack)
                HomePageSite.Back();
        }

        public void GoHome(object sender, RoutedEventArgs args)
        {
            HomePageSite.Address = "https://smmodmanager.com/forum/";
        }

        public void UpdateUrl(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (CurrentUrl.Text != HomePageSite.Address)
                CurrentUrl.Text = HomePageSite.Address;
        }

    }

}