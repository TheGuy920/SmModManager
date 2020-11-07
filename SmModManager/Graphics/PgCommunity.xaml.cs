using CefSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmModManager.Graphics
{
    /// <summary>
    /// Interaction logic for PgCommunity.xaml
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
