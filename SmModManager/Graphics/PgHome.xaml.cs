using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using CefSharp;
using CefSharp.Handlers;
using SmModManager.Core;

namespace SmModManager.Graphics
{

    public partial class PgHome
    {

        public static PgHome GetPgHome;
        public Thread CheckLoggedIn;
        public bool IsLogedIn;
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
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Utilities.GetSteamLocation(), "steam.exe"),
                Arguments = @"steam://run/387990//"
            };
            Process.Start(startInfo);
            WnManager.GetWnManager.MinimizeWindow();
        }

        public void SetUp()
        {
            HomePageSite.Address = "https://steamcommunity.com/login/home/?goto=";
            HomePageSite.MenuHandler = new MenuHandler();
            CheckLoggedIn.Start();
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
                    if (count < 60)
                        count++;
                    else if (count == 60)
                        Dispatcher.Invoke(() =>
                        {
                            WnManager.GetWnManager.Notification("Sign-in is NOT required\nbut it is recomended");
                            count = 61;
                        });
                    Thread.Sleep(100);
                }
            }
            catch { }
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

        public void UpdateUrl()
        {
            if (CurrentUrl.Text != HomePageSite.Address)
                CurrentUrl.Text = HomePageSite.Address;
        }

    }

}