using SmModManager.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace SmModManager.Graphics
{
    public partial class WnManager
    {
        public static WnManager GetWnManager;
        public static Vector2 PreviousPosition;
        readonly Thread FixedUpdateThread;
        public bool UpdateTopMenuBool = true;
        public static bool IsWindowOpen { get; private set; }
        public WnManager()
        {
            InitializeComponent();
            GetWnManager = this;
            FixedUpdateThread = new Thread(Updater);
            FixedUpdateThread.IsBackground = true;
            FixedUpdateThread.Start();
            IsWindowOpen = true;
        }
        public void AppClosing(object sender, CancelEventArgs e)
        {
            App.IsClosing = true;
            PgHome.GetPgHome.ThreadCanRun = false;
            Environment.Exit(Environment.ExitCode);
        }
        public void SendNotification(string message)
        {
            Dispatcher.Invoke(() =>
            {
                Notification(message);
            });
        }
        public void Notification(string Message, float WindowHeight = 50, float WindowWidth = 200)
        {
            Storyboard sb = FindResource("MessagePopup") as Storyboard;
            sb.Stop();
            sb.Begin();
            NotificationBox.Width = Math.Clamp(WindowWidth, 200, 600);
            NotificationBox.Height = Math.Clamp(WindowHeight, 50, 600);
            NotificationMessage.Text = Message;
            NotificationBox.Visibility = Visibility.Visible;
        }
        public void ClearNotification(object sender, EventArgs e)
        {
            Storyboard sb = this.FindResource("MessagePopup") as Storyboard;
            sb.Seek(new TimeSpan(50000000));
        }
        public void SetInVisible(object sender, EventArgs e)
        {
            NotificationBox.Visibility = Visibility.Hidden;
            UpdateTopMenuBool = true;
        }
        public void Updater()
        {
            //call methods to be updated every 20 ms
            try
            {
                while (IsWindowOpen)
                {
                    Thread.Sleep(20);
                    Dispatcher.Invoke(() =>
                    {
                        RunVoidList();
                    });
                }
            }
            catch{ }
        }
        public void RunVoidList()
        {
            PgManage.GetPgManage.UpdatePreviewImages();
            UpdateTopMenu();
            PgStore.getPgStore.UpdateUrl();
            PgHome.GetPgHome.UpdateUrl();
        }
        public void UpdateTopMenu()
        {
            if (PreviousPosition.X != App.WindowManager.ActualHeight || PreviousPosition.Y != App.WindowManager.ActualWidth)
            {
                PgMultiplayer.GetPgMultiplayer.UpdateStuff();
                PgAdvanced.GetPgAdvanced.UpdateTextBoxSize();
                if (UpdateTopMenuBool)
                    TopMenuPanel.Width = App.WindowManager.ActualWidth;
                PageView.Width = App.WindowManager.ActualWidth - 15;
                PreviousPosition.X = (float)App.WindowManager.ActualHeight;
                PreviousPosition.Y = (float)App.WindowManager.ActualWidth;
                PageView.Height = Math.Clamp((float)App.WindowManager.ActualHeight - 89, 10, 999999);
                PageView.Width = Math.Clamp((float)App.WindowManager.ActualWidth-15, 10, 999999);
            }

        }
        private void ClearButtonFontWeight()
        {
            ManageButton.FontWeight = FontWeights.Normal;
            MultiplayerButton.FontWeight = FontWeights.Normal;
            BackupsButton.FontWeight = FontWeights.Normal;
            AdvancedButton.FontWeight = FontWeights.Normal;
            StoreButton.FontWeight = FontWeights.Normal;
            HomeButton.FontWeight = FontWeights.Normal;

            ManageButton.FontSize = 12;
            MultiplayerButton.FontSize = 12;
            BackupsButton.FontSize = 12;
            AdvancedButton.FontSize = 12;
            StoreButton.FontSize = 12;
            HomeButton.FontSize = 12;
        }

        public void ShowManagePage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageManage);
            ManageButton.FontWeight = FontWeights.Bold;
            ManageButton.FontSize = 15;
        }

        private void ShowMultiplayerPage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageMultiplayer);
            MultiplayerButton.FontWeight = FontWeights.Bold;
            MultiplayerButton.FontSize = 15;
        }

        private void ShowBackupsPage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageBackups);
            BackupsButton.FontWeight = FontWeights.Bold;
            BackupsButton.FontSize = 15;
        }
        public void ShowHomePage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageHome);
            HomeButton.FontWeight = FontWeights.Bold;
            HomeButton.FontSize = 15;
        }
        private void ShowStorePage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageStore);
            StoreButton.FontWeight = FontWeights.Bold;
            StoreButton.FontSize = 15;
        }

        private void ShowAdvancedPage(object sender, RoutedEventArgs args)
        {
            ClearButtonFontWeight();
            PageView.Navigate(App.PageAdvanced);
            AdvancedButton.FontWeight = FontWeights.Bold;
            AdvancedButton.FontSize = 15;
        }

        private void Exit(object sender, RoutedEventArgs args)
        {
            System.Windows.Application.Current.Shutdown();
        }

        public void MinimizeWindow()
        {
            this.WindowState = (WindowState)FormWindowState.Minimized;
        }        
        public void CallMinimizeWindow()
        {
            Dispatcher.Invoke(() =>
            {
                MinimizeWindow();
            });
        }
    }
    public static class DisableNavigation
    {
        public static bool GetDisable(DependencyObject o)
        {
            return (bool)o.GetValue(DisableProperty);
        }
        public static void SetDisable(DependencyObject o, bool value)
        {
            o.SetValue(DisableProperty, value);
        }

        public static readonly DependencyProperty DisableProperty =
            DependencyProperty.RegisterAttached("Disable", typeof(bool), typeof(DisableNavigation),
                                                new PropertyMetadata(false, DisableChanged));



        public static void DisableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var frame = (Frame)sender;
            frame.Navigated += DontNavigate;
            frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
        }

        public static void DontNavigate(object sender, NavigationEventArgs e)
        {
            ((Frame)sender).NavigationService.RemoveBackEntry();
        }
    }
}