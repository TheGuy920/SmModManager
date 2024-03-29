﻿// Copyright © 2013 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using SmModManager;
using SmModManager.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;

namespace CefSharp.Handlers
{
    public class DownloadHandler : IDownloadHandler
    {
        public event EventHandler<DownloadItem> OnBeforeDownloadFired;

        public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            OnBeforeDownloadFired?.Invoke(this, downloadItem);
            Debug.WriteLine("Download Triggered: " + downloadItem);
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    downloadItem.FullPath = Path.Combine(Constants.CachePath, downloadItem.SuggestedFileName);
                    callback.Continue(downloadItem.FullPath, showDialog: false);
                }
            }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            OnDownloadUpdatedFired?.Invoke(this, downloadItem);
            if (downloadItem.IsComplete)
            {
                App.GetApp.NewModAdded(chromiumWebBrowser);
            }
        }
    }
}