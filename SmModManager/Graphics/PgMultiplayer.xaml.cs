using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CefSharp;
using CefSharp.Handlers;
using CefSharp.Wpf;
using SmModManager.Core;

namespace SmModManager.Graphics
{

    public class JavascriptManager : ILoadHandler, IRenderProcessMessageHandler
    {

        public static string InjectionData = File.ReadAllText(Path.Combine(Constants.Resources, "Assets", "SiteManager.js"));
        public string injection = InjectionData;

        public JavascriptManager(ChromiumWebBrowser browser)
        {
            browser.LoadHandler = this;
            browser.RenderProcessMessageHandler = this;
        }

        public void OnFrameLoadEnd(IWebBrowser chromiumWebBrowser, FrameLoadEndEventArgs frameLoadEndArgs)
        {
            //throw new NotImplementedException();
        }

        public void OnFrameLoadStart(IWebBrowser chromiumWebBrowser, FrameLoadStartEventArgs frameLoadStartArgs)
        {
            //throw new NotImplementedException();
        }

        public void OnLoadError(IWebBrowser chromiumWebBrowser, LoadErrorEventArgs loadErrorArgs)
        {
            //throw new NotImplementedException();
        }

        public void OnLoadingStateChange(IWebBrowser chromiumWebBrowser, LoadingStateChangedEventArgs loadingStateChangedArgs)
        {
            //throw new NotImplementedException();
        }

        public void OnContextCreated(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            frame.ExecuteJavaScriptAsync(injection);
        }

        public void OnContextReleased(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
            //throw new NotImplementedException();
        }

        public void OnFocusedNodeChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IDomNode node)
        {
            //throw new NotImplementedException();
        }

        public void OnUncaughtException(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, JavascriptException exception)
        {
            //throw new NotImplementedException();
        }

    }

    public class JsDialogHandler : IJsDialogHandler
    {

        public bool OnJSDialog(IWebBrowser browserControl, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
        {
            if (messageText.StartsWith("JoinUser: "))
            {
                WnJoinFriend.GetWnJoinFriend.CloseWindow();
                PgMultiplayer.GetPgMultiplayer.SteamUserId = messageText.Replace("JoinUser: ", "");
                new WnJoinFriend().Show();
            }
            callback.Continue(true);
            return true;
        }

        public void OnResetDialogState(IWebBrowser browserControl, IBrowser browser)
        {
            //throw new NotImplementedException();
        }

        public void OnDialogClosed(IWebBrowser browserControl, IBrowser browser)
        {
            //throw new NotImplementedException();
        }

        public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback)
        {
            throw new NotImplementedException();
        }

        public bool OnJSBeforeUnload(IWebBrowser browserControl, IBrowser browser, string message, bool isReload, IJsDialogCallback callback)
        {
            return true;
        }

    }

    public class BrowserLifeSpanHandler : ILifeSpanHandler
    {

        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName,
            WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo,
            IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            return true;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {
            //throw new NotImplementedException();
        }

        public bool DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            return false;
        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {
            //throw new NotImplementedException();
        }

    }

    public partial class PgMultiplayer
    {

        public static PgMultiplayer GetPgMultiplayer;
        public JavascriptManager jsmanager;
        public string SteamUserId = "";

        public PgMultiplayer()
        {
            GetPgMultiplayer = this;
            InitializeComponent();
            Setup();
            HomePageSite.Address = "https://steamcommunity.com/friends";
            new WnJoinFriend();
        }

        public void GotoAddress(string address)
        {
            HomePageSite.Address = address;
        }

        public void Setup()
        {
            HomePageSite.Address = "https://steamcommunity.com/friends";
            HomePageSite.DownloadHandler = new DownloadHandler();
            InvisiblePage.DownloadHandler = new DownloadHandler();
            HomePageSite.MenuHandler = new MenuHandler();
            var requestContextSettings = new RequestContextSettings { CachePath = Path.Combine(Constants.CachePath, "UserDataCache") };
            requestContextSettings.PersistSessionCookies = true;
            requestContextSettings.PersistUserPreferences = true;
            var requestContext = new RequestContext(requestContextSettings);
            HomePageSite.RequestContext = requestContext;
            InvisiblePage.RequestContext = requestContext;
            InvisiblePage.LifeSpanHandler = new BrowserLifeSpanHandler();
            HomePageSite.LifeSpanHandler = new BrowserLifeSpanHandler();
            jsmanager = new JavascriptManager(InvisiblePage);
            jsmanager = new JavascriptManager(HomePageSite);
            var jss = new JsDialogHandler();
            InvisiblePage.JsDialogHandler = jss;
            HomePageSite.JsDialogHandler = jss;
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
            HomePageSite.Address = "https://steamcommunity.com/friends";
        }

        public void AddressChangedUpdate(object sender, DependencyPropertyChangedEventArgs e)
        {
            CurrentUrl.Text = HomePageSite.Address;
        }

        public void UpdateOnlineStatus(object sender, RoutedEventArgs args)
        {
            App.PageWnJoinFriend.RefreshCurrentModsStatus();
        }

        public void UpdateStuff()
        {
            FriendsDock.Height = Math.Clamp(App.WindowManager.ActualHeight - 118, 50, 9999);
        }

        public void UpdateTabSelection(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0].GetType() == typeof(TabItem))
            {
                var item = (TabItem)args.AddedItems[0];
                item.Foreground = Brushes.Black;
            }
            if (args.RemovedItems.Count > 0 && args.RemovedItems[0].GetType() == typeof(TabItem))
            {
                var item = (TabItem)args.RemovedItems[0];
                item.Foreground = Brushes.White;
            }
        }

    }

}