using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Application = System.Windows.Application;

namespace SmModManager.Graphics
{

    public partial class WnManager
    {

        public static WnManager GetWnManager;
        public static Vector2 PreviousPosition;
        public bool UpdateTopMenuBool = true;
        public bool CanNavigate = true;
        public bool CanExit = true;

        public WnManager()
        {
            Debug.WriteLine(App.Settings.StartUpX);
            Debug.WriteLine(App.Settings.StartUpY);
            Width = App.Settings.StartUpX;
            Height = App.Settings.StartUpY;
            Top = (Screen.PrimaryScreen.Bounds.Height - Height) / 2;
            Left = (Screen.PrimaryScreen.Bounds.Width - Width) / 2;
            this.WindowState = App.Settings.StartupMode;
            InitializeComponent();
            GetWnManager = this;
            IsWindowOpen = true;
        }

        public static bool IsWindowOpen { get; private set; }

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
            try
            {
                var messageExtend = Math.Clamp((Message.Length / 25) - 1, 0, 99999);
                int lines = Message.Split(Environment.NewLine).Length-1 + messageExtend;
                var sb = FindResource("MessagePopup") as Storyboard;
                sb.Stop();
                sb.Begin();
                NotificationBox.Width = Math.Clamp(WindowWidth, 200, 600);
                NotificationBox.Height = Math.Clamp(WindowHeight, 50, 600) + (lines*10);
                NotificationMessage.Text = Message;
                NotificationBox.Visibility = Visibility.Visible;
            }
            catch
            {
                // nothing
            }
        }
        public void ClearNotification(object sender, EventArgs e)
        {
            var sb = FindResource("MessagePopup") as Storyboard;
            var time = (long)sb.GetCurrentTime().TotalMilliseconds;
            var NewTime = Math.Abs(time - 1000) + 1000;
            var percent = ((float)NewTime / 1000) - 1;
            long val = 50000000 + (int)(10000000 * percent);
            if (time < 1000)
                sb.Seek(new TimeSpan(val));
            else if (time < 3000)
                sb.Seek(new TimeSpan(50000000));
        }

        public void SetInVisible(object sender, EventArgs e)
        {
            NotificationBox.Visibility = Visibility.Hidden;
            UpdateTopMenuBool = true;
        }


        public void RunVoidList(object sender, SizeChangedEventArgs e)
        {
            if(Width < 700)
                Width = 700;
            if (Height < 500)
                Height = 500;
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
                PageView.Width = Math.Clamp((float)App.WindowManager.ActualWidth - 15, 10, 999999);
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
            CommunityButton.FontWeight = FontWeights.Normal;
            CollectionsButton.FontWeight = FontWeights.Normal;

            ManageButton.FontSize = 12;
            MultiplayerButton.FontSize = 12;
            BackupsButton.FontSize = 12;
            AdvancedButton.FontSize = 12;
            StoreButton.FontSize = 12;
            HomeButton.FontSize = 12;
            CommunityButton.FontSize = 12;
            CollectionsButton.FontSize = 12;
        }

        public void ShowManagePage(object sender, RoutedEventArgs args)
        {
            if (CanNavigate)
            {
                ClearButtonFontWeight();
                PageView.Navigate(App.PageManage);
                ManageButton.FontWeight = FontWeights.Bold;
                ManageButton.FontSize = 15;
            }
        }

        public void ShowMultiplayerPage(object sender, RoutedEventArgs args)
        {
            if (CanNavigate)
            {
                Dispatcher.Invoke(() =>
                {
                    ClearButtonFontWeight();
                    PageView.Navigate(App.PageMultiplayer);
                    MultiplayerButton.FontWeight = FontWeights.Bold;
                    MultiplayerButton.FontSize = 15;
                });
            }
        }

        private void ShowBackupsPage(object sender, RoutedEventArgs args)
        {
            if (CanNavigate)
            {
                ClearButtonFontWeight();
                PageView.Navigate(App.PageBackups);
                BackupsButton.FontWeight = FontWeights.Bold;
                BackupsButton.FontSize = 15;
            }
        }

        public void ShowHomePage(object sender, RoutedEventArgs args)
        {
            if (CanNavigate)
            {
                ClearButtonFontWeight();
                PageView.Navigate(App.PageHome);
                HomeButton.FontWeight = FontWeights.Bold;
                HomeButton.FontSize = 15;
            }
        }

        private void ShowStorePage(object sender, RoutedEventArgs args)
        {
            if (CanNavigate)
            {
                ClearButtonFontWeight();
                PageView.Navigate(App.PageStore);
                StoreButton.FontWeight = FontWeights.Bold;
                StoreButton.FontSize = 15;
            }
        }
        
        private void ShowAdvancedPage(object sender, RoutedEventArgs args)
        {
            if (CanNavigate)
            {
                ClearButtonFontWeight();
                PageView.Navigate(App.PageAdvanced);
                AdvancedButton.FontWeight = FontWeights.Bold;
                AdvancedButton.FontSize = 15;
            }
        }
        private void ShowCommunityPage(object sender, RoutedEventArgs args)
        {
            if (CanNavigate)
            {
                ClearButtonFontWeight();
                PageView.Navigate(App.CommunityPage);
                CommunityButton.FontWeight = FontWeights.Bold;
                CommunityButton.FontSize = 15;
            }
        }

        public void ShowCollectionsPage(object sender, RoutedEventArgs args)
        {
            if (CanNavigate)
            {
            }
        }
        private void Exit(object sender, RoutedEventArgs args)
        {
            if (CanNavigate || CanExit)
            {
                Application.Current.Shutdown();
            }
        }
        public void ShowFormatLoading()
        {
            ClearButtonFontWeight();
            App.FormatLoadingPage.UpdateStatus();
            PageView.Navigate(App.FormatLoadingPage);
        }

        public void MinimizeWindow()
        {
            WindowState = (WindowState)FormWindowState.Minimized;
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

        public static readonly DependencyProperty DisableProperty =
            DependencyProperty.RegisterAttached("Disable", typeof(bool), typeof(DisableNavigation),
                new PropertyMetadata(false, DisableChanged));

        public static bool GetDisable(DependencyObject o)
        {
            return (bool)o.GetValue(DisableProperty);
        }

        public static void SetDisable(DependencyObject o, bool value)
        {
            o.SetValue(DisableProperty, value);
        }


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