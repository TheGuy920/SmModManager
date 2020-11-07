using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using CefSharp;
using SmModManager.Core;
using SmModManager.Core.Handlers;

namespace SmModManager.Graphics
{

    public partial class PgHome
    {

        public static PgHome GetPgHome;
        public Thread CheckLoggedIn;
        public bool IsLogedIn;
        public bool isMenuShown;
        public string ModManagerGroupChat = "steam://friends/joinchat/103582791468534120";
        public bool ThreadCanRun = true;

        public PgHome()
        {
            GetPgHome = this;
            InitializeComponent();
            CheckLoggedIn = new Thread(GetPgHome.CheckIfLoggedIn)
            {
                IsBackground = true
            };
            SetUp();
        }

        private void PlayGame(object sender, RoutedEventArgs args)
        {
            var startArgs = "";
            if (App.Settings.VerMode)
                startArgs += "";
            if (App.Settings.DevMode)
                startArgs += "-dev%20";
            if (App.Settings.WindowMode)
                startArgs += "-window%20";
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Utilities.GetSteamLocation(), "steam.exe"),
                Arguments = @"steam://run/387990//" + startArgs
            };
            Process.Start(startInfo);
            WnManager.GetWnManager.MinimizeWindow();
        }

        public void SetUp()
        {
            HomePageSite.Address = "https://steamcommunity.com/login/home/?goto=";
            HomePageSite.MenuHandler = new MenuHandler();
            CheckLoggedIn.Start();
            if (App.Settings.DevMode)
                DevModeImage.Visibility = Visibility.Visible;
            else
                DevModeImage.Visibility = Visibility.Hidden;
            if (App.Settings.WindowMode)
                RunWindowedImage.Visibility = Visibility.Visible;
            else
                RunWindowedImage.Visibility = Visibility.Hidden;
        }

        public void CheckIfLoggedIn()
        {
            var count = 0;
            try
            {
                while (!IsLogedIn && ThreadCanRun)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (HomePageSite.Address != "https://steamcommunity.com/login/home/?goto=")
                        {
                            using (var client = new WebClient())
                            {
                                var htmlCode = client.DownloadString(HomePageSite.Address);
                                var sub1 = htmlCode.Substring(htmlCode.IndexOf("steamid\":\""), 30);
                                var SteamId = sub1[10..sub1.IndexOf("\",\"")];
                                App.UserSteamId = SteamId;
                            }
                            IsLogedIn = true;
                            HomePageSite.Load("https://steamcommunity.com/app/387990");
                            PgMultiplayer.GetPgMultiplayer.GoHome(null, null);
                        }
                    });
                    if (count < 12)
                        if (App.HasFormattedAllMods)
                        {
                            if (count == 0)
                                if (!App.Settings.HasTakenTutorial)
                                    Dispatcher.Invoke(() => { WnManager.GetWnManager.Notification("Looks like your new!\nHead over to Advanced section and check out the tutorial!"); });
                            count++;
                        }
                        else if (count == 12)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                WnManager.GetWnManager.Notification("Sign-in is NOT required\nbut it is recomended");
                                count = 13;
                            });
                        }
                    Thread.Sleep(500);
                }
            }
            catch
            {
            }
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
            HomePageSite.Address = "https://steamcommunity.com/app/387990";
        }

        public void ShowMenu(object sender, RoutedEventArgs args)
        {
            isMenuShown = !isMenuShown;
            if (isMenuShown)
                GridMenu.Visibility = Visibility.Visible;
            else
                GridMenu.Visibility = Visibility.Hidden;
        }

        public void UpdateUrl()
        {
            if (CurrentUrl.Text != HomePageSite.Address)
                CurrentUrl.Text = HomePageSite.Address;
        }

        public void VerifyFiles(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show("Are you sure that you want to Verify you game files?\nThis will revert your game files back to their original state!", "Verify Files", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Utilities.GetSteamLocation(), "steam.exe"),
                Arguments = @"steam://validate/387990/"
            };
            Process.Start(startInfo);
            ShowMenu(null, null);
        }

        public void ToggleVer(object sender, RoutedEventArgs args)
        {
            App.Settings.VerMode = !App.Settings.VerMode;
        }

        public void ToggleWindowMode(object sender, RoutedEventArgs args)
        {
            App.Settings.WindowMode = !App.Settings.WindowMode;
            App.Settings.Save();
            if (App.Settings.WindowMode)
                RunWindowedImage.Visibility = Visibility.Visible;
            else
                RunWindowedImage.Visibility = Visibility.Hidden;
        }

        public void ToggleDevMode(object sender, RoutedEventArgs args)
        {
            App.Settings.DevMode = !App.Settings.DevMode;
            App.Settings.Save();
            if (App.Settings.DevMode)
                DevModeImage.Visibility = Visibility.Visible;
            else
                DevModeImage.Visibility = Visibility.Hidden;
        }

        private void JoinGroup(object sender, RoutedEventArgs args)
        {
            HomePageSite.Address = "https://s.team/chat/edHZfF8D";
            ShowMenu(null, null);
        }

    }

}