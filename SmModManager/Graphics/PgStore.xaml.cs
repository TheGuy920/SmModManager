﻿using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using SmModManager.Core;
using SmModManager.Core.Bindings;
using SmModManager.Core.Enums;
using SmModManager.Core.Models;

namespace SmModManager.Graphics
{

    public partial class PgStore
    {
        public static PgStore getPgStore;
        public PgStore()
        {
            getPgStore = this;
            InitializeComponent();
            WorkShop.DownloadHandler = new CefSharp.Handlers.DownloadHandler();
            SmModManager_1.DownloadHandler = new CefSharp.Handlers.DownloadHandler();
            SmModManager_2.DownloadHandler = new CefSharp.Handlers.DownloadHandler();
            var requestContextSettings = new RequestContextSettings { CachePath = Path.Combine(Constants.CachePath, "UserDataCache") };
            requestContextSettings.PersistSessionCookies = true;
            requestContextSettings.PersistUserPreferences = true;
            var requestContext = new RequestContext(requestContextSettings);
            WorkShop.RequestContext = requestContext;
            SmModManager_1.RequestContext = requestContext;
            SmModManager_2.RequestContext = requestContext;
            WorkShop.MenuHandler = new CefSharp.Handlers.MenuHandler();
            SmModManager_1.MenuHandler = new CefSharp.Handlers.MenuHandler();
            SmModManager_2.MenuHandler = new CefSharp.Handlers.MenuHandler();
        }
        public void MoveForward(object sender, RoutedEventArgs args)
        {
            if (WS.IsSelected)
                if (WorkShop.CanGoForward)
                    WorkShop.Forward();
            if (SMMT.IsSelected)
                if (SmModManager_1.CanGoForward)
                    SmModManager_1.Forward();
            if (SMMD.IsSelected)
                if (SmModManager_2.CanGoForward)
                    SmModManager_2.Forward();
        }
        public void MoveBackward(object sender, RoutedEventArgs args)
        {
            if (WS.IsSelected)
                if (WorkShop.CanGoBack)
                    WorkShop.Back();
            if (SMMT.IsSelected)
                if (SmModManager_1.CanGoBack)
                    SmModManager_1.Back();
            if (SMMD.IsSelected)
                if (SmModManager_2.CanGoBack)
                    SmModManager_2.Back();
        }
        public void GoHome(object sender, RoutedEventArgs args)
        {
            if (WS.IsSelected)
                WorkShop.Address = "https://steamcommunity.com/app/387990/workshop/";
            if (SMMT.IsSelected)
                SmModManager_1.Address = "https://smmods.com/";
            if (SMMD.IsSelected)
                SmModManager_2.Address = "https://scrapmechanicmods.com/";
        }

        public void UpdateUrl()
        {
            if (WS.IsSelected && WS_CurrentUrl.Text != WorkShop.Address)
                WS_CurrentUrl.Text = WorkShop.Address;
            if (SMMT.IsSelected && SMMT_CurrentUrl.Text != SmModManager_1.Address)
                SMMT_CurrentUrl.Text = SmModManager_1.Address;
            if (SMMD.IsSelected && SMMD_CurrentUrl.Text != SmModManager_2.Address)
                SMMD_CurrentUrl.Text = SmModManager_2.Address;

            if (WS.IsSelected)
            {
                WS.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                WS.Foreground = new SolidColorBrush(Colors.White);
            }
            if (SMMT.IsSelected)
            {
                SMMT.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                SMMT.Foreground = new SolidColorBrush(Colors.White);
            }
            if (SMMD.IsSelected)
            {
                SMMD.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                SMMD.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

}