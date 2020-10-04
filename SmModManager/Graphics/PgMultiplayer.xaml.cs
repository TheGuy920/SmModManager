using CefSharp;
using CefSharp.Wpf;
using SmModManager.Core;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Net;
using System.Windows.Controls;

namespace SmModManager.Graphics
{
    public class JavascriptManager : ILoadHandler, IRenderProcessMessageHandler
    {
        public static string InjectionData = System.IO.File.ReadAllText(System.IO.Path.Combine(Constants.Resources, "Assets", "SiteManager.js"));
        public string injection = InjectionData;

        public JavascriptManager(ChromiumWebBrowser browser)
        {
            browser.LoadHandler = this;
            browser.RenderProcessMessageHandler = this;
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

        public void OnUncaughtException(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, JavascriptException exception)
        {
            //throw new NotImplementedException();
        }
    }
    public class JsDialogHandler : IJsDialogHandler
    {
        public bool OnJSDialog(IWebBrowser browserControl, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
        {
            if(messageText.StartsWith("JoinUser: "))
            {
                JoinFriend.GetJoinFriend.CloseWindow();
                PgMultiplayer.GetPgMultiplayer.SteamUserId = messageText.Replace("JoinUser: ", "");
                new JoinFriend().Show();
            }
            callback.Continue(true);
            return true;
        }

        public bool OnJSBeforeUnload(IWebBrowser browserControl, IBrowser browser, string message, bool isReload, IJsDialogCallback callback)
        {
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
        public string SteamUserId = "";
        public JavascriptManager jsmanager;
        public static PgMultiplayer GetPgMultiplayer;
        public PgMultiplayer()
        {
            GetPgMultiplayer = this;
            InitializeComponent();
            Setup();
            HomePageSite.Address = "https://steamcommunity.com/friends";
            new JoinFriend();
        }

        public void GotoAddress(string address)
        {
            HomePageSite.Address = address;
        }
        public void Setup()
        {
            HomePageSite.Address = "https://steamcommunity.com/friends";
            HomePageSite.DownloadHandler = new CefSharp.Handlers.DownloadHandler();
            InvisiblePage.DownloadHandler = new CefSharp.Handlers.DownloadHandler();
            HomePageSite.MenuHandler = new CefSharp.Handlers.MenuHandler();
            var requestContextSettings = new RequestContextSettings { CachePath = System.IO.Path.Combine(Constants.CachePath, "UserDataCache") };
            requestContextSettings.PersistSessionCookies = true;
            requestContextSettings.PersistUserPreferences = true;
            var requestContext = new RequestContext(requestContextSettings);
            HomePageSite.RequestContext = requestContext;
            InvisiblePage.RequestContext = requestContext;
            InvisiblePage.LifeSpanHandler = new BrowserLifeSpanHandler();
            HomePageSite.LifeSpanHandler = new BrowserLifeSpanHandler();
            jsmanager = new JavascriptManager(InvisiblePage);
            jsmanager = new JavascriptManager(HomePageSite);
            JsDialogHandler jss = new JsDialogHandler();
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
            App.PageJoinFriend.RefreshCurrentModsStatus();
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
                item.Foreground = System.Windows.Media.Brushes.Black;
            }
            if (args.RemovedItems.Count > 0 && args.RemovedItems[0].GetType() == typeof(TabItem))
            {
                var item = (TabItem)args.RemovedItems[0];
                item.Foreground = System.Windows.Media.Brushes.White;
            }
        }
    }

}